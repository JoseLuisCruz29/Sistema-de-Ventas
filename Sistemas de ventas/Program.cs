using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
    c.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v2"
    });
    // Configuración para token JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduzca 'Bearer' seguido de su token JWT."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "yourdomain.com",
            ValidAudience = "yourdomain.com",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("vainitaOMGclavelargaysegura_a234243423423awda"))
        };
    });

builder.Services.AddAuthorization();

// Función para generar el JWT
string GenerateJwtToken()
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, "test"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("User","Mi usuario")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("vainitaOMGclavelargaysegura_a234243423423awda"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "yourdomain.com",
        audience: "yourdomain.com",
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "V2");
    c.SwaggerEndpoint("/swagger/v3/swagger.json", "V3");
});

app.UseHttpsRedirection();


// Endpoint de login para generar el JWT
app.MapPost("/login", async (AppDbContext dbContext, UserLogin user) =>
{
    var currentUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == user.Username);


    if (currentUser != null && currentUser.Password == user.Password)
    {
        // Generar el token JWT
        var token = GenerateJwtToken();
        return Results.Ok(new { token });
    }


    return Results.Unauthorized();
});

var orderDetails = new List<OrderDetail>();
var invoices = new List<Invoice>();
//CRUD Sencillo de Empleados
app.MapGet("/Employees", async (AppDbContext dbContext) =>
    await dbContext.Employees.ToListAsync()).RequireAuthorization();
app.MapGet("/Employees/{id}", async (AppDbContext dbContext, int id) =>
{
    var employee = await dbContext.Employees.FindAsync(id);
    return employee != null ? Results.Ok(employee) : Results.NotFound();
}).RequireAuthorization();
app.MapPost("/Employees", async (AppDbContext dbContext, Employee newEmployee) =>
{
    dbContext.Employees.Add(newEmployee);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/Employees/{newEmployee.Id}", newEmployee);
}).RequireAuthorization();
app.MapPut("/Employees", async (AppDbContext dbContext, int id, Employee UpdatedEmployee) =>
{
    var employee = await dbContext.Employees.FindAsync(id);
    if (employee == null)
    {
        return Results.NotFound();
    }

    employee.Name = UpdatedEmployee.Name;
    employee.Position = UpdatedEmployee.Position;
    employee.Salary = UpdatedEmployee.Salary;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
app.MapDelete("/Employee/{id}", async (AppDbContext dbContext, int id) =>
{
    var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
    if (employee == null) return Results.NotFound();

    dbContext.Employees.Remove(employee);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
//CRUD Sencillo de Compañia
app.MapGet("/Companies", async (AppDbContext dbContext) =>
    await dbContext.Companies.ToListAsync()).RequireAuthorization();
app.MapGet("/Companies/{id}", async (AppDbContext dbContext, int id) =>
{
    var Company = await dbContext.Companies.FindAsync(id);
    return Company != null ? Results.Ok(Company) : Results.NotFound();
}).RequireAuthorization();
app.MapPost("/Companies", async (AppDbContext dbContext, Company newCompany) =>
{
    dbContext.Companies.Add(newCompany);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/Companies/{newCompany.Id}", newCompany);
}).RequireAuthorization();
app.MapPut("/Companies", async (AppDbContext dbContext, int id, Company UpdatedCompanies) =>
{
    var company = await dbContext.Companies.FindAsync(id);
    if (company == null)
    {
        return Results.NotFound();
    }

    company.Id = UpdatedCompanies.Id;
    company.Name = UpdatedCompanies.Name;


    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
app.MapDelete("/Companies/{id}", async (AppDbContext dbContext, int id) =>
{
    var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id);
    if (company == null) return Results.NotFound();

    dbContext.Companies.Remove(company);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
// --- CRUD de Artículos ---
app.MapGet("/articles", async (AppDbContext dbContext) =>
    await dbContext.Articles.ToListAsync()).RequireAuthorization();
app.MapGet("/articles/{id}", async (AppDbContext dbContext, int id) =>
{
    var article = await dbContext.Articles.FindAsync(id);
    return article != null ? Results.Ok(article) : Results.NotFound();
}).RequireAuthorization();
app.MapPost("/articles", async (AppDbContext dbContext, Article newArticle) =>
{
    dbContext.Articles.Add(newArticle);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/articles/{newArticle.Id}", newArticle);
}).RequireAuthorization();
app.MapPut("/articles/{id}", async (AppDbContext dbContext, int id, Article updatedArticle) =>
{
    var article = await dbContext.Articles.FindAsync(id);
    if (article == null) return Results.NotFound();

    article.Name = updatedArticle.Name;
    article.Price = updatedArticle.Price;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
app.MapDelete("/articles/{id}", async (AppDbContext dbContext, int id) =>
{
    var article = await dbContext.Articles.FirstOrDefaultAsync(a => a.Id == id);
    if (article == null) return Results.NotFound();

    dbContext.Articles.Remove(article);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
// --- CRUD de Órdenes ---
app.MapGet("/orders", async (AppDbContext dbContext) =>
    await dbContext.Orders.ToListAsync()).RequireAuthorization();
app.MapGet("/orders/{id}", async (AppDbContext dbContext, int id) =>
{
    var order = await dbContext.Orders.FindAsync(id);
    return order != null ? Results.Ok(order) : Results.NotFound();
}).RequireAuthorization();
app.MapPost("/orders", async (AppDbContext dbContext, Order newOrder) =>
{
    dbContext.Orders.Add(newOrder);
    newOrder.Status = "Pending";
    await dbContext.SaveChangesAsync();
    return Results.Created($"/orders/{newOrder.Id}", newOrder);
}).RequireAuthorization();
app.MapPut("/orders/{id}", async (AppDbContext dbContext, int id, Order updatedOrder) =>
{
    var order = await dbContext.Orders.FindAsync(id);
    if (order == null) return Results.NotFound();

    order.Description = updatedOrder.Description;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
app.MapDelete("/orders/{id}", async (AppDbContext dbContext, int id) =>
{
    var order = await dbContext.Orders.FirstOrDefaultAsync(a => a.Id == id);
    if (order == null) return Results.NotFound();

    if (order.Status == "Pending")
    {
        return Results.BadRequest("Cannot delete an order in 'Pending' status.");
    }

    dbContext.Orders.Remove(order);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// --- Agregar Artículos a Órdenes (OrderDetails) ---
app.MapPost("/orders/{orderId}/addArticle/{articleId}", async (AppDbContext dbContext, int orderId, int articleId) =>
{
    var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
    var article = await dbContext.Articles.FirstOrDefaultAsync(a => a.Id == articleId);

    if (order == null || article == null) return Results.NotFound("Order or Article not found.");

    var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Id == order.EmployeeId);
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
    await dbContext.OrderDetails.AddAsync(orderDetail);

    // Actualizar el valor total de la orden
    order.TotalValue += article.Price;
    await dbContext.SaveChangesAsync();
    return Results.Ok(orderDetail);
}).RequireAuthorization();

// --- Completar una Orden y Generar Factura ---
app.MapPut("/orders/{orderId}/complete", async (AppDbContext dbContext, int orderId) =>
{
    var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
    if (order == null) return Results.NotFound("Order not found.");

    var hasArticles = await dbContext.OrderDetails.AnyAsync(od => od.OrderId == orderId);
    if (!hasArticles) return Results.BadRequest("Cannot complete an order without articles.");

    order.Status = "Completed";

    var invoice = new Invoice
    {
        Id = invoices.Any() ? invoices.Max(i => i.Id) + 1 : 1,
        OrderId = order.Id,
        Status = "Pending",
        DeliveryDate = DateTime.Now.AddDays(7) // Fecha estimada de entrega
    };
    await dbContext.Invoices.AddAsync(invoice);
    await dbContext.SaveChangesAsync();
    return Results.Ok(invoice);
}).RequireAuthorization();

// --- CRUD de Facturas ---
app.MapGet("/invoices", async (AppDbContext dbContext) =>
    await dbContext.Invoices.ToListAsync()).RequireAuthorization();

app.MapGet("/invoices/{id}", async (AppDbContext dbContext, int id) =>
{
    var invoice = await dbContext.Invoices.FindAsync(id);
    return invoice != null ? Results.Ok(invoice) : Results.NotFound();
}).RequireAuthorization();
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
    public DateTime DeliveryDate { get; set; }
}
public class UserLogin
{
    public int id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

