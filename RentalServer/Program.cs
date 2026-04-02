using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Настраиваем Swagger для тестирования
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Настраиваем подключение к SQLite
builder.Services.AddSqlite<RentalDb>("Data Source=rentals.db");

// 3. Собираем приложение
var app = builder.Build();

// 4. Включаем интерфейс Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. ЭНДПОИНТ GET: Читаем из базы
app.MapGet("/api/rentals", async (string? city, RentalDb db) => 
{
    var query = db.Rentals.AsQueryable();
    
    if (!string.IsNullOrEmpty(city))
    {
        query = query.Where(r => r.City.ToLower() == city.ToLower());
    }
    
    return Results.Ok(await query.ToListAsync());
});

// 6. ЭНДПОИНТ POST: Сохраняем в базу
app.MapPost("/api/rentals", async (Rental newRental, RentalDb db) => 
{
    db.Rentals.Add(newRental);
    await db.SaveChangesAsync();
    
    return Results.Created($"/api/rentals/{newRental.Id}", newRental);
});

app.Run();

// --- НАШИ МОДЕЛИ ДАННЫХ ---

class Rental
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public string City { get; set; } = "";
}

class RentalDb : DbContext
{
    public RentalDb(DbContextOptions<RentalDb> options) : base(options) { }
    public DbSet<Rental> Rentals => Set<Rental>();
}