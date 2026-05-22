// <copyright file="ExceptionHandlingMiddleware.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace VianaHub.Global.Middleware.Lib.Middleware
{
    using VianaHub.Global.Middleware.Lib.ExceptionHandling;
    using VianaHub.Global.Middleware.Lib.ExceptionHandling.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System.Net;

    /// <summary>
    /// Middleware for handling exceptions globally in the application pipeline.
    /// Captures unhandled exceptions, logs them, and returns appropriate HTTP responses.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware delegate in the pipeline.</param>
        /// <param name="logger">The logger instance for logging errors.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invokes the middleware asynchronously, handling any unhandled exceptions.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            try
            {
                await _next(httpContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var correlationId = ErrorResponseHelper.GetCorrelationId(httpContext);

                _logger.LogError(ex, 
                    "⚠️ [CorrelationId: {CorrelationId}] An unhandled exception occurred | Type: {ExceptionType} | Message: {Message}", 
                    correlationId, ex.GetType().FullName, ex.Message);

                await HandleExceptionAsync(httpContext, ex).ConfigureAwait(false);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var correlationId = ErrorResponseHelper.GetCorrelationId(context);

            switch (exception)
            {
                case ConflictException conflictException:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    return context.Response.WriteAsync(new ErrorDetails(context.Response.StatusCode, conflictException.Message, correlationId).ToString());

                case NotFoundException notFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return context.Response.WriteAsync(new ErrorDetails(context.Response.StatusCode, notFoundException.Message, correlationId).ToString());

                case BadRequestException badRequestException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return context.Response.WriteAsync(new ErrorDetails(context.Response.StatusCode, badRequestException.Message, correlationId).ToString());

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return context.Response.WriteAsync(new ErrorDetails(context.Response.StatusCode, "Internal Server Error", correlationId).ToString());
            }
        }
    }
}
