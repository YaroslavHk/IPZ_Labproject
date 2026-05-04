using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace RentalClient.Services;

public interface IRentalApiService
{
    Task<bool> LoginAsync(AuthRequest authRequest);
    Task<bool> RegisterAsync(RegisterRequest registerRequest);
    Task<Guid?> CreateListingAsync(RentalRequest rentalRequest);
    Task<bool> UploadListingPhotosAsync(Guid rentalId, IEnumerable<string> filePaths);
    Task<bool> DeleteRentalAsync(Guid rentalId);
    Task<List<ShortRentalResponse>> GetRentals(int page, int pageSize);
    Task<RentalResponse> GetRentalByIdAsync(Guid rentalId);
    Task<List<ShortRentalResponse>> GetShortRentalByFilterAsync(int page, int pageSize, string? title, decimal? minPrice, decimal? maxPrice, int? rooms, float? minSpace, float? maxSpace, string? type, string? sortBy, bool isDescending);
    Task<List<ShortRentalResponse>> GetMyRentalsAsync();
    Task<bool> UpdateProfileAsync(UpdateProfileRequest request);
    Task<bool> UpdateListingAsync(Guid rentalId, RentalRequest request);
    Task<bool> HideRentalAsync(Guid rentalId);
    Task<bool> AddToFavoritesAsync(Guid rentalId);
    Task<bool> RemoveFromFavoritesAsync(Guid rentalId);
    Task<List<ShortRentalResponse>> GetFavoriteRentalsAsync();
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
        var message = await _httpClient.PostAsJsonAsync("/api/auth/login", authRequest);

