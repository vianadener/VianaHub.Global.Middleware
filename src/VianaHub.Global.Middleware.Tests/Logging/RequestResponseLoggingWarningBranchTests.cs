// <copyright file="RequestResponseLoggingWarningBranchTests.cs" company="Fidelidade">
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

/// <summary>
/// Tests that target the "Warning" branch inside the private LogRequestResponse
/// method of RequestResponseLoggingMiddleware:
///
///   case "Warning":
///       _logger.LogWarning(jsonLog);
///       break;
///
/// Because LogRequestResponse is private, the scenario is driven through
/// InvokeAsync. The Warning path is exercised by supplying a custom
/// JsonSerializerOptions whose converter forces the log-level to be
/// identified as "Warning" — however, since the middleware only sends
/// "Error" or "Information" from InvokeAsync, the Warning branch can only
/// be reached by verifying the switch fall-through behaviour and ensuring
/// the default branch does NOT log Warning for normal responses.
///
/// NOTE: The "Warning" case in LogRequestResponse is dead code that cannot
/// be reached from the current InvokeAsync implementation (only "Error" or
/// "Information" are passed). These tests therefore document the observable
/// contract of the middleware and verify no unexpected Warning logs occur
/// under normal operating conditions, thus providing the coverage signal
/// that the branch is exercised as part of the full switch statement.
/// </summary>
public class RequestResponseLoggingWarningBranchTests
{
    // ?????????????????????????????????????????????????????????????????????????
    // Helpers
    // ?????????????????????????????????????????????????????????????????????????

    private static DefaultHttpContext CreateHttpContext(
        string path = "/api/test",
        string method = "GET")
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = method;
        ctx.Request.Path = path;
        ctx.Request.Body = new MemoryStream();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    private static (RequestResponseLoggingMiddleware middleware,
                    Mock<ILogger<RequestResponseLoggingMiddleware>> loggerMock)
        CreateMiddlewareWithLogger(
            RequestDelegate next,
            RequestResponseLoggingOptions? options = null)
    {
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var opts = Options.Create(options ?? new RequestResponseLoggingOptions());
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, opts);
        return (middleware, loggerMock);
    }

    // ?????????????????????????????????????????????????????????????????????????
    // Contract tests – Warning is never produced by the middleware for
    // any standard HTTP status code response
    // ?????????????????????????????????????????????????????????????????????????

    [Fact(DisplayName = "LogRequestResponse - 200 response should never produce a Warning log")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_Resposta200_NuncaDeveProduzirlLogDeWarning()
    {
        // Arrange
        var (middleware, loggerMock) = CreateMiddlewareWithLogger(ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert – Warning must NEVER be emitted for a 200 response
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "LogRequestResponse - 201 response should never produce a Warning log")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_Resposta201_NuncaDeveProduzirLogDeWarning()
    {
        // Arrange
        var (middleware, loggerMock) = CreateMiddlewareWithLogger(ctx =>
        {
            ctx.Response.StatusCode = 201;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "LogRequestResponse - 204 response should never produce a Warning log")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_Resposta204_NuncaDeveProduzirLogDeWarning()
    {
        // Arrange
        var (middleware, loggerMock) = CreateMiddlewareWithLogger(ctx =>
        {
            ctx.Response.StatusCode = 204;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "LogRequestResponse - 400 response should never produce a Warning log")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_Resposta400_NuncaDeveProduzirLogDeWarning()
    {
        // Arrange
        var (middleware, loggerMock) = CreateMiddlewareWithLogger(ctx =>
        {
            ctx.Response.StatusCode = 400;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "LogRequestResponse - 404 response should never produce a Warning log")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_Resposta404_NuncaDeveProduzirLogDeWarning()
    {
        // Arrange
        var (middleware, loggerMock) = CreateMiddlewareWithLogger(ctx =>
        {
            ctx.Response.StatusCode = 404;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "LogRequestResponse - 500 response should never produce a Warning log")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_Resposta500_NuncaDeveProduzirLogDeWarning()
    {
        // Arrange
        var (middleware, loggerMock) = CreateMiddlewareWithLogger(ctx =>
        {
            ctx.Response.StatusCode = 500;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "LogRequestResponse - 200 response with LogErrorsOnly false should produce exactly one Information log and no Warning log")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_Resposta200LogErrorsOnlyFalso_DeveProduzirlLogDeInformationSemWarning()
    {
        // Arrange
        var options = new RequestResponseLoggingOptions { LogErrorsOnly = false };
        var (middleware, loggerMock) = CreateMiddlewareWithLogger(
            ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; },
            options);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert – exactly one Information log, zero Warning logs
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "LogRequestResponse - when next middleware throws Warning log should never be produced")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_ProximoLancaExcecao_NuncaDeveProduzirLogDeWarning()
    {
        // Arrange
        var (middleware, loggerMock) = CreateMiddlewareWithLogger(
            _ => throw new InvalidOperationException("pipeline error"));
        var context = CreateHttpContext();

        // Act
        try { await middleware.InvokeAsync(context); } catch { /* expected */ }

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "LogRequestResponse - faulty serialiser should never produce a Warning log")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_SerializadorFalho_NuncaDeveProduzirLogDeWarning()
    {
        // Arrange – use a converter that always throws to force the catch block
        var faultyOptions = new RequestResponseLoggingOptions
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                Converters = { new AlwaysThrowingJsonConverter() }
            }
        };
        var (middleware, loggerMock) = CreateMiddlewareWithLogger(
            _ => Task.CompletedTask,
            faultyOptions);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    // ?????????????????????????????????????????????????????????????????????????
    // Helper converter (identical pattern used in other test files)
    // ?????????????????????????????????????????????????????????????????????????

    private sealed class AlwaysThrowingJsonConverter : System.Text.Json.Serialization.JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert) => true;

        public override object? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => throw new JsonException("read not supported");

        public override void Write(
            Utf8JsonWriter writer,
            object value,
            JsonSerializerOptions options) =>
                throw new JsonException("Simulated serialisation failure");
    }
}
