// <copyright file="SanitizationMiddleware.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace EBL.FIG.Common.Middleware.Lib.Sanitization
{
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Web;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Middleware that sanitizes incoming requests to prevent potential security risks such as XSS and large payloads.
    /// </summary>
    public class SanitizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SanitizationMiddleware> _logger;
        private readonly SanitizationOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SanitizationMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">Logger instance for logging errors and events.</param>
        /// <param name="options">Configuration options for sanitization.</param>
        public SanitizationMiddleware(
            RequestDelegate next,
            ILogger<SanitizationMiddleware> logger,
            IOptions<SanitizationOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Processes the HTTP request, applying sanitization rules and validation before passing it down the pipeline.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Check if the request content type is allowed
                if (!IsContentTypeAllowed(context.Request.ContentType))
                {
                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    await context.Response.WriteAsJsonAsync(new { error = "Unsupported content type" });
                    return;
                }

                // Check if the request size exceeds the allowed maximum
                if (context.Request.ContentLength > _options.MaxRequestSize)
                {
                    context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                    await context.Response.WriteAsJsonAsync(new { error = "Request size exceeds maximum allowed size" });
                    return;
                }

                // Enable request body buffering for reading and modification
                context.Request.EnableBuffering();

                if (context.Request.ContentLength > 0)
                {
                    var originalBody = context.Request.Body;
                    try
                    {
                        // Read the request body content
                        var requestContent = await new StreamReader(context.Request.Body).ReadToEndAsync();
                        context.Request.Body.Position = 0; // Reset stream position

                        // Validate and sanitize JSON structure if enabled
                        if (_options.ValidateJsonStructure && !string.IsNullOrEmpty(requestContent))
                        {
                            try
                            {
                                using var document = JsonDocument.Parse(requestContent);
                                var sanitizedContent = SanitizeJsonContent(document.RootElement);
                                var newContent = JsonSerializer.Serialize(sanitizedContent);

                                // Replace the original request body with the sanitized content
                                var byteArray = Encoding.UTF8.GetBytes(newContent);
                                var newStream = new MemoryStream(byteArray);
                                context.Request.Body = newStream;
                                context.Request.ContentLength = byteArray.Length;
                            }
                            catch (JsonException)
                            {
                                // Respond with a 400 Bad Request if JSON is invalid
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Response.WriteAsJsonAsync(new { error = "Invalid JSON format" });
                                return;
                            }
                        }
                    }
                    finally
                    {
                        // Restore the original request body
                        context.Request.Body = originalBody;
                    }
                }

                // Proceed to the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log any errors and return a 500 Internal Server Error response
                _logger.LogError(ex, "Error occurred in SanitizationMiddleware");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "An error occurred while processing the request" });
            }
        }

        /// <summary>
        /// Checks if the given content type is allowed.
        /// </summary>
        /// <param name="contentType">The content type to validate.</param>
        /// <returns>True if allowed, otherwise false.</returns>
        private bool IsContentTypeAllowed(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return true;

            return contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Recursively sanitizes JSON content by filtering out dangerous values.
        /// </summary>
        /// <param name="element">The JSON element to sanitize.</param>
        /// <returns>A sanitized version of the JSON object.</returns>
        private object SanitizeJsonContent(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object>();
                    foreach (var property in element.EnumerateObject())
                    {
                        obj[SanitizeString(property.Name)] = SanitizeJsonContent(property.Value);
                    }
                    return obj;

                case JsonValueKind.Array:
                    return element.EnumerateArray()
                        .Select(item => SanitizeJsonContent(item))
                        .ToArray();

                case JsonValueKind.String:
                    return SanitizeString(element.GetString() ?? string.Empty);

                case JsonValueKind.Number:
                    return element.GetDecimal();

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Null:
                    return null!;

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Applies various sanitization techniques to a string input, such as trimming, HTML encoding, and XSS protection.
        /// </summary>
        /// <param name="input">The input string to sanitize.</param>
        /// <returns>The sanitized string.</returns>
        private string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Trim the string if it exceeds the maximum allowed length
            if (input.Length > _options.MaxStringLength)
                input = input[.._options.MaxStringLength];

            // Encode HTML entities if enabled
            if (_options.EnableHtmlEncoding)
                input = HttpUtility.HtmlEncode(input);

            // Remove XSS-related patterns if enabled
            if (_options.EnableXssProtection)
            {
                input = Regex.Replace(input, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, @"javascript:", "", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, @"on\w+\s*=", "", RegexOptions.IgnoreCase);
            }

            // Remove any denied characters from the input
            foreach (var deniedChar in _options.DeniedCharacters)
                input = input.Replace(deniedChar, string.Empty);

            // Validate rule-based expressions if enabled
            if (_options.ValidateRuleExpressions && input.Contains("Expression", StringComparison.OrdinalIgnoreCase))
            {
                input = ValidateRuleExpression(input);
            }

            return input;
        }

        /// <summary>
        /// Validates and removes potentially dangerous rule expressions.
        /// </summary>
        /// <param name="expression">The input expression to validate.</param>
        /// <returns>The sanitized expression.</returns>
        private string ValidateRuleExpression(string expression)
        {
            // Remove references to system-related classes to prevent abuse
            expression = Regex.Replace(expression, @"System\.", "", RegexOptions.IgnoreCase);
            expression = Regex.Replace(expression, @"Process\.", "", RegexOptions.IgnoreCase);
            expression = Regex.Replace(expression, @"File\.", "", RegexOptions.IgnoreCase);
            expression = Regex.Replace(expression, @"Environment\.", "", RegexOptions.IgnoreCase);

            return expression;
        }
    }
}
