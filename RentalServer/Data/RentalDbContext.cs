using Microsoft.EntityFrameworkCore;
using RentalServer.Models;

namespace RentalServer.Data;

public class RentalDbContext : DbContext
{
    public RentalDbContext(DbContextOptions<RentalDbContext> options) : base(options){}
    
    public DbSet<Rental> Rentals => Set<Rental>();
}