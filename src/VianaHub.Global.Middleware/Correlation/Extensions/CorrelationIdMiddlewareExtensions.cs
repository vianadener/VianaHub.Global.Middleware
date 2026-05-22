// <copyright file="CorrelationIdMiddlewareExtensions.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace VianaHub.Global.Middleware.Lib.Correlation.Extensions
{
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Provides an extension method for adding the Correlation ID middleware to the application pipeline.
    /// </summary>
    public static class CorrelationIdMiddlewareExtensions
    {
        /// <summary>
        /// Adds the Correlation ID middleware to the application's request processing pipeline.
        /// This middleware is responsible for generating and propagating a correlation ID for tracking requests.
        /// </summary>
        /// <param name="builder">The application builder used to configure the middleware pipeline.</param>
        /// <returns>The updated application builder with the Correlation ID middleware registered.</returns>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