        if (message.IsSuccessStatusCode)
        {
            var authResult = await message.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResult != null)
            {
                _userSession.Login(
                    authResult.Token, 
                    authResult.UserName, 
                    authResult.Email, 
                    authResult.Phone,
                    authResult.UserId);
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
                return true;
            }
        }
        return false;
    }

    public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
    {
        var message = await _httpClient.PostAsJsonAsync("/api/auth/register", registerRequest);
        return message.IsSuccessStatusCode;
    }

    public async Task<Guid?> CreateListingAsync(RentalRequest rentalRequest)
    {
        var message = await _httpClient.PostAsJsonAsync("/api/rentals", rentalRequest);
    
        if (message.IsSuccessStatusCode)
        {
            var result = await message.Content.ReadFromJsonAsync<RentalPostResponse>();
            return result?.Id;
        }
        
        var error = await message.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Failed to create rental: {message.StatusCode}\nDetails: {error}");
    }
    
    public async Task<bool> UploadListingPhotosAsync(Guid rentalId, IEnumerable<string> filePaths)
    {
        bool isAllUploadedSuccessfully = true;

        foreach (var path in filePaths)
        {
            try
            {
                using var content = new MultipartFormDataContent();
            
                var fileContent = new ByteArrayContent(File.ReadAllBytes(path));
            
                content.Add(fileContent, "file", Path.GetFileName(path));

                var response = await _httpClient.PostAsync($"/api/rentals/{rentalId}/images", content);
            
                if (!response.IsSuccessStatusCode)
                {
                    isAllUploadedSuccessfully = false;
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[API Error] Помилка завантаження {Path.GetFileName(path)}: {error}");
                }
            }
            catch (Exception ex)
            {
                isAllUploadedSuccessfully = false;
                System.Diagnostics.Debug.WriteLine($"[Network Error] Помилка завантаження {Path.GetFileName(path)}: {ex.Message}");
            }
        }

        return isAllUploadedSuccessfully;
    }
    
    public async Task<bool> DeleteRentalAsync(Guid rentalId)
    {
        var message = await _httpClient.DeleteAsync($"/api/rentals/{rentalId}");
        return message.IsSuccessStatusCode;
    }
    
    public async Task<List<ShortRentalResponse>> GetRentals(int page, int pageSize)
    {
        var message = await _httpClient.GetAsync($"/api/rentals?page={page}&pageSize={pageSize}");
    
        if (!message.IsSuccessStatusCode)
            throw new HttpRequestException($"Не вдалося завантажити оголошення. Код: {message.StatusCode}");
       
        var data = await message.Content.ReadFromJsonAsync<List<ShortRentalResponse>>();
        return data ?? new List<ShortRentalResponse>();
    }

    public async Task<RentalResponse> GetRentalByIdAsync(Guid rentalId)
    {
        var message = await _httpClient.GetAsync($"/api/rentals/{rentalId}");

        if (!message.IsSuccessStatusCode)
            throw new HttpRequestException($"Не вдалося завантажити оголошення. Код: {message.StatusCode}");
        
        return await message.Content.ReadFromJsonAsync<RentalResponse>();
    }

    public async Task<List<ShortRentalResponse>> GetShortRentalByFilterAsync(
        int page, int pageSize, string? title = null, decimal? minPrice = null, decimal? maxPrice = null, 
        int? roomsCount = null, float? minSpace = null, float? maxSpace = null, 
        string? type = null, string? sortBy = null, bool isDescending = false)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(title)) queryParams.Add($"city={Uri.EscapeDataString(title)}");
        if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice.Value}");
        if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice.Value}");
        if (roomsCount.HasValue) queryParams.Add($"roomsCount={roomsCount.Value}");
        if (minSpace.HasValue) queryParams.Add($"minSpace={minSpace.Value}");
        if (maxSpace.HasValue) queryParams.Add($"maxSpace={maxSpace.Value}");
        if (!string.IsNullOrWhiteSpace(type)) queryParams.Add($"type={Uri.EscapeDataString(type)}");
        if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        
        queryParams.Add($"sortDesc={isDescending.ToString().ToLower()}"); 

        string apiRoute = $"/api/rentals?{string.Join("&", queryParams)}";
        
        var message = await _httpClient.GetAsync(apiRoute);

        if (!message.IsSuccessStatusCode)
            throw new HttpRequestException($"Не вдалося завантажити оголошення. Код: {message.StatusCode}");
        
        var data = await message.Content.ReadFromJsonAsync<List<ShortRentalResponse>>();
        return data ?? new List<ShortRentalResponse>();
    }

    public async Task<List<ShortRentalResponse>> GetMyRentalsAsync()
    {
        var message = await _httpClient.GetAsync($"/api/rentals?userId={_userSession.UserId}");

        if (!message.IsSuccessStatusCode)
            return new List<ShortRentalResponse>();
    
        var data = await message.Content.ReadFromJsonAsync<List<ShortRentalResponse>>();
        return data ?? new List<ShortRentalResponse>();
    }
    
    public async Task<bool> UpdateProfileAsync(UpdateProfileRequest request)
    {
        var message = await _httpClient.PatchAsJsonAsync("/api/auth/profile", request);
        return message.IsSuccessStatusCode;
    }
    
    public async Task<bool> UpdateListingAsync(Guid rentalId, RentalRequest request)
    {
        var message = await _httpClient.PutAsJsonAsync($"/api/rentals/{rentalId}", request);
        return message.IsSuccessStatusCode;
    }
    
    public async Task<bool> HideRentalAsync(Guid rentalId)
    {
        var message = await _httpClient.PatchAsync($"/api/rentals/{rentalId}/hide", null);
        return message.IsSuccessStatusCode;
    }
    
    public async Task<bool> AddToFavoritesAsync(Guid rentalId)
    {
        var message = await _httpClient.PostAsync($"/api/favorites/{rentalId}", null);
        return message.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveFromFavoritesAsync(Guid rentalId)
    {
        var message = await _httpClient.DeleteAsync($"/api/favorites/{rentalId}");
        return message.IsSuccessStatusCode;
    }
    
    public async Task<List<ShortRentalResponse>> GetFavoriteRentalsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/favorites");
        
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<ShortRentalResponse>>();
                return data ?? new List<ShortRentalResponse>();
            }
            return new List<ShortRentalResponse>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error (GetFavorites): {ex.Message}");
            return new List<ShortRentalResponse>();
        }
    }
}