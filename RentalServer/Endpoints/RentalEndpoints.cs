using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RentalServer.Data;
using RentalServer.DTO;
using RentalServer.Middlewares;
using RentalServer.Models;

namespace RentalServer.Endpoints;

public static class RentalEndpoints
{
    public static void MapRentalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rentals");
        
       group.MapGet("/", async (
    RentalDbContext db, 
    int page = 1, 
    int pageSize = 10,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    //List<int>? roomsCount = null,
    float? minSpace = null,
    float? maxSpace = null,
    string? type = null,
    string? sortBy = null,
    bool sortDesc = false) =>
{
    if (page < 1) page = 1;
    if (pageSize > 50) pageSize = 50; 
    
    var query = db.Rentals.Where(r => !r.IsHidden && !r.IsRentedOut);
    
    if (minPrice.HasValue) query = query.Where(r => r.Price >= minPrice.Value);
    if (maxPrice.HasValue) query = query.Where(r => r.Price <= maxPrice.Value);
   // if (roomsCount != null) query = query.Where(r => roomsCount.Contains(r.RoomCount));
    if (minSpace.HasValue) query = query.Where(r => r.LivingSpace >= minSpace.Value);
    if (maxSpace.HasValue) query = query.Where(r => r.LivingSpace <= maxSpace.Value);
    
    if (!string.IsNullOrWhiteSpace(type))
    {
        var typeLower = type.ToLower();
        query = query.Where(r => r.Type.ToLower() == typeLower);
    }

    // 3. Сортування (перед пагінацією)
    if (!string.IsNullOrEmpty(sortBy))
    {
        query = sortBy.ToLower() switch
        {
            "price" => sortDesc ? query.OrderByDescending(r => r.Price) : query.OrderBy(r => r.Price),
            "rooms" => sortDesc ? query.OrderByDescending(r => r.RoomCount) : query.OrderBy(r => r.RoomCount),
            "space" => sortDesc ? query.OrderByDescending(r => r.LivingSpace) : query.OrderBy(r => r.LivingSpace),
            _ => query.OrderByDescending(r => r.Created)
        };
    }
    else
    {
        query = query.OrderByDescending(r => r.Created);
    }

    // 4. Пагінація та вибірка даних
    var rentals = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(r => new ShortRentalResponse
        {
            Id = r.Id,
            Title = r.Title,
            Price = r.Price,
            City = r.City,
            Type = r.Type,
            Address = r.Address,
            RoomCount = r.RoomCount,
            LivingSpace = r.LivingSpace,
            UserId = r.UserId
        })
        .ToListAsync();

    return rentals.Any() ? Results.Ok(rentals) : Results.NotFound(new { Message = "No matches found." });
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
                RoomCount = request.RoomCount,
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