// <copyright file="NotificationMiddleware.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>

namespace VianaHub.Global.Middleware.Lib.Middleware;

using VianaHub.Global.Middleware.Lib.ExceptionHandling;

using VianaHub.Global.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Middleware responsible for processing accumulated notifications from the INotify service.
/// This middleware runs after the request has been processed and checks if any validation
/// or business rule errors have been collected. If notifications exist, it returns a
/// structured error response with the appropriate HTTP status code.
/// </summary>
public class NotificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NotificationMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for logging errors.</param>
    public NotificationMiddleware(RequestDelegate next, ILogger<NotificationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware asynchronously. Processes the request and checks for notifications
    /// after execution. If notifications are present and the response hasn't started, constructs
    /// and returns an error response with the collected messages.
    /// </summary>
    /// <param name="context">The HTTP context of the current request.</param>
    /// <param name="notify">The notification service containing accumulated errors.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, INotify notify)
    {
        await _next(context);

        if (notify != null && notify.HasNotify())
        {
            var correlationId = ErrorResponseHelper.GetCorrelationId(context);

            if (context.Response.HasStarted)
            {
                _logger.LogError("⚠️ [NOTIFY] [CorrelationId: {CorrelationId}] Response already started; cannot modify", correlationId);
                return;
            }

            var statusCode = (int)notify.GetStatusCode();
            var errorMessages = notify.GetErrorMessage();

            _logger.LogError("⚠️ [NOTIFY] [CorrelationId: {CorrelationId}] Notifications Found | StatusCode: {StatusCode} | Count: {Count} | Messages: {Messages}", 
                correlationId, statusCode, errorMessages.Count, string.Join("; ", errorMessages));

            var errorResponse = new ErrorResponse(ErrorResponseHelper.GetErrorTitle(statusCode));

            foreach (var message in errorMessages)
            {
                if (message.Contains(':'))
                {
                    var parts = message.Split(':', 2);
                    var field = parts[0].Trim();
                    var payload = parts[1].Trim();

                    errorResponse.AddError(field, payload);
                }
                else
                {
                    errorResponse.AddError(message);
                }
            }

            await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, statusCode);
        }
    }
}
