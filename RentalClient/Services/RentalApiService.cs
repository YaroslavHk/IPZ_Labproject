using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;

namespace RentalClient.Services;

public interface IRentalApiService
{
    Task<bool> LoginAsync(AuthRequest authRequest);
    
    Task<bool> RegisterAsync(RegisterRequest registerRequest);

    Task<bool> CreateListingAsync(RentalRequest rentalRequest);
    
    Task<List<ShortRentalResponse>> GetRentals(int page, int pageSize);

}

public class RentalApiService : IRentalApiService
{
    private readonly HttpClient _httpClient;
    private readonly IUserSession _userSession;
    
    public RentalApiService(HttpClient httpClient, IUserSession userSession)
    {
        _httpClient = httpClient;
        _userSession = userSession;
    }
    

    public async Task<bool> LoginAsync(AuthRequest authRequest)
    {
        HttpResponseMessage message =  await _httpClient.PostAsJsonAsync("/api/auth/login", authRequest);

        if (message.IsSuccessStatusCode)
        {
            var authResult = await message.Content.ReadFromJsonAsync<AuthResponse>();
            
            _userSession.Login(authResult.Token, authRequest.Login);
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
            return true;
        }
        return false;
    }

    public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
    {
        HttpResponseMessage message = await _httpClient.PostAsJsonAsync("/api/auth/register", registerRequest);

        if (message.IsSuccessStatusCode)
        {
            return true;
        }
        return false;
    }

    public async Task<bool> CreateListingAsync(RentalRequest  rentalRequest)
    {
        HttpResponseMessage message = await _httpClient.PostAsJsonAsync("/api/rentals", rentalRequest);
    
        if (message.IsSuccessStatusCode)
        {
            var result = await message.Content.ReadFromJsonAsync<RentalPostResponse>();
            MessageBox.Show($"[Success] Rental created with ID: {result?.Id}");
            return true;
        }
        var error = await message.Content.ReadAsStringAsync();
        throw new HttpRequestException($"[Error] Failed to create rental: {message.StatusCode}\nDetails: {error}");
    }
    
    public async Task<List<ShortRentalResponse>> GetRentals(int page, int pageSize)
    {
        HttpResponseMessage message = await _httpClient.GetAsync($"/api/rentals?page={page}&pageSize={pageSize}");
    
        if (!message.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Не вдалося завантажити оголошення. Код: {message.StatusCode}");
        }
       
        var data = await message.Content.ReadFromJsonAsync<List<ShortRentalResponse>>();
        return data ?? new List<ShortRentalResponse>();
    }
    
}