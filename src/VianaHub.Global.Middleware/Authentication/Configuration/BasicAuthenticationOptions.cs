namespace EBL.FIG.Common.Middleware.Lib.Authentication.Configuration;

/// <summary>
/// Represents the configuration options for basic authentication.
/// </summary>
public class BasicAuthenticationOptions
{
    /// <summary>
    /// Gets or sets the username required for authentication.
    /// Default value is an empty string.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password required for authentication.
    /// Default value is an empty string.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
