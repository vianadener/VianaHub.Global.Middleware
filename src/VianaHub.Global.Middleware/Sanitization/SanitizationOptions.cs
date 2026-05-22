// <copyright file="SanitizationOptions.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace VianaHub.Global.Middleware.Lib.Sanitization;

/// <summary>
/// Configuration options for the sanitization middleware.
/// </summary>
public class SanitizationOptions
{
    /// <summary>
    /// Maximum allowed request size in bytes. Default is 10MB (10 * 1024 * 1024 bytes).
    /// </summary>
    public int MaxRequestSize { get; set; } = 10485760;

    /// <summary>
    /// Specifies whether HTML encoding should be applied to string inputs to prevent HTML injection.
    /// </summary>
    public bool EnableHtmlEncoding { get; set; } = true;

    /// <summary>
    /// Specifies whether Cross-Site Scripting (XSS) protection should be applied to string inputs.
    /// </summary>
    public bool EnableXssProtection { get; set; } = true;

    /// <summary>
    /// A list of characters that are explicitly allowed in string inputs.
    /// </summary>
    public string[] AllowedCharacters { get; set; } = new[] { "@", "_", "-", ".", ",", " " };

    /// <summary>
    /// A list of characters that are explicitly denied in string inputs to prevent potential security risks.
    /// </summary>
    public string[] DeniedCharacters { get; set; } = new[] { "<", ">", "'", "\"", ";", "=", "(", ")", "{", "}" };

    /// <summary>
    /// Maximum allowed length for string inputs to prevent excessively large input values.
    /// </summary>
    public int MaxStringLength { get; set; } = 4000;

    /// <summary>
    /// Specifies whether rule expressions should be validated to prevent potentially dangerous expressions.
    /// </summary>
    public bool ValidateRuleExpressions { get; set; } = true;

    /// <summary>
    /// Specifies whether incoming JSON request bodies should be validated for structural correctness.
    /// </summary>
    public bool ValidateJsonStructure { get; set; } = true;
}
