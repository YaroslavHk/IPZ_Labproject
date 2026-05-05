using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RentalServer.Data;
using RentalServer.Endpoints;
using RentalServer.ExceptionHandeler;
using RentalServer.Middlewares;
using RentalServer.Models.Debug;
using Serilog;
using Serilog.Formatting.Compact;

using Microsoft.AspNetCore.HttpOverrides;
using RentalServer.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "logs/server-log.json", rollingInterval: RollingInterval.Day)
    .WriteTo.BetterStack(sourceToken: "UxM7SjjFHuLaE4iXdXP7PBZP")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddSqlite<RentalDbContext>("Data Source=rentals_v2.db");

string secretKey = builder.Configuration["Jwt:Key"] ?? "MySuperSecretKeyForDevelopmentOnly123!"; 
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

//builder.Services.AddHostedService<NgrokHostedService>();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Введи токен в формате: Bearer {твой_токен}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // Указываем, какие именно заголовки мы хотим читать
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                               ForwardedHeaders.XForwardedProto | 
                               ForwardedHeaders.XForwardedHost;
    
    // Для безопасности ASP.NET по умолчанию доверяет только локальным прокси. 
    // Очищаем списки, чтобы доверять туннелю Ngrok:
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseSerilogRequestLogging();

if (args.Contains("--seed"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<RentalDbContext>();
        
        Console.WriteLine("Начинаю заполнение базы данных...");
        await DatabaseSeeder.SeedAsync(db);
        Console.WriteLine("Заполнение завершено. Выход из программы.");
    }
    
    // Прерываем выполнение (сервер не будет запускаться, мы просто выполнили скрипт)
    return; 
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();


app.MapRentalEndpoints();
app.MapAuthEndpoints(app.Configuration);
app.MapFavoriteEndpoints();
    
app.Run();

public partial class Program { }