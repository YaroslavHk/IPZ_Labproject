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
        group.MapGet("/",
            async (RentalDbContext db) =>
            {
                var rental = await db.Rentals.Select(r => new RentalResponse
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        Price = r.Price,
                        City = r.City,
                        Address = r.Address,
                        Type = r.Type,
                        LivingSpace = r.LivingSpace,
                        UserId = r.UserId,
                        Created = r.Created,
                        LastModified = r.LastModified

                    })
                    .ToListAsync<RentalResponse>();

                if (rental == null)
                {
                    return Results.NotFound(new
                    {
                        Message = "There is no Apartaments"
                    });
                }

                return Results.Ok(rental);
            });
        
// ЭНДПОИНТ GET: Получить одну конкретную квартиру по её ID
        group.MapGet("/{id}", async (int id, RentalDbContext db) => 
        {
            // Ищем квартиру в базе по ID
            var rental = await db.Rentals.FindAsync(id);
    
            // Если ничего не нашли, возвращаем статус 404 (Not Found)
            if (rental == null) 
            {
                return Results.NotFound(new { Message = "Apartament does not exist." });
            }
    
            // Если нашли, возвращаем статус 200 (Ok) и саму квартиру в формате JSON
            return Results.Ok(rental);
        });
        
        group.MapPost("/", async (Rental newRental, RentalDbContext db) => 
        {
            db.Rentals.Add(newRental);
            await db.SaveChangesAsync();
        
            return Results.Created($"/api/rentals/{newRental.Id}", newRental);
        });
    }
}