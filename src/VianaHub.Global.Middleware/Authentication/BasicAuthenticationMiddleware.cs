using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EBL.FIG.Common.Middleware.Lib.Authentication;

/// <summary>
/// Middleware for handling basic authentication in an ASP.NET Core application.
/// </summary>
public class BasicAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _username;
    private readonly string _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="username">The required username for authentication.</param>
    /// <param name="password">The required password for authentication.</param>
    public BasicAuthenticationMiddleware(RequestDelegate next, string username, string password)
    {
        _next = next;
        _username = username;
        _password = password;
    }

    /// <summary>
    /// Processes the incoming HTTP request and applies basic authentication.
    /// </summary>
    /// <param name="context">The HTTP context of the request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!TryGetAuthenticationHeader(context, out var authHeader))
        {
            SetUnauthorizedResponse(context);
            return;
        }

        // Decode the base64-encoded credentials
        var credentialBytes = Convert.FromBase64String(authHeader);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
        var username = credentials[0];
        var password = credentials[1];

        // Validate credentials
        if (username != _username || password != _password)
        {
            SetUnauthorizedResponse(context);
            return;
        }

        // Proceed to the next middleware in the pipeline
        await _next(context);
    }

    /// <summary>
    /// Extracts the authentication header from the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="authHeader">The extracted authentication header value.</param>
    /// <returns>True if a valid authentication header is found, otherwise false.</returns>
    private static bool TryGetAuthenticationHeader(HttpContext context, out string authHeader)
    {
        authHeader = string.Empty;

        // Check if the Authorization header exists
        if (!context.Request.Headers.ContainsKey("Authorization"))
            return false;

        var header = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);

        // Ensure the authentication scheme is Basic
        if (header.Scheme != "Basic")
            return false;

        authHeader = header.Parameter!;
        return true;
    }

    /// <summary>
    /// Sets the HTTP response to 401 Unauthorized and adds the WWW-Authenticate header.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    private static void SetUnauthorizedResponse(HttpContext context)
    {
        context.Response.Headers["WWW-Authenticate"] = "Basic";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }
}
