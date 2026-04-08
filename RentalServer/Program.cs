using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RentalServer.Data;
using RentalServer.Endpoints;
using RentalServer.ExceptionHandeler;
using RentalServer.Middlewares; // Убедись, что папка правильная
using Serilog;
using Serilog.Formatting.Compact;

// Настройка Serilog (Пишет в консоль и в JSON-файл)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "logs/server-log.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Подключаем Serilog к хосту
builder.Host.UseSerilog();

string secretKey = builder.Configuration["Jwt:Key"] ?? "MySuperSecretKeyForDevelopmentOnly123!"; 

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Создаем схему (теперь без .Models)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Введи токен в формате: Bearer {твой_токен}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey, // В .NET 10 рекомендуется использовать Http
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    
    // 2. Создаем требование через лямбду (document => ...)
    // И передаем только один аргумент "Bearer" в Reference
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Включаем логирование запросов
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

app.MapRentalEndpoints();
app.MapAuthEndpoints(app.Configuration);

app.Run();