// <copyright file="HttpProtocolExceptionMiddleware.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>

namespace EBL.FIG.Common.Middleware.Lib.Middleware;

using EBL.FIG.Common.Middleware.Lib.ExceptionHandling;

using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Middleware responsible for handling HTTP protocol-level exceptions.
/// Captures BadHttpRequestException (without JsonException inner exception) that occur
/// due to malformed HTTP requests, invalid headers, or protocol violations.
/// </summary>
public class HttpProtocolExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpProtocolExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpProtocolExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for logging errors.</param>
    public HttpProtocolExceptionMiddleware(RequestDelegate next, ILogger<HttpProtocolExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware asynchronously, catching HTTP protocol exceptions and
    /// returning structured error responses.
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
        catch (BadHttpRequestException ex) when (ex.InnerException is not System.Text.Json.JsonException)
        {
            await HandleHttpProtocolExceptionAsync(context, ex, notify);
        }
    }

    private async Task HandleHttpProtocolExceptionAsync(HttpContext context, BadHttpRequestException exception, INotify notify)
    {
        var errorId = ErrorResponseHelper.GenerateErrorId();
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        if (context.Response.HasStarted)
        {
            _logger.LogError("⚠️ [ERROR-{ErrorId}] [CorrelationId: {CorrelationId}] Response already started; cannot modify", errorId, correlationId);
            return;
        }

        _logger.LogError(exception, 
            "⚠️ [ERROR-{ErrorId}] [CorrelationId: {CorrelationId}] HTTP Protocol Exception | StatusCode: {StatusCode} | Message: {Message}", 
            errorId, correlationId, exception.StatusCode, exception.Message);

        notify.Add("Invalid request format", 400);

        var errorResponse = new ErrorResponse("BadRequest");
        errorResponse.AddError("Request", "The request could not be processed due to invalid format or content");
        errorResponse.AddError("Error ID", $"Contact support with the ID: {errorId}");

        await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, 400);
    }
}
