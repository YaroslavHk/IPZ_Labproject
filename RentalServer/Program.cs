using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RentalServer.Data;
using RentalServer.Endpoints;

var builder = WebApplication.CreateBuilder(args);
string secretKey = builder.Configuration["Jwt:Key"] ?? "MySuperSecretKeyForDevelopmentOnly123!"; 
// 1. Setup Database Connection
builder.Services.AddSqlite<RentalDbContext>("Data Source=rentals_v2.db");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();
// 2. Add Swagger for API testing in the browser
// Настройка Swagger (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Создаем схему (кнопка Authorize)
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Введи токен в формате: Bearer {твой_токен}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    };
    
    options.AddSecurityDefinition("Bearer", securityScheme);

    // 2. Создаем требование с новым синтаксисом OpenAPI v2 (.NET 10)
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        // Передаем название схемы ("Bearer") и сам документ
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. Activate Authentication and Authorization Middleware
// Order is strictly important: Who are you? -> Are you allowed?
app.UseAuthentication();
app.UseAuthorization();

// 5. Map Endpoints
app.MapRentalEndpoints();
app.MapAuthEndpoints(app.Configuration);

app.Run();