// <copyright file="RequestResponseLoggingMiddleware.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace VianaHub.Global.Middleware.Lib.Logging;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;
    private readonly RequestResponseLoggingOptions _options;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        IOptions<RequestResponseLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _memoryStreamManager = new RecyclableMemoryStreamManager();
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if we should log this path
        if (!ShouldLogPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Start timer for request duration
        var sw = Stopwatch.StartNew();

        // Collect request data
        var requestBody = await GetRequestBodyAsync(context.Request);
        var requestHeaders = GetHeaders(context.Request.Headers, _options.SensitiveHeaders);

        // Save original body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            // Replace response body with our memory stream to capture it
            using var responseBodyStream = _memoryStreamManager.GetStream();
            context.Response.Body = responseBodyStream;

            // Call the next delegate/middleware in the pipeline
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions
                LogRequestResponse(context, requestBody, requestHeaders, "Error", sw.ElapsedMilliseconds, null, null, ex);
                throw;
            }

            // Capture response data
            var responseBody = await GetResponseBodyAsync(context, responseBodyStream, originalBodyStream);
            var responseHeaders = GetHeaders(context.Response.Headers, _options.SensitiveHeaders);

            // Only log if:
            // 1. We log all requests, OR
            // 2. It's an error response and we're configured to log errors only
            bool isError = context.Response.StatusCode >= 400;
            if (!_options.LogErrorsOnly || isError)
            {
                string logLevel = isError ? "Error" : "Information";
                LogRequestResponse(context, requestBody, requestHeaders, logLevel, sw.ElapsedMilliseconds, responseBody, responseHeaders);
            }
        }
        finally
        {
            // Always restore the original body stream
            context.Response.Body = originalBodyStream;
        }
    }

    private bool ShouldLogPath(PathString path)
    {
        if (_options.PathFilters == null || _options.PathFilters.Count == 0)
        {
            return true;
        }

        string pathStr = path.ToString().ToLowerInvariant();
        return _options.PathFilters.Any(filter => pathStr.StartsWith(filter, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> GetRequestBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanSeek)
        {
            // Enable buffering so we can read the request body multiple times
            request.EnableBuffering();
        }

        request.Body.Position = 0;
        string requestBody;
        using (var streamReader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true))
        {
            requestBody = await streamReader.ReadToEndAsync();
        }
        request.Body.Position = 0;

        return requestBody;
    }

    private async Task<string> GetResponseBodyAsync(HttpContext context, MemoryStream responseBodyStream, Stream originalBodyStream)
    {
        responseBodyStream.Position = 0;
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        responseBodyStream.Position = 0;

        await responseBodyStream.CopyToAsync(originalBodyStream);

        return responseBody;
    }

    private Dictionary<string, string> GetHeaders(IHeaderDictionary headers, HashSet<string> sensitiveHeaders)
    {
        var result = new Dictionary<string, string>();
        foreach (var header in headers)
        {
            var key = header.Key;
            var value = header.Value.ToString();

            if (sensitiveHeaders.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                value = "[REDACTED]";
            }
            result[key] = value;
        }
        return result;
    }

    private void LogRequestResponse(
        HttpContext context,
        string requestBody,
        Dictionary<string, string> requestHeaders,
        string logLevel,
        long elapsedMs,
        string responseBody = null,
        Dictionary<string, string> responseHeaders = null,
        Exception ex = null)
    {
        try
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier,
                HttpMethod = context.Request.Method,
                context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                context.Response?.StatusCode,
                ElapsedMilliseconds = elapsedMs,
                Request = new
                {
                    Headers = requestHeaders,
                    Body = requestBody
                },
                Response = responseBody != null ? new
                {
                    Headers = responseHeaders,
                    Body = responseBody
                } : null,
                Error = ex != null ? new
                {
                    ex.Message,
                    ex.StackTrace
                } : null
            };

            // Convert to JSON for structured logging
            string jsonLog = JsonSerializer.Serialize(logEntry, _options.JsonSerializerOptions);

            // Send to the appropriate logger based on level
            switch (logLevel)
            {
                case "Error":
                    _logger.LogError(jsonLog);
                    break;
                case "Warning":
                    _logger.LogWarning(jsonLog);
                    break;
                default:
                    _logger.LogInformation(jsonLog);
                    break;
            }

            // If buffering is enabled and the buffer is full, trigger a flush
            if (_options.EnableBuffering && _options.BufferSize > 0)
            {
                // In a real implementation, this would track buffer size and flush when needed
                // This is a simplified example - actual implementation would depend on the logging system
            }
        }
        catch (Exception loggingEx)
        {
            // Log the error but don't throw - we don't want logging failures to break the application
            _logger.LogError(loggingEx, "Error occurred while logging request/response");
        }
    }
}

public class RequestResponseLoggingOptions
{
    public HashSet<string> SensitiveHeaders { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-API-Key",
        "ApiKey",
        "Password"
    };

    public List<string> PathFilters { get; set; } = new List<string>();

    public bool LogErrorsOnly { get; set; } = false;

    public bool EnableBuffering { get; set; } = true;

    public int BufferSize { get; set; } = 1000;

    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
    {
        WriteIndented = false
    };
}

public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IServiceCollection AddRequestResponseLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options from appsettings.json or other configuration sources
        services.Configure<RequestResponseLoggingOptions>(
            configuration.GetSection("RequestResponseLogging"));

        return services;
    }

    public static IApplicationBuilder UseRequestResponseLogging(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}