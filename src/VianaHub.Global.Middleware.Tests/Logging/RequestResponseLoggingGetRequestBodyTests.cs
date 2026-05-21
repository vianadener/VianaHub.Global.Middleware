// <copyright file="RequestResponseLoggingGetRequestBodyTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using EBL.FIG.Common.Middleware.Lib.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace EBL.FIG.Common.Middleware.Tests.Logging;

/// <summary>
/// Tests that target the GetRequestBodyAsync method of RequestResponseLoggingMiddleware,
/// specifically the branch:
///
///   if (!request.Body.CanSeek)
///   {
///       request.EnableBuffering();
///   }
///
/// Because GetRequestBodyAsync is private, every scenario is driven through
/// the public InvokeAsync entry point.
/// </summary>
public class RequestResponseLoggingGetRequestBodyTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Stream helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A stream that deliberately returns CanSeek = false so the middleware
    /// is forced to call request.EnableBuffering() before reading the body.
    /// The stream wraps a MemoryStream so reads still work correctly.
    /// </summary>
    private sealed class NonSeekableStream : Stream
    {
        private readonly MemoryStream _inner;

        public NonSeekableStream(byte[] data) => _inner = new MemoryStream(data);

        public override bool CanRead => true;
        public override bool CanSeek => false;           // ← forces EnableBuffering branch
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static RequestResponseLoggingMiddleware CreateMiddleware(
        RequestDelegate next,
        RequestResponseLoggingOptions? options = null)
    {
        var logger = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
        var optionsWrapper = Options.Create(options ?? new RequestResponseLoggingOptions());
        return new RequestResponseLoggingMiddleware(next, logger.Object, optionsWrapper);
    }

    /// <summary>
    /// Builds a context whose request body is a plain seekable MemoryStream
    /// (CanSeek == true) — the EnableBuffering branch is NOT taken.
    /// </summary>
    private static DefaultHttpContext CreateContextWithSeekableBody(string bodyContent)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/test";
        context.Request.ContentType = "application/json";

        var bytes = Encoding.UTF8.GetBytes(bodyContent);
        context.Request.Body = new MemoryStream(bytes);   // CanSeek == true
        context.Response.Body = new MemoryStream();
        return context;
    }

    /// <summary>
    /// Builds a context whose request body is a NonSeekableStream
    /// (CanSeek == false) — the EnableBuffering branch IS taken.
    /// </summary>
    private static DefaultHttpContext CreateContextWithNonSeekableBody(string bodyContent)
    {
        var bytes = Encoding.UTF8.GetBytes(bodyContent);

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/test";
        context.Request.ContentType = "application/json";
        context.Request.Body = new NonSeekableStream(bytes);  // CanSeek == false
        context.Response.Body = new MemoryStream();
        return context;
    }

    /// <summary>
    /// Builds a context with an empty non-seekable body.
    /// </summary>
    private static DefaultHttpContext CreateContextWithEmptyNonSeekableBody()
        => CreateContextWithNonSeekableBody(string.Empty);

    // ─────────────────────────────────────────────────────────────────────────
    // Success – seekable body (CanSeek == true, EnableBuffering NOT called)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Seekable body should complete pipeline without exception")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_SeekableBody_DeveConcluirPipelineSemExcecao()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithSeekableBody("{\"id\":1}");

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Null(exception);
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Seekable body should keep request body seekable after middleware executes")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_SeekableBody_BodyDevePermanezerSeekableAposExecucao()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithSeekableBody("{\"key\":\"value\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert – the body stream still supports seeking after the middleware ran
        Assert.True(context.Request.Body.CanSeek);
    }

    [Fact(DisplayName = "Seekable body should allow downstream handler to read body content after middleware")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_SeekableBody_DownstreamDevePoderLerBodyAposMiddleware()
    {
        // Arrange
        const string expectedBody = "{\"name\":\"test\"}";
        string? capturedBody = null;

        RequestDelegate next = async ctx =>
        {
            ctx.Request.Body.Position = 0;
            using var reader = new StreamReader(ctx.Request.Body, leaveOpen: true);
            capturedBody = await reader.ReadToEndAsync();
        };

        var middleware = CreateMiddleware(next);
        var context = CreateContextWithSeekableBody(expectedBody);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(expectedBody, capturedBody);
    }

    [Fact(DisplayName = "Seekable body with empty content should complete pipeline without exception")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_SeekableBodyVazio_DeveConcluirPipelineSemExcecao()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithSeekableBody(string.Empty);

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Null(exception);
    }

    [Fact(DisplayName = "Seekable body with large content should complete pipeline without exception")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_SeekableBodyGrande_DeveConcluirPipelineSemExcecao()
    {
        // Arrange
        var largeBody = new string('x', 64 * 1024); // 64 KB
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithSeekableBody(largeBody);

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Null(exception);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Success – non-seekable body (CanSeek == false, EnableBuffering IS called)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Non-seekable body should trigger EnableBuffering and complete pipeline without exception")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_BodyNaoSeekable_DeveAcionarEnableBufferingEConcluirSemExcecao()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithNonSeekableBody("{\"id\":42}");

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert – EnableBuffering replaces the body with a seekable stream; pipeline continues
        Assert.Null(exception);
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Non-seekable body should be replaced with a seekable stream after EnableBuffering")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_BodyNaoSeekable_DeveSerSubstituídoPorStreamSeekable()
    {
        // Arrange
        bool? bodyIsSeekableInsideNext = null;
        RequestDelegate next = ctx =>
        {
            bodyIsSeekableInsideNext = ctx.Request.Body.CanSeek;
            return Task.CompletedTask;
        };

        var middleware = CreateMiddleware(next);
        var context = CreateContextWithNonSeekableBody("{\"value\":99}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert – after EnableBuffering the body is seekable
        Assert.NotNull(bodyIsSeekableInsideNext);
        Assert.True(bodyIsSeekableInsideNext);
    }

    [Fact(DisplayName = "Non-seekable body content should be readable by downstream handler after EnableBuffering")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_BodyNaoSeekable_ConteudoDeveSerLegivelPeloDownstream()
    {
        // Arrange
        const string expectedBody = "{\"product\":\"widget\"}";
        string? capturedBody = null;

        RequestDelegate next = async ctx =>
        {
            ctx.Request.Body.Position = 0;
            using var reader = new StreamReader(ctx.Request.Body, leaveOpen: true);
            capturedBody = await reader.ReadToEndAsync();
        };

        var middleware = CreateMiddleware(next);
        var context = CreateContextWithNonSeekableBody(expectedBody);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(expectedBody, capturedBody);
    }

    [Fact(DisplayName = "Non-seekable empty body should complete pipeline without exception")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_BodyNaoSeekableVazio_DeveConcluirPipelineSemExcecao()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithEmptyNonSeekableBody();

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Null(exception);
    }

    [Fact(DisplayName = "Non-seekable body should not affect response body stream")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_BodyNaoSeekable_NaoDeveAfetarStreamDeResposta()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithNonSeekableBody("{\"ok\":true}");
        var originalResponseStream = context.Response.Body;

        // Act
        await middleware.InvokeAsync(context);

        // Assert – response body stream must be restored to the original
        Assert.Same(originalResponseStream, context.Response.Body);
    }

    [Fact(DisplayName = "Non-seekable body with large content should complete pipeline without exception")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_BodyNaoSeekableGrande_DeveConcluirPipelineSemExcecao()
    {
        // Arrange
        var largeBody = new string('z', 64 * 1024); // 64 KB
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithNonSeekableBody(largeBody);

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Null(exception);
    }

    [Fact(DisplayName = "Non-seekable body should allow logger to receive the request body content")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_BodyNaoSeekable_LoggerDeveReceberConteudoDoBody()
    {
        // Arrange
        const string expectedContent = "non-seekable-payload";
        var loggerMock = new Mock<ILogger<RequestResponseLoggingMiddleware>>();
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

        var optionsWrapper = Options.Create(new RequestResponseLoggingOptions());
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggingMiddleware(next, loggerMock.Object, optionsWrapper);
        var context = CreateContextWithNonSeekableBody(expectedContent);

        // Act
        await middleware.InvokeAsync(context);

        // Assert – the body content must appear somewhere in the log entry
        Assert.NotNull(capturedLog);
        Assert.Contains(expectedContent, capturedLog);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Failure – non-seekable body with downstream exception
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Non-seekable body when next middleware throws should rethrow exception")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_BodyNaoSeekableProximoLancaExcecao_DevePropagar()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("downstream failure");
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithNonSeekableBody("{\"crash\":true}");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));
    }

    [Fact(DisplayName = "Non-seekable body when next middleware throws should restore response body stream")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_BodyNaoSeekableProximoLancaExcecao_DeveRestaurarStreamDeResposta()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("crash");
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithNonSeekableBody("{\"x\":1}");
        var originalResponseStream = context.Response.Body;

        // Act
        try { await middleware.InvokeAsync(context); } catch { /* expected */ }

        // Assert – the finally block must always restore the response stream
        Assert.Same(originalResponseStream, context.Response.Body);
    }

    [Fact(DisplayName = "Seekable body when next middleware throws should rethrow exception")]
    [Trait("Logging", "")]
    public async Task GetRequestBodyAsync_SeekableBodyProximoLancaExcecao_DevePropagar()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("seekable crash");
        var middleware = CreateMiddleware(next);
        var context = CreateContextWithSeekableBody("{\"fail\":true}");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));
    }
}
