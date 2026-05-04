using System;
using System.IO;
using System.Text.Json;

namespace RentalClient.Services;

public static class ConfigLoader
{
    private record AppConfig(string ServerUrl);

    public static string LoadServerUrl()
    {
        string filePath = "config.json";

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"[Критическая ошибка]: Файл {filePath} не найден.");
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = JsonSerializer.Deserialize<AppConfig>(json, options);

            if (config == null || string.IsNullOrWhiteSpace(config.ServerUrl))
            {
                throw new InvalidOperationException("[Критическая ошибка]: ServerUrl пуст в config.json.");
            }

            return config.ServerUrl;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"[Ошибка структуры JSON]: {ex.Message}", ex);
        }
    }
}