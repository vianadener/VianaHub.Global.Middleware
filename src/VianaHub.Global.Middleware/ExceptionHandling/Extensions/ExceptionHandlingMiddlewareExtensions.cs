// <copyright file="ExceptionHandlingMiddlewareExtensions.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace VianaHub.Global.Middleware.Lib.ExceptionHandling.Extensions
{
    using VianaHub.Global.Middleware.Lib.Middleware;
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Provides an extension method for registering the ExceptionHandlingMiddleware in the application pipeline.
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Adds the ExceptionHandlingMiddleware to the application's middleware pipeline.
        /// This middleware is responsible for handling exceptions globally.
        /// </summary>
        /// <param name="builder">The application builder used to configure the middleware pipeline.</param>
        /// <returns>The updated application builder with the middleware registered.</returns>
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
