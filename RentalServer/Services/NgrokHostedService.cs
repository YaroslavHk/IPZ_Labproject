using System.Diagnostics;
using System.Text.Json;

namespace RentalServer.Services;

public class NgrokHostedService : BackgroundService
{
    private Process? _ngrokProcess;
    private readonly ILogger<NgrokHostedService> _logger;
    private readonly HttpClient _httpClient = new HttpClient();

    public NgrokHostedService(ILogger<NgrokHostedService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Запуск Ngrok в фоновом режиме...");

        try
        {
            // 1. Запускаем Ngrok скрыто (без черного окна)
            _ngrokProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "ngrok",        // Работает, если ngrok добавлен в PATH Windows
                Arguments = "http 5177",   // Твой порт
                CreateNoWindow = true,     // Не показывать отдельное окно консоли
                UseShellExecute = false
            });

            // 2. Ждем пару секунд, чтобы Ngrok успел подняться и открыть туннель
            await Task.Delay(2000, stoppingToken);

            // 3. Стучимся в локальное API Ngrok, чтобы достать нашу публичную ссылку
            var response = await _httpClient.GetAsync("http://127.0.0.1:4040/api/tunnels", stoppingToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(stoppingToken);
                using var doc = JsonDocument.Parse(json);
                
                // Достаем URL из JSON-ответа Ngrok
                var url = doc.RootElement.GetProperty("tunnels")[0].GetProperty("public_url").GetString();
                
                _logger.LogInformation("=====================================================");
                _logger.LogInformation($"🚀 NGROK URL: {url}");
                _logger.LogInformation("=====================================================");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка запуска Ngrok: {ex.Message}. Убедитесь, что ngrok установлен и добавлен в PATH.");
        }
    }

    // Этот метод вызывается автоматически при остановке сервера (Ctrl+C или кнопка Стоп в Rider)
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Остановка Ngrok...");
        
        if (_ngrokProcess != null && !_ngrokProcess.HasExited)
        {
            _ngrokProcess.Kill(); // Убиваем процесс
            _ngrokProcess.Dispose();
        }
        
        await base.StopAsync(cancellationToken);
    }
}