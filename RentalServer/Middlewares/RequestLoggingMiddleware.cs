using System.Text;

namespace RentalServer.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var requestBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        _logger.LogInformation(
            "HTTP Request Information: Method: {Method}, Path: {Path}, QueryString: {QueryString}, Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            string.IsNullOrWhiteSpace(requestBody) ? "Empty" : requestBody);

        await _next(context);
    }
}