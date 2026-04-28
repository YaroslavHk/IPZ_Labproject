using Microsoft.EntityFrameworkCore;
using RentalServer.Models;

namespace RentalServer.Data;

public class RentalDbContext : DbContext
{
    public RentalDbContext(DbContextOptions<RentalDbContext> options) : base(options){}
    
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RentalImage> RentalImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(u => u.UserId).ValueGeneratedOnAdd();
        modelBuilder.Entity<Rental>().Property(r => r.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<RentalImage>().Property(i => i.Id).ValueGeneratedOnAdd();
        base.OnModelCreating(modelBuilder);
    }
}