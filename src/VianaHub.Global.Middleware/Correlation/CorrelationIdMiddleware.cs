// <copyright file="CorrelationIdMiddleware.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;

namespace EBL.FIG.Common.Middleware.Lib.Correlation;

/// <summary>
/// Middleware responsible for handling Correlation IDs in HTTP requests.
/// It ensures that each request has a unique identifier, which can be used for tracking requests across services.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware component in the pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Middleware logic for processing the incoming HTTP request and ensuring a Correlation ID is present.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        context.Items[CorrelationIdHeader] = correlationId;

        await _next(context);
    }
}
