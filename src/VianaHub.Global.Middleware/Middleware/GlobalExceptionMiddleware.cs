using EBL.FIG.Common.Middleware.Lib.ExceptionHandling;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace EBL.FIG.Common.Middleware.Lib.Middleware;

/// <summary>
/// Middleware acting as the final catch-all exception handler in the pipeline.
/// Captures any unhandled exceptions that were not caught by specialized middleware,
/// ensuring no exceptions leak to the client without proper handling. Returns a generic
/// 500 Internal Server Error response while logging detailed error information for diagnostics.
/// This middleware should be registered first (outermost) in the middleware pipeline.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for logging errors.</param>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware asynchronously. Acts as a safety net for any unhandled exceptions
    /// in the application pipeline, logging the full exception details and returning a generic
    /// error response to protect sensitive information from being exposed to clients.
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
        catch (Exception ex)
        {
            await HandleUnexpectedExceptionAsync(context, ex, notify);
        }
    }

    private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception exception, INotify notify)
    {
        var errorId = ErrorResponseHelper.GenerateErrorId();
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        if (context.Response.HasStarted)
        {
            _logger.LogError("⚠️ [ERROR-{ErrorId}] [CorrelationId: {CorrelationId}] Response already started; cannot modify", errorId, correlationId);
            return;
        }

        _logger.LogError(exception, 
            "⚠️ [ERROR-{ErrorId}] [CorrelationId: {CorrelationId}] Unhandled Exception | Type: {ExceptionType} | Message: {Message}", 
            errorId, correlationId, exception.GetType().FullName, exception.Message);

        notify.Add("An unexpected error occurred while processing your request", 500);

        var errorResponse = new ErrorResponse("Internal Server Error");
        errorResponse.AddError("System", "An unexpected error occurred. Our team has been notified and is working on a solution");
        errorResponse.AddError("Error ID", $"Contact support with the ID: {errorId}");

        await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, (int)HttpStatusCode.InternalServerError);
    }
}
