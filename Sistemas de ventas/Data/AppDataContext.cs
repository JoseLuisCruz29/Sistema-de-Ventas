using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<UserLogin> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>()
        .Property(a => a.Price)
        .HasPrecision(18, 2);
        modelBuilder.Entity<Order>()
        .Property(o => o.TotalValue)
        .HasPrecision(18, 2);
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserLogin>()
        .HasKey(u => u.id);
    }
};