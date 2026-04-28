using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RentalServer.Data;
using RentalServer.DTO;
using RentalServer.Middlewares;
using RentalServer.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace RentalServer.Endpoints;

public static class RentalEndpoints
{
    public static void MapRentalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rentals");
        
        // === 1. ПОЛУЧЕНИЕ СПИСКА (С ПАГИНАЦИЕЙ, ФИЛЬТРАМИ И ПОЛНЫМИ URL КАРТИНОК) ===
        group.MapGet("/", async (
            RentalDbContext db, 
            HttpContext context,
            int page = 1, 
            int pageSize = 10,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? roomsCount = null,
            float? minSpace = null,
            float? maxSpace = null,
            string? type = null,
            string? city = null,
            Guid? userId = null,
            string? sortBy = null,
            bool sortDesc = false) =>
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50; 
            
            // Получаем домен сервера (например, http://localhost:5177)
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            
            // Обязательно Include(r => r.Images), чтобы картинки подтянулись из базы
            var query = db.Rentals.Include(r => r.Images).Where(r => !r.IsHidden && !r.IsRentedOut);
            
            if (!userId.HasValue)
            {
                query = query.Where(r => !r.IsHidden && !r.IsRentedOut);
            }
            else
            {
                query = query.Where(r => r.UserId == userId.Value);
            }
            
            if (minPrice.HasValue) query = query.Where(r => r.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(r => r.Price <= maxPrice.Value);
            if (roomsCount.HasValue) query = query.Where(r => r.RoomCount == roomsCount.Value);
            if (minSpace.HasValue) query = query.Where(r => r.LivingSpace >= minSpace.Value);
            if (maxSpace.HasValue) query = query.Where(r => r.LivingSpace <= maxSpace.Value);
            
            if (!string.IsNullOrWhiteSpace(city)) // НОВОЕ
                query = query.Where(r => r.City.ToLower().Contains(city.ToLower()));

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(r => r.Type.ToLower() == type.ToLower());
            
            if (!string.IsNullOrWhiteSpace(type))
            {
                var typeLower = type.ToLower();
                query = query.Where(r => r.Type.ToLower() == typeLower);
            }

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
                    UserId = r.UserId,
                    MainThumbnailUrl = r.Images.Where(i => i.IsMain)
                                               .Select(i => baseUrl + i.ThumbnailUrl)
                                               .FirstOrDefault()
                })
                .ToListAsync();

            return rentals.Any() ? Results.Ok(rentals) : Results.NotFound(new { Message = "No matches found." });
        });

        // === 2. ПОЛУЧЕНИЕ ОДНОЙ КВАРТИРЫ ПО ID ===
        group.MapGet("/{id:guid}", async (Guid id, RentalDbContext db, HttpContext context) => 
        {
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            
            // Используем Include, чтобы загрузить связанные картинки
            var rental = await db.Rentals.Include(r => r.Images).FirstOrDefaultAsync(r => r.Id == id);
    
            if (rental == null) 
            {
                return Results.NotFound(new { Message = "Apartment does not exist." });
            }
            
            // Формируем детальный ответ с полными ссылками на картинки
            var response = new
            {
                rental.Id,
                rental.Title,
                rental.Description,
                rental.Price,
                rental.City,
                rental.Address,
                rental.Type,
                rental.LivingSpace,
                rental.RoomCount,
                rental.UserId,
                rental.Created,
                FullImageUrls = rental.Images.Select(i => baseUrl + i.FullImageUrl).ToList(),
            };
    
            return Results.Ok(response);
        });
        
        // === 3. СОЗДАНИЕ КВАРТИРЫ ===
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
        
        // === 4. ЗАГРУЗКА КАРТИНОК К КВАРТИРЕ ===
        group.MapPost("/{id:guid}/images", async (Guid id, IFormFile file, RentalDbContext db) =>
        {
            var rental = await db.Rentals.FindAsync(id);
            if (rental == null) return Results.NotFound();
    
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "rentals");
            Directory.CreateDirectory(uploadsFolder);
    
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsFolder, fileName);
            var thumbName = $"thumb_{fileName}";
            var thumbPath = Path.Combine(uploadsFolder, thumbName);
    
            // Сохраняем оригинал
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
    
            // Генерируем миниатюру
            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(300, 300),
                    Mode = ResizeMode.Max
                }));
                await image.SaveAsync(thumbPath);
            }
    
            var isFirstImage = !db.RentalImages.Any(i => i.RentalId == id);
    
            var newImage = new RentalImage
            {
                RentalId = id,
                FullImageUrl = $"/images/rentals/{fileName}",     // Сохраняем ТОЛЬКО относительный путь
                ThumbnailUrl = $"/images/rentals/{thumbName}",   // Сохраняем ТОЛЬКО относительный путь
                IsMain = isFirstImage
            };
    
            db.RentalImages.Add(newImage);
            await db.SaveChangesAsync();
    
            // При загрузке отдаем клиенту относительные пути (или можно приклеить baseUrl, если клиенту надо сразу показать)
            return Results.Ok(new { newImage.FullImageUrl, newImage.ThumbnailUrl });
        })
        .DisableAntiforgery();
        
        // 5. РЕДАКТИРОВАНИЕ (TEST 19)
        group.MapPut("/{id:guid}", async (Guid id, RentalRequest request, RentalDbContext db, ClaimsPrincipal user) =>
        {
            var rental = await db.Rentals.FindAsync(id);
            if (rental == null) return Results.NotFound();

            // Проверка прав: только владелец может менять
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (rental.UserId != userId) return Results.Forbid();

            rental.Title = request.Title;
            rental.Description = request.Description;
            rental.Price = request.Price;
            rental.City = request.City;
            rental.Address = request.Address;
            rental.Type = request.Type;
            rental.LivingSpace = request.LivingSpace;
            rental.RoomCount = request.RoomCount;
            rental.LastModified = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

// 6. СКРЫТЬ/ПОКАЗАТЬ (TEST 20)
        group.MapPatch("/{id:guid}/hide", async (Guid id, RentalDbContext db, ClaimsPrincipal user) =>
        {
            var rental = await db.Rentals.FindAsync(id);
            if (rental == null) return Results.NotFound();

            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (rental.UserId != userId) return Results.Forbid();

            rental.IsHidden = !rental.IsHidden;
            await db.SaveChangesAsync();
            return Results.Ok(new { rental.IsHidden });
        }).RequireAuthorization();

// 7. УДАЛЕНИЕ (TEST 21)
        group.MapDelete("/{id:guid}", async (Guid id, RentalDbContext db, ClaimsPrincipal user) =>
        {
            var rental = await db.Rentals.FindAsync(id);
            if (rental == null) return Results.NotFound();

            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (rental.UserId != userId) return Results.Forbid();

            db.Rentals.Remove(rental);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
        
        
        
        
    }
    
    
    
}