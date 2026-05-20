// <copyright file="SanitizationMiddlewareExtensions.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace EBL.FIG.Common.Middleware.Lib.Sanitization.Extensions
{
    using EBL.FIG.Common.Middleware.Lib.Sanitization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Provides extension methods to configure and use the sanitization middleware in an ASP.NET Core application.
    /// </summary>
    public static class SanitizationMiddlewareExtensions
    {
        /// <summary>
        /// Adds the sanitization middleware to the application's request pipeline.
        /// </summary>
        /// <param name="builder">The application builder instance.</param>
        /// <param name="configureOptions">
        /// An optional action to configure <see cref="SanitizationOptions"/> before passing them to the middleware.
        /// </param>
        /// <returns>The modified <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder UseSanitization(
            this IApplicationBuilder builder,
            Action<SanitizationOptions>? configureOptions = null)
        {
            // If configuration options are provided, create and configure a new SanitizationOptions instance.
            if (configureOptions != null)
            {
                var options = new SanitizationOptions();
                configureOptions(options);

                // Register the middleware with the configured options.
                builder.UseMiddleware<SanitizationMiddleware>(Options.Create(options));
            }
            else
            {
                // Register the middleware without custom options.
                builder.UseMiddleware<SanitizationMiddleware>();
            }

            return builder;
        }
    }
}
