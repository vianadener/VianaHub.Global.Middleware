using VianaHub.Global.Middleware.Lib.Authentication.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace VianaHub.Global.Middleware.Lib.Authentication.Extensions;

/// <summary>
/// Provides extension methods to configure and use basic authentication middleware.
/// </summary>
public static class BasicAuthenticationMiddlewareExtensions
{
    /// <summary>
    /// Registers the basic authentication configuration in the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">The action to configure authentication options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBasicAuthentication(this IServiceCollection services, Action<BasicAuthenticationOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Adds the basic authentication middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <returns>The application builder with middleware configured.</returns>
    public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app, string username, string password)
    {
        return app.UseMiddleware<BasicAuthenticationMiddleware>(username, password);
    }
}
