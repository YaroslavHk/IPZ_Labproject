using Microsoft.EntityFrameworkCore;
using RentalServer.Data;
using RentalServer.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup Database Connection
builder.Services.AddSqlite<RentalDbContext>("Data Source=rentals_v2.db");

// 2. Add Swagger for API testing in the browser
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. Register all endpoints from our separate files
app.MapRentalEndpoints();

// 5. Run the application
app.Run();