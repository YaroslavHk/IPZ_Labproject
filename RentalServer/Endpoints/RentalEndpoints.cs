using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // ОБЯЗАТЕЛЬНО добавить для работы с ClaimsPrincipal
using RentalServer.Data;
using RentalServer.DTO;
using RentalServer.Models;

namespace RentalServer.Endpoints;

public static class RentalEndpoints
{
    public static void MapRentalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rentals");
        
        // GET (Доступно всем) - просмотр всех объявлений
        group.MapGet("/", async (RentalDbContext db) =>
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
            }).ToListAsync();

            if (!rental.Any())
            {
                return Results.NotFound(new { Message = "There are no Apartments" });
            }

            return Results.Ok(rental);
        });
        
        // GET (Доступно всем) - просмотр конкретного объявления
        group.MapGet("/{id}", async (Guid id, RentalDbContext db) => 
        {
            var rental = await db.Rentals.FindAsync(id);
            if (rental == null) 
            {
                return Results.NotFound(new { Message = "Apartment does not exist." });
            }
            return Results.Ok(rental);
        });
        
        // POST (ТОЛЬКО ДЛЯ АВТОРИЗОВАННЫХ) - создание объявления
        group.MapPost("/", async (Rental newRental, RentalDbContext db, ClaimsPrincipal user) => 
        {
            // 1. Достаем ID пользователя из токена (тот самый ClaimTypes.NameIdentifier, который мы клали при логине)
            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Results.Unauthorized();
            }

            // 2. Принудительно присваиваем квартире ID того человека, чей токен мы получили
            newRental.UserId = userId;
            
            // 3. Проставляем даты (чтобы не зависеть от того, что прислали с фронта)
            newRental.Created = DateTime.UtcNow;
            newRental.LastModified = DateTime.UtcNow;
        
            db.Rentals.Add(newRental);
            await db.SaveChangesAsync();
        
            return Results.Created($"/api/rentals/{newRental.Id}", newRental);
        })
        .RequireAuthorization(); // <--- КЛЮЧЕВОЙ МОМЕНТ: эндпоинт требует токен!
    }
}