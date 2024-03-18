using Microsoft.EntityFrameworkCore;
using OrderAPI.Models;

namespace OrderAPI.DbContext;

public class OrderDbContext: Microsoft.EntityFrameworkCore.DbContext  
{

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().ToTable("Orders");
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems");

        modelBuilder.Entity<OrderItem>()
            .HasOne(p => p.Order)
            .WithMany(b => b.OrderItems)
            .HasForeignKey(p => p.OrderId);
    }

}