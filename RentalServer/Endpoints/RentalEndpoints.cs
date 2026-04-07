using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RentalServer.Data;
using RentalServer.DTO;
using RentalServer.Models;

namespace RentalServer.Endpoints;

public static class RentalEndpoints
{
    public static void MapRentalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rentals");

        group.MapGet("/", async (RentalDbContext db, int page = 1, int pageSize = 10) =>
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50; 

            var rentals = await db.Rentals
                .Where(r => !r.IsHidden && !r.IsRentedOut)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ShortRentalResponse
                {
                    Id = r.Id,
                    Title = r.Title,
                    Price = r.Price,
                    City = r.City,
                    Type = r.Type,
                    LivingSpace = r.LivingSpace,
                    UserId = r.UserId
                })
                .ToListAsync();

            if (!rentals.Any())
            {
                return Results.NotFound(new { Message = "No apartments found." });
            }

            return Results.Ok(rentals);
        });

        group.MapGet("/{id}", async (Guid id, RentalDbContext db) => 
        {
            var rental = await db.Rentals.FindAsync(id);
    
            if (rental == null) 
            {
                return Results.NotFound(new { Message = "Apartment does not exist." });
            }
    
            return Results.Ok(rental);
        });
        
        group.MapPost("/", async (RentalRequest request, RentalDbContext db, ClaimsPrincipal user) => 
        {
            var validator = new CreateRentalRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Results.Unauthorized();
            }

            var newRental = new Rental
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                City = request.City,
                Address = request.Address,
                Type = request.Type,
                LivingSpace = request.LivingSpace,
                UserId = userId,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                IsRentedOut = false,
                IsHidden = false
            };
        
            db.Rentals.Add(newRental);
            await db.SaveChangesAsync();

            var response = new RentalPostResponse
            {
                Id = newRental.Id
            };
        
            return Results.Created($"/api/rentals/{newRental.Id}", response);
        })
        .RequireAuthorization();
    }
}