// <copyright file="RequestResponseLoggingMiddlewareTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using EBL.FIG.Common.Middleware.Lib.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;
using System.Text.Json;

namespace EBL.FIG.Common.Middleware.Tests.Logging;

public class RequestResponseLoggingMiddlewareTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static RequestResponseLoggingMiddleware CreateMiddleware(
        RequestDelegate next,
        RequestResponseLoggingOptions? options = null)
    {
        var logger = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var optionsWrapper = Options.Create(options ?? new RequestResponseLoggingOptions());
        return new RequestResponseLoggingMiddleware(next, logger.Object, optionsWrapper);
    }

    private static DefaultHttpContext CreateHttpContext(string path = "/api/test", string method = "GET")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.Body = new MemoryStream();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static DefaultHttpContext CreateHttpContextWithBody(string body, string path = "/api/test", string method = "POST")
    {
        var context = CreateHttpContext(path, method);
        var bytes = Encoding.UTF8.GetBytes(body);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentType = "application/json";
        return context;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Success
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "InvokeAsync with matching path should invoke next middleware")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_PathCompativel_DeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext("/api/test");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "InvokeAsync with path filter matching should invoke next middleware")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_FiltroPathCompativel_DeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var options = new RequestResponseLoggingOptions
        {
            PathFilters = new List<string> { "/api" }
        };

        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext("/api/values");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "InvokeAsync with path filter not matching should skip logging and invoke next")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_FiltroPathNaoCompativel_DevePassarSemLogar()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var options = new RequestResponseLoggingOptions
        {
            PathFilters = new List<string> { "/api" }
        };

        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext("/health");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "InvokeAsync should restore original response body stream after execution")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_DeveRestaurarStreamOriginalDoResponseBody()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var originalStream = context.Response.Body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Same(originalStream, context.Response.Body);
    }

    [Fact(DisplayName = "InvokeAsync should log Information for 200 response")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_RespostaOk_DeveLogarInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var optionsWrapper = Options.Create(new RequestResponseLoggingOptions());
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };

        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "InvokeAsync should log Error for 500 response")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_RespostaErroServidor_DeveLogarError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var optionsWrapper = Options.Create(new RequestResponseLoggingOptions());
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 500;
            return Task.CompletedTask;
        };

        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "InvokeAsync should log Error for 400 response")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_RespostaBadRequest_DeveLogarError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var optionsWrapper = Options.Create(new RequestResponseLoggingOptions());
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 400;
            return Task.CompletedTask;
        };

        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "InvokeAsync with LogErrorsOnly=true and 200 response should not log")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_LogErrorsOnlyHabilitadoComRespostaSucesso_NaoDeveLogar()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var options = new RequestResponseLoggingOptions { LogErrorsOnly = true };
        var optionsWrapper = Options.Create(options);
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };

        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "InvokeAsync with LogErrorsOnly=true and 500 response should log Error")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_LogErrorsOnlyHabilitadoComRespostaErro_DeveLogarError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var options = new RequestResponseLoggingOptions { LogErrorsOnly = true };
        var optionsWrapper = Options.Create(options);
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 500;
            return Task.CompletedTask;
        };

        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "InvokeAsync with request body should capture body without breaking pipeline")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_ComBodyNaRequisicao_DeveLerBodySemQuebrarPipeline()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = CreateMiddleware(next);
        var context = CreateHttpContextWithBody("{\"key\":\"value\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "InvokeAsync should redact sensitive Authorization header")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_HeaderSensivelAuthorization_DeveSerRedacted()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var optionsWrapper = Options.Create(new RequestResponseLoggingOptions());

        string? capturedLog = null;
        loggerMock
            .Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception?, Delegate>((_, _, state, _, formatter) =>
            {
                capturedLog = formatter.DynamicInvoke(state, null) as string;
            });

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext();
        context.Request.Headers["Authorization"] = "Bearer super-secret-token";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Contains("[REDACTED]", capturedLog);
        Assert.DoesNotContain("super-secret-token", capturedLog);
    }

    [Fact(DisplayName = "InvokeAsync should redact sensitive Cookie header")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_HeaderSensivelCookie_DeveSerRedacted()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var optionsWrapper = Options.Create(new RequestResponseLoggingOptions());

        string? capturedLog = null;
        loggerMock
            .Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception?, Delegate>((_, _, state, _, formatter) =>
            {
                capturedLog = formatter.DynamicInvoke(state, null) as string;
            });

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext();
        context.Request.Headers["Cookie"] = "session=abc123";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Contains("[REDACTED]", capturedLog);
        Assert.DoesNotContain("session=abc123", capturedLog);
    }

    [Fact(DisplayName = "InvokeAsync with empty PathFilters should log all paths")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_FiltroPathVazio_DeveLogarTodosOsCaminhos()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var options = new RequestResponseLoggingOptions { PathFilters = new List<string>() };
        var optionsWrapper = Options.Create(options);
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext("/any/arbitrary/path");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Failure
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "InvokeAsync when next middleware throws should rethrow exception")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_ProximoMiddlewareLancaExcecao_DevePropagar()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Pipeline error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));
    }

    [Fact(DisplayName = "InvokeAsync when next middleware throws should log Error before rethrowing")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_ProximoMiddlewareLancaExcecao_DeveLogarErrorAntesDeRelançar()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var optionsWrapper = Options.Create(new RequestResponseLoggingOptions());
        RequestDelegate next = _ => throw new InvalidOperationException("Pipeline error");
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext();

        // Act
        try { await middleware.InvokeAsync(context); } catch { /* expected */ }

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "InvokeAsync when path filter does not match should not log")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_FiltroPathNaoCompativel_NaoDeveLogar()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var options = new RequestResponseLoggingOptions
        {
            PathFilters = new List<string> { "/api" }
        };
        var optionsWrapper = Options.Create(options);
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateHttpContext("/health/live");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "InvokeAsync should always restore response body stream even when exception occurs")]
    [Trait("Logging", "")]
    public async Task InvokeAsync_ExcecaoNoPipeline_DeveRestaurarStreamOriginalDoBody()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Unexpected error");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var originalStream = context.Response.Body;

        // Act
        try { await middleware.InvokeAsync(context); } catch { /* expected */ }

        // Assert
        Assert.Same(originalStream, context.Response.Body);
    }
}
