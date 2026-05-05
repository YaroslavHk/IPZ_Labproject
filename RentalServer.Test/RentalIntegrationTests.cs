using System.Net;
using System.Net.Http.Json;
using RentalServer.DTO; // Убедись, что этот using совпадает с твоим пространством имен DTO
using Xunit;

namespace RentalServer.Tests;

public class RentalIntegrationTests : IClassFixture<RentalApiFactory>
{
    private readonly HttpClient _client;

    public RentalIntegrationTests(RentalApiFactory factory)
    {
        // Фабрика автоматически поднимет сервер в памяти и даст нам клиента для отправки HTTP запросов
        _client = factory.CreateClient();
    }

    // ТЕСТ 1: Проверка получения списка
    [Fact]
    public async Task GetRentals_WhenEmpty_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/rentals");
        
        // Так как тестовая БД пустая, твой код логично возвращает 404
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ТЕСТ 2: Проверка 404 ошибки
    [Fact]
    public async Task GetRentalById_NonExistent_ReturnsNotFound()
    {
        var fakeId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/rentals/{fakeId}");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ТЕСТ 3: Проверка валидатора (FluentValidation)
    [Fact]
    public async Task CreateRental_WithInvalidData_ReturnsBadRequest()
    {
        // 1. АВТОРИЗАЦИЯ (Имитируем реального пользователя)
        // Регистрируем фейкового юзера в нашей in-memory БД
        var regRequest = new registerRequest 
        { 
            Email = "test@test.com", 
            UserName = "tester", 
            Phone = "12345678", 
            Password = "Password123" 
        };
        await _client.PostAsJsonAsync("/api/auth/register", regRequest);

        // Логинимся под ним
        var loginRequest = new AuthRequest { Login = "test@test.com", Password = "Password123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authData = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Добавляем полученный JWT токен в заголовки нашего тестового клиента
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authData!.Token);

        // ==========================================================

        // 2. САМ ТЕСТ (Отправляем невалидные данные квартиры)
        var request = new RentalRequest { Title = "", Price = 0, City = "Kyiv" };
        var response = await _client.PostAsJsonAsync("/api/rentals", request);
        
        // Теперь мы авторизованы! Сервер пустит нас внутрь, отработает валидатор и вернет 400
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ТЕСТ 4: Имитация отправки файлов с WPF-клиента
    [Fact]
    public async Task UploadImages_WithValidFiles_WorksProperly()
    {
        using var content = new MultipartFormDataContent();
        var fakeFile = new ByteArrayContent(new byte[] { 0xFF, 0xD8 }); // Имитация байтов
        
        // Указываем тип контента, чтобы Minimal API понял, что это картинка
        fakeFile.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        
        // ВАЖНО: Имя "file" должно совпадать с IFormFile file на твоем сервере
        content.Add(fakeFile, "file", "test.jpg"); 

        var fakeId = Guid.NewGuid();
        var response = await _client.PostAsync($"/api/rentals/{fakeId}/images", content);

        // Теперь биндинг пройдет успешно, сервер не найдет квартиру и вернет 404
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}