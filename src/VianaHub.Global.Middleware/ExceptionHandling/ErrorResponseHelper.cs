// <copyright file="ErrorResponseHelper.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>

namespace VianaHub.Global.Middleware.Lib.ExceptionHandling;

using VianaHub.Global.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

/// <summary>
/// Helper class providing shared functionality for error handling middlewares.
/// Centralizes common operations like error ID generation, JSON serialization, and response writing.
/// </summary>
public static class ErrorResponseHelper
{
    /// <summary>
    /// Generates a unique error identifier (12-character GUID).
    /// </summary>
    /// <returns>A 12-character string identifier.</returns>
    public static string GenerateErrorId() => Guid.NewGuid().ToString("N")[..12];

    /// <summary>
    /// Returns standardized JSON serialization options used across all error responses.
    /// </summary>
    /// <returns>JsonSerializerOptions configured with camelCase naming and indentation.</returns>
    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
    }

    /// <summary>
    /// Writes a JSON error response to the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="errorResponse">The error response to serialize.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task WriteJsonResponseAsync(HttpContext context, ErrorResponse errorResponse, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var json = JsonSerializer.Serialize(errorResponse, GetJsonSerializerOptions());
        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Gets a friendly title for the given HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A string representation of the status code.</returns>
    public static string GetErrorTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "BadRequest",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "NotFound",
            409 => "Conflict",
            410 => "Gone",
            422 => "UnprocessableEntity",
            429 => "TooManyRequests",
            500 => "InternalServerError",
            503 => "ServiceUnavailable",
            _ => "BadRequest"
        };
    }

    /// <summary>
    /// Extracts the correlation ID from the HTTP context headers.
    /// Checks multiple common header names: X-Correlation-ID, X-Correlation-Id, CorrelationId.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The correlation ID if found, otherwise "N/A".</returns>
    public static string GetCorrelationId(HttpContext context)
    {
        if (context?.Request?.Headers == null)
        {
            return "N/A";
        }

        // Try multiple common header names
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                         ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                         ?? context.Request.Headers["CorrelationId"].FirstOrDefault()
                         ?? context.Items["X-Correlation-ID"]?.ToString();

        return string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("N")[..12] : correlationId;
    }
}
