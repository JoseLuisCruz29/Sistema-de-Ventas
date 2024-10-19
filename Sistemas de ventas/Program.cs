var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Datos iniciales

// CRUD de empleados
var employees = new List<Employee> {
    new Employee {Id = 1, Name = "Jose Luis", Position = "Frontend", Salary = "28000USD", CompanyId = 1 },
    new Employee {Id = 2, Name = "Pedrito", Position = "Backend", Salary = "30000USD", CompanyId = 2 }
};

// CRUD de compañías
var companies = new List<Company> {
    new Company {Id = 1, Name = "Nacional"},
    new Company {Id = 2, Name = "El Bravo"}
};

// CRUD de artículos
var articles = new List<Article> {
    new Article {Id = 1, Name = "Coca Cola de Cristal", Price = 25, CompanyId = 1 },
    new Article {Id = 2, Name = "Pantalon", Price = 3000, CompanyId = 2 },
    new Article {Id = 3, Name = "Camisa", Price = 3500, CompanyId = 1 }
};

// CRUD de órdenes
var orders = new List<Order> {
    new Order {Id = 1, Description = "Order 1", EmployeeId = 1, TotalValue = 0, Status = "Pending" },
    new Order {Id = 2, Description = "Order 2", EmployeeId = 2, TotalValue = 0, Status = "Pending" }
};

// CRUD de detalles de órdenes
var orderDetails = new List<OrderDetail>();

// CRUD de facturas
var invoices = new List<Invoice>();

// --- CRUD de Artículos ---
app.MapGet("/articles", () => Results.Ok(articles));
app.MapGet("/articles/{id}", (int id) =>
{
    var article = articles.FirstOrDefault(a => a.Id == id);
    return article != null ? Results.Ok(article) : Results.NotFound();
});
app.MapPost("/articles", (Article newArticle) =>
{
    newArticle.Id = articles.Max(e => e.Id) + 1;
    articles.Add(newArticle);
    return Results.Created($"/articles/{newArticle.Id}", newArticle);
});
app.MapPut("/articles/{id}", (int id, Article updatedArticle) =>
{
    var article = articles.FirstOrDefault(a => a.Id == id);
    if (article == null) return Results.NotFound();

    article.Name = updatedArticle.Name;
    article.Price = updatedArticle.Price;

    return Results.NoContent();
});
app.MapDelete("/articles/{id}", (int id) =>
{
    var article = articles.FirstOrDefault(a => a.Id == id);
    if (article == null) return Results.NotFound();
    articles.Remove(article);
    return Results.NoContent();
});

// --- CRUD de Órdenes ---
app.MapGet("/orders", () => Results.Ok(orders));
app.MapGet("/orders/{id}", (int id) =>
{
    var order = orders.FirstOrDefault(o => o.Id == id);
    return order != null ? Results.Ok(order) : Results.NotFound();
});
app.MapPost("/orders", (Order newOrder) =>
{
    newOrder.Id = orders.Max(o => o.Id) + 1;
    newOrder.Status = "Pending"; // La orden comienza en estado pendiente
    orders.Add(newOrder);
    return Results.Created($"/orders/{newOrder.Id}", newOrder);
});
app.MapPut("/orders/{id}", (int id, Order updatedOrder) =>
{
    var order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    order.Description = updatedOrder.Description;

    return Results.NoContent();
});
app.MapDelete("/orders/{id}", (int id) =>
{
    var order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    if (order.Status == "Pending")
    {
        return Results.BadRequest("Cannot delete an order in 'Pending' status.");
    }

    orders.Remove(order);
    return Results.NoContent();
});

// --- Agregar Artículos a Órdenes (OrderDetails) ---
app.MapPost("/orders/{orderId}/addArticle/{articleId}", (int orderId, int articleId) =>
{
    var order = orders.FirstOrDefault(o => o.Id == orderId);
    var article = articles.FirstOrDefault(a => a.Id == articleId);

    if (order == null || article == null) return Results.NotFound("Order or Article not found.");

    var employee = employees.FirstOrDefault(e => e.Id == order.EmployeeId);
    if (employee == null || article.CompanyId != employee.CompanyId)
    {
        return Results.BadRequest("The article must belong to the same company as the employee.");
    }

    var orderDetail = new OrderDetail
    {
        Id = orderDetails.Any() ? orderDetails.Max(od => od.Id) + 1 : 1,
        OrderId = order.Id,
        ArticleId = article.Id
    };
    orderDetails.Add(orderDetail);

    // Actualizar el valor total de la orden
    order.TotalValue += article.Price;

    return Results.Ok(orderDetail);
});

// --- Completar una Orden y Generar Factura ---
app.MapPut("/orders/{orderId}/complete", (int orderId) =>
{
    var order = orders.FirstOrDefault(o => o.Id == orderId);
    if (order == null) return Results.NotFound("Order not found.");

    var hasArticles = orderDetails.Any(od => od.OrderId == orderId);
    if (!hasArticles) return Results.BadRequest("Cannot complete an order without articles.");

    order.Status = "Completed";

    var invoice = new Invoice
    {
        Id = invoices.Any() ? invoices.Max(i => i.Id) + 1 : 1,
        OrderId = order.Id,
        Status = "Pending",
        EstimatedDeliveryDate = DateTime.Now.AddDays(7) // Fecha estimada de entrega
    };
    invoices.Add(invoice);

    return Results.Ok(invoice);
});

// --- CRUD de Facturas ---
app.MapGet("/invoices", () => Results.Ok(invoices));
app.MapGet("/invoices/{id}", (int id) =>
{
    var invoice = invoices.FirstOrDefault(i => i.Id == id);
    return invoice != null ? Results.Ok(invoice) : Results.NotFound();
});
app.Run();
// --- Modelos ---
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public string Salary { get; set; }
    public int CompanyId { get; set; }
}

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Article
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int CompanyId { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string Description { get; set; }
    public int EmployeeId { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; }
}

public class OrderDetail
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int OrderId { get; set; }
}

public class Invoice
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
}


