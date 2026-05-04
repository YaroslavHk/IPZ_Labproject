using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using RentalClient.Services;

namespace RentalClient.Core
{
    public class NgrokImageConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _baseUrl = ConfigLoader.LoadServerUrl();

        private static readonly ConcurrentDictionary<string, BitmapImage> _memoryCache = new();

        private static readonly string _cacheDirectory = Path.Combine(Path.GetTempPath(), "RentalClient_ImageCache");

        static NgrokImageConverter()
        {
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        url = $"{_baseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
                    }

                    if (_memoryCache.TryGetValue(url, out var cachedImage))
                    {
                        return cachedImage;
                    }

                    string safeFileName = GetHashString(url) + ".jpg";
                    string localFilePath = Path.Combine(_cacheDirectory, safeFileName);

                    byte[] imageBytes;

                    if (File.Exists(localFilePath))
                    {
                        imageBytes = File.ReadAllBytes(localFilePath);
                    }
                    else
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Add("ngrok-skip-browser-warning", "true");

                        var response = _httpClient.Send(request);
                        response.EnsureSuccessStatusCode();

                        imageBytes = response.Content.ReadAsByteArrayAsync().Result;

                        Task.Run(() => 
                        {
                            try { File.WriteAllBytes(localFilePath, imageBytes); } 
                            catch { /* Игнорируем ошибки доступа к файлу при сохранении кэша */ }
                        });
                    }

                    using var ms = new MemoryStream(imageBytes);
                    var bitmap = new BitmapImage();
                    
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    
                    bitmap.Freeze(); 

                    _memoryCache.TryAdd(url, bitmap);

                    return bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ПОМИЛКА ЗАВАНТАЖЕННЯ ФОТО: {ex.Message} | URL: {url}");
                }
            }
            
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Зворотня конвертація не підтримується.");
        }

        private static string GetHashString(string inputString)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}