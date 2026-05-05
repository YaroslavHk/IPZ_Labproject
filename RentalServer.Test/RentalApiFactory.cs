using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging; // <-- Добавили для работы с логами
using RentalServer.Data;

namespace RentalServer.Tests;

public class RentalApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 1. Полностью отключаем все провайдеры логов (включая Better Stack) для тестов
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            // 2. Удаляем твою боевую SQLite БД из сервисов
            services.RemoveAll(typeof(DbContextOptions<RentalDbContext>));
            services.RemoveAll(typeof(RentalDbContext));

            // 3. Создаем чистую БД в оперативной памяти
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<RentalDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // 4. Применяем схему БД, чтобы таблицы создались
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RentalDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Close();
    }
}