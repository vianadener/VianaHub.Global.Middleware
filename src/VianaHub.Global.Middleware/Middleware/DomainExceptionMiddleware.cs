// <copyright file="DomainExceptionMiddleware.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>

namespace VianaHub.Global.Middleware.Lib.Middleware;

using VianaHub.Global.Middleware.Lib.ExceptionHandling;
using VianaHub.Global.Middleware.Lib.ExceptionHandling.Exceptions;
using VianaHub.Global.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Middleware responsible for handling domain-specific exceptions.
/// Captures custom business exceptions such as NotFoundException, BadRequestException,
/// and ConflictException, translating them into appropriate HTTP responses.
/// </summary>
public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for logging errors.</param>
    public DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware asynchronously, catching domain exceptions and
    /// returning structured error responses with appropriate HTTP status codes.
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
        catch (ConflictException ex)
        {
            await HandleDomainExceptionAsync(context, ex, 409, "Conflict", notify);
        }
        catch (NotFoundException ex)
        {
            await HandleDomainExceptionAsync(context, ex, 404, "NotFound", notify);
        }
        catch (BadRequestException ex)
        {
            await HandleDomainExceptionAsync(context, ex, 400, "BadRequest", notify);
        }
    }

    private async Task HandleDomainExceptionAsync(HttpContext context, Exception exception, int statusCode, string title, INotify notify)
    {
        var errorId = ErrorResponseHelper.GenerateErrorId();
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        if (context.Response.HasStarted)
        {
            _logger.LogError("⚠️ [ERROR-{ErrorId}] [CorrelationId: {CorrelationId}] Response already started; cannot modify", errorId, correlationId);
            return;
        }

        _logger.LogError(exception, 
            "⚠️ [ERROR-{ErrorId}] [CorrelationId: {CorrelationId}] Domain Exception | Type: {ExceptionType} | StatusCode: {StatusCode} | Message: {Message}", 
            errorId, correlationId, exception.GetType().FullName, statusCode, exception.Message);

        notify.Add(exception.Message, statusCode);

        var errorResponse = new ErrorResponse(title);
        errorResponse.AddError("Domain", exception.Message);
        errorResponse.AddError("Error ID", $"Contact support with the ID: {errorId}");

        await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, statusCode);
    }
}
