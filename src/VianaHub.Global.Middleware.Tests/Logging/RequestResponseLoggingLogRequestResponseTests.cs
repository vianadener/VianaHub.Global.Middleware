// <copyright file="RequestResponseLoggingLogRequestResponseTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using EBL.FIG.Common.Middleware.Lib.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBL.FIG.Common.Middleware.Tests.Logging;

/// <summary>
/// Tests that target the catch block inside LogRequestResponse:
///
///   catch (Exception loggingEx)
///   {
///       _logger.LogError(loggingEx, "Error occurred while logging request/response");
///   }
///
/// Because LogRequestResponse is private, every scenario is exercised through
/// the public InvokeAsync entry point.
///
/// The catch block is reached by injecting a <see cref="JsonSerializerOptions"/>
/// whose custom converter deliberately throws during serialisation, which is the
/// only step inside the try that can be made to fail from outside.
/// </summary>
public class RequestResponseLoggingLogRequestResponseTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Fault-injection helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A <see cref="JsonConverter{T}"/> that always throws a
    /// <see cref="JsonException"/> when serialisation is attempted.
    /// Injecting this into <see cref="JsonSerializerOptions"/> makes
    /// <c>JsonSerializer.Serialize</c> fail inside <c>LogRequestResponse</c>,
    /// which sends execution into the catch block.
    /// </summary>
    private sealed class AlwaysThrowingConverter : JsonConverter<object>
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
                throw new JsonException("Simulated serialisation failure in LogRequestResponse");
    }

    /// <summary>
    /// Returns <see cref="RequestResponseLoggingOptions"/> whose
    /// <see cref="RequestResponseLoggingOptions.JsonSerializerOptions"/>
    /// contain <see cref="AlwaysThrowingConverter"/>, guaranteeing that
    /// <c>JsonSerializer.Serialize</c> throws inside <c>LogRequestResponse</c>.
    /// </summary>
    private static RequestResponseLoggingOptions OptionsWithFaultySerializer() =>
        new()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                Converters = { new AlwaysThrowingConverter() }
            }
        };

    // ─────────────────────────────────────────────────────────────────────────
    // Context / middleware factory helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static DefaultHttpContext CreateHttpContext(
        string path = "/api/test",
        string method = "GET",
        int statusCode = 200)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = method;
        ctx.Request.Path = path;
        ctx.Request.Body = new MemoryStream();
        ctx.Response.Body = new MemoryStream();
        ctx.Response.StatusCode = statusCode;
        return ctx;
    }

    private static DefaultHttpContext CreateHttpContextWithBody(
        string body,
        string path = "/api/test",
        string method = "POST")
    {
        var ctx = CreateHttpContext(path, method);
        ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        ctx.Request.ContentType = "application/json";
        return ctx;
    }

    /// <summary>
    /// Builds a middleware + logger mock pair so tests can verify
    /// calls on the mock without losing the middleware reference.
    /// </summary>
    private static (RequestResponseLoggingMiddleware middleware,
                    Mock<ILogger<RequestResponseLoggingMiddleware>> loggerMock)
        CreateFaultyMiddleware(
            RequestDelegate next,
            RequestResponseLoggingOptions? options = null)
    {
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var opts = Options.Create(options ?? OptionsWithFaultySerializer());
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, opts);
        return (middleware, loggerMock);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Success – catch block swallows the exception; pipeline is NOT broken
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Serialisation failure should not throw to the caller")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacao_NaoDevePropagar()
    {
        // Arrange
        var (middleware, _) = CreateFaultyMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContext();

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert – the catch block must swallow the serialisation exception
        Assert.Null(exception);
    }

    [Fact(DisplayName = "Serialisation failure should still invoke next middleware")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacao_DeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextCalled = false;
        var (middleware, _) = CreateFaultyMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Serialisation failure should call LogError with the caught exception")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacao_DeveChamarLogErrorComExcecaoCapturada()
    {
        // Arrange
        var (middleware, loggerMock) = CreateFaultyMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert – LogError must be called with the exception that was caught
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsNotNull<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Serialisation failure error message should contain 'Error occurred while logging request/response'")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacao_MensagemDeErroDeveConterTextoEsperado()
    {
        // Arrange
        var (middleware, loggerMock) = CreateFaultyMiddleware(_ => Task.CompletedTask);
        string? capturedMessage = null;

        loggerMock
            .Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsNotNull<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception?, Delegate>(
                (_, _, state, _, formatter) =>
                    capturedMessage = formatter.DynamicInvoke(state, null) as string);

        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.Contains("Error occurred while logging request/response", capturedMessage);
    }

    [Fact(DisplayName = "Serialisation failure should restore response body stream")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacao_DeveRestaurarStreamDeResposta()
    {
        // Arrange
        var (middleware, _) = CreateFaultyMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContext();
        var originalStream = context.Response.Body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert – the finally block must restore the stream regardless of catch activity
        Assert.Same(originalStream, context.Response.Body);
    }

    [Fact(DisplayName = "Serialisation failure on error response should still call LogError")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacaoEmRespostaDeErro_DeveChamarLogError()
    {
        // Arrange
        var (middleware, loggerMock) = CreateFaultyMiddleware(ctx =>
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
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsNotNull<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Serialisation failure on 400 response should not throw to the caller")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacaoEmResposta400_NaoDevePropagar()
    {
        // Arrange
        var (middleware, _) = CreateFaultyMiddleware(ctx =>
        {
            ctx.Response.StatusCode = 400;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Null(exception);
    }

    [Fact(DisplayName = "Serialisation failure with request body should not throw to the caller")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacaoComBody_NaoDevePropagar()
    {
        // Arrange
        var (middleware, _) = CreateFaultyMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContextWithBody("{\"key\":\"value\"}");

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Null(exception);
    }

    [Fact(DisplayName = "Serialisation failure with request body should call LogError with the caught exception")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacaoComBody_DeveChamarLogErrorComExcecao()
    {
        // Arrange
        var (middleware, loggerMock) = CreateFaultyMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContextWithBody("{\"order\":42}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsNotNull<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Failure path routed through the inner catch (next throws → LogRequestResponse
    // is called from the inner catch of InvokeAsync with the pipeline exception)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Serialisation failure during exception path should not suppress pipeline exception")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacaoNoCaminhoDeExcecao_NaoDeveSufocarExcecaoDoPipeline()
    {
        // Arrange – next throws, which calls LogRequestResponse(… ex) from the inner catch.
        // LogRequestResponse itself then fails during serialisation, entering the catch block.
        // The pipeline exception must still be re-thrown.
        var (middleware, _) = CreateFaultyMiddleware(
            _ => throw new InvalidOperationException("pipeline failure"));
        var context = CreateHttpContext();

        // Act & Assert – the original pipeline exception propagates
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));
    }

    [Fact(DisplayName = "Serialisation failure during exception path should call LogError with the serialisation exception")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacaoNoCaminhoDeExcecao_DeveChamarLogErrorComExcecaoDeSerializacao()
    {
        // Arrange
        var (middleware, loggerMock) = CreateFaultyMiddleware(
            _ => throw new InvalidOperationException("pipeline failure"));
        var context = CreateHttpContext();

        // Act
        try { await middleware.InvokeAsync(context); } catch { /* expected pipeline exception */ }

        // Assert – the catch block in LogRequestResponse logged its own serialisation error
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsNotNull<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Serialisation failure during exception path should restore response body stream")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_FalhaSerializacaoNoCaminhoDeExcecao_DeveRestaurarStreamDeResposta()
    {
        // Arrange
        var (middleware, _) = CreateFaultyMiddleware(
            _ => throw new Exception("crash"));
        var context = CreateHttpContext();
        var originalStream = context.Response.Body;

        // Act
        try { await middleware.InvokeAsync(context); } catch { /* expected */ }

        // Assert
        Assert.Same(originalStream, context.Response.Body);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Negative guard – with a healthy serialiser the catch block is NOT entered
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Healthy serialiser should not call LogError with an exception")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_SerializadorSaudavel_NaoDeveChamarLogErrorComExcecao()
    {
        // Arrange – default options use a healthy JsonSerializerOptions
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var opts = Options.Create(new RequestResponseLoggingOptions());
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, opts);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert – LogError must NOT be called with a non-null exception
        // (it may be called with null, which is the normal information log path)
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsNotNull<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact(DisplayName = "Healthy serialiser on error response should not enter the catch block")]
    [Trait("Logging", "")]
    public async Task LogRequestResponse_SerializadorSaudavelEmRespostaDeErro_NaoDeveEntrarNoCatch()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var opts = Options.Create(new RequestResponseLoggingOptions());
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 500;
            return Task.CompletedTask;
        };
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, opts);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert – the Error log here comes from the normal error-level logging, not from
        // the catch block; we verify it was NOT called with a non-null exception object
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsNotNull<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
