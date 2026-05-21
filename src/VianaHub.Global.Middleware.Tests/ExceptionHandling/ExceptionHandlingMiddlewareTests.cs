using EBL.FIG.Common.Middleware.Lib.ExceptionHandling.Exceptions;
using EBL.FIG.Common.Middleware.Lib.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling;

public class ExceptionHandlingMiddlewareTests
{
    private static DefaultHttpContext CreateHttpContext() => new();

    private static Mock<ILogger<ExceptionHandlingMiddleware>> CreateLoggerMock()
    {
        return new Mock<ILogger<ExceptionHandlingMiddleware>>();
    }

    private static ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next)
    {
        var loggerMock = CreateLoggerMock();
        return new ExceptionHandlingMiddleware(next, loggerMock.Object);
    }

    #region Sucesso

    [Fact(DisplayName = "ExceptionHandlingMiddleware - No exception should invoke next middleware and return 200")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_SemExcecao_DeveInvocarProximoMiddlewareERetornar200()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - NotFoundException should return 404 status code")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_NotFoundException_DeveRetornar404()
    {
        // Arrange
        RequestDelegate next = _ => throw new NotFoundException("Resource not found");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - NotFoundException should set content type to application/json")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_NotFoundException_DeveDefinirContentTypeComoApplicationJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new NotFoundException("Resource not found");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - BadRequestException should return 400 status code")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_BadRequestException_DeveRetornar400()
    {
        // Arrange
        RequestDelegate next = _ => throw new BadRequestException("Invalid payload");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - BadRequestException should set content type to application/json")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_BadRequestException_DeveDefinirContentTypeComoApplicationJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new BadRequestException("Invalid payload");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - ConflictException should return 409 status code")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_ConflictException_DeveRetornar409()
    {
        // Arrange
        RequestDelegate next = _ => throw new ConflictException("Duplicate resource");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - ConflictException should set content type to application/json")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_ConflictException_DeveDefinirContentTypeComoApplicationJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new ConflictException("Duplicate resource");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - Unhandled exception should return 500 status code")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_ExcecaoNaoTratada_DeveRetornar500()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Unexpected failure");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - Unhandled exception should set content type to application/json")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_ExcecaoNaoTratada_DeveDefinirContentTypeComoApplicationJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Unexpected failure");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - CorrelationId from context items should be used in error details")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_ComCorrelationIdNoContextItems_DeveUsarCorrelationIdNosDetalhesDoErro()
    {
        // Arrange
        const string correlationId = "test-correlation-id";
        RequestDelegate next = _ => throw new NotFoundException("Not found");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        context.Items["X-Correlation-ID"] = correlationId;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "ExceptionHandlingMiddleware - Null HttpContext should throw ArgumentNullException")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_HttpContextNulo_DeveLancarArgumentNullException()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => middleware.InvokeAsync(null!));
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - Null next delegate in constructor should throw ArgumentNullException")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_NextNulo_DeveLancarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new ExceptionHandlingMiddleware(null!, CreateLoggerMock().Object));
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - NotFoundException should not return 500 status code")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_NotFoundException_NaoDeveRetornar500()
    {
        // Arrange
        RequestDelegate next = _ => throw new NotFoundException("Not found");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotEqual(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact(DisplayName = "ExceptionHandlingMiddleware - ConflictException should not return 404 status code")]
    [Trait("ExceptionHandling", "")]
    public async Task InvokeAsync_ConflictException_NaoDeveRetornar404()
    {
        // Arrange
        RequestDelegate next = _ => throw new ConflictException("Conflict");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotEqual(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    #endregion
}
