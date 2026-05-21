// <copyright file="JsonExceptionMiddleware.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>

namespace EBL.FIG.Common.Middleware.Lib.Middleware;

using EBL.FIG.Common.Middleware.Lib.ExceptionHandling;

using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// Middleware responsible for handling JSON parsing exceptions during HTTP request processing.
/// Captures JsonException and BadHttpRequestException with JsonException inner exception,
/// providing detailed error feedback including line numbers and byte positions when available.
/// </summary>
public class JsonExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JsonExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for logging errors.</param>
    public JsonExceptionMiddleware(RequestDelegate next, ILogger<JsonExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware asynchronously, catching JSON-related exceptions and
    /// returning structured error responses with detailed diagnostic information.
    /// </summary>
    /// <param name="context">The HTTP context of the current request.</param>
    /// <param name="notify">The notification service for logging errors.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, INotify notify)
    {
        try
        {
            await _next(context);
        }
        catch (BadHttpRequestException ex) when (ex.InnerException is JsonException jsonEx)
        {
            await HandleJsonExceptionAsync(context, jsonEx, notify);
        }
        catch (JsonException jsonEx)
        {
            await HandleJsonExceptionAsync(context, jsonEx, notify);
        }
    }

    private async Task HandleJsonExceptionAsync(HttpContext context, JsonException jsonEx, INotify notify)
    {
        var errorId = ErrorResponseHelper.GenerateErrorId();
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        if (context.Response.HasStarted)
        {
            _logger.LogError("⚠️ [ERROR-{ErrorId}] [CorrelationId: {CorrelationId}] Response already started; cannot modify", errorId, correlationId);
            return;
        }

        string friendlyKey = GetFriendlyJsonErrorKey(jsonEx);

        var lineNumber = GetLineNumber(jsonEx);
        var position = GetBytePosition(jsonEx);

        _logger.LogError(jsonEx, 
            "⚠️ [ERROR-{ErrorId}] [CorrelationId: {CorrelationId}] JSON Exception | Error: {FriendlyError} | Line: {Line} | Position: {Position} | Message: {Message}", 
            errorId, correlationId, friendlyKey, lineNumber?.ToString() ?? "N/A", position?.ToString() ?? "N/A", jsonEx.Message);

        notify.Add($"Invalid JSON format: {friendlyKey}", 400);

        var errorResponse = new ErrorResponse("Invalid JSON Format");
        errorResponse.AddError("Format", friendlyKey);
        errorResponse.AddError("Error ID", $"Contact support with the ID: {errorId}");

        if (lineNumber.HasValue)
        {
            errorResponse.AddError("Line", $"Line {lineNumber.Value}");
        }

        if (position.HasValue)
        {
            errorResponse.AddError("Position", $"Position {position.Value}");
        }

        await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, 400);
    }

    private static string GetFriendlyJsonErrorKey(JsonException jsonEx)
    {
        var message = jsonEx.Message.ToLower();

        if (message.Contains("trailing comma"))
        {
            return "JSON contains an invalid trailing comma";
        }

        if (message.Contains("unexpected character") || message.Contains("invalid character"))
        {
            return "JSON contains an invalid character";
        }

        if (message.Contains("unterminated string"))
        {
            return "JSON contains an unterminated string";
        }

        if (message.Contains("expected") && message.Contains("got"))
        {
            return "JSON is malformed";
        }

        if (message.Contains("depth"))
        {
            return "JSON has too many nesting levels";
        }

        if (message.Contains("property name"))
        {
            return "JSON contains an invalid property name";
        }

        if (message.Contains("missing"))
        {
            return "JSON is incomplete";
        }

        return "JSON is invalid";
    }

    private static int? GetLineNumber(JsonException jsonEx)
    {
        var lineNumberProperty = jsonEx.GetType().GetProperty("LineNumber");
        if (lineNumberProperty != null)
        {
            var value = lineNumberProperty.GetValue(jsonEx);
            if (value != null)
            {
                return Convert.ToInt32(value);
            }
        }

        return null;
    }

    private static int? GetBytePosition(JsonException jsonEx)
    {
        var bytePositionProperty = jsonEx.GetType().GetProperty("BytePositionInLine");
        if (bytePositionProperty != null)
        {
            var value = bytePositionProperty.GetValue(jsonEx);
            if (value != null)
            {
                return Convert.ToInt32(value);
            }
        }

        return null;
    }
}
