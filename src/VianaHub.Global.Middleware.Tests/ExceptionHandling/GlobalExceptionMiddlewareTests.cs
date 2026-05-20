// <copyright file="GlobalExceptionMiddlewareTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>

using EBL.FIG.Common.Middleware.Lib.Middleware;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling;

public class GlobalExceptionMiddlewareTests
{
    private static Mock<INotify> CreateNotifyMock()
    {
        var mock = new Mock<INotify>();
        mock.Setup(n => n.HasNotify()).Returns(false);
        mock.Setup(n => n.GetErrorMessage()).Returns(new List<string>());
        return mock;
    }

    private static Mock<ILogger<GlobalExceptionMiddleware>> CreateLoggerMock()
    {
        return new Mock<ILogger<GlobalExceptionMiddleware>>();
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - No exception should invoke next middleware and return 200")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_SemExcecao_DeveInvocarProximoMiddlewareERetornar200()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - General exception should return 500 status code")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_ExcecaoGeral_DeveRetornar500()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Unexpected failure");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - General exception should set content type to application/json")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_ExcecaoGeral_DeveDefinirContentTypeComoApplicationJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Unexpected failure");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - General exception response body should contain Error ID")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_ExcecaoGeral_RespostaDeveConterErrorId()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Unexpected failure");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("Error ID", body);
        Assert.Contains("Contact support", body);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - General exception should call notify.Add with status 500")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_ExcecaoGeral_DeveChamarNotifyAddComStatus500()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Unexpected failure");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        notifyMock.Verify(n => n.Add(It.IsAny<string>(), 500), Times.Once);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - Response body should be valid JSON on general exception")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_ExcecaoGeral_RespostaDeveSerJsonValido()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Failure");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var parsed = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.Equal(JsonValueKind.Object, parsed.ValueKind);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - General exception when response already started should not set status code")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_ExcecaoGeralComRespostaJaIniciada_NaoDeveDefinirStatusCode()
    {
        // Arrange
        var notifyMock = CreateNotifyMock();

        var httpResponseMock = new Mock<HttpResponse>();
        httpResponseMock.SetupGet(r => r.HasStarted).Returns(true);
        httpResponseMock.SetupGet(r => r.StatusCode).Returns(200);
        httpResponseMock.SetupSet(r => r.StatusCode = It.IsAny<int>());
        httpResponseMock.SetupSet(r => r.ContentType = It.IsAny<string>());

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(c => c.Response).Returns(httpResponseMock.Object);

        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(_ => throw new Exception("Crash after write"), loggerMock.Object);

        // Act
        await middleware.InvokeAsync(httpContextMock.Object, notifyMock.Object);

        // Assert
        httpResponseMock.VerifySet(r => r.StatusCode = It.IsAny<int>(), Times.Never);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - General exception response title should be Internal Server Error")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_ExcecaoGeral_RespostaDeveTerTituloInternalServerError()
    {
        // Arrange
        RequestDelegate next = _ => throw new Exception("Random failure");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("Internal Server Error", body);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - ArgumentNullException should return 500")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_ArgumentNullException_DeveRetornar500()
    {
        // Arrange
        RequestDelegate next = _ => throw new ArgumentNullException("parameter");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - InvalidOperationException should return 500")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_InvalidOperationException_DeveRetornar500()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Invalid operation");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - NullReferenceException should return 500")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_NullReferenceException_DeveRetornar500()
    {
        // Arrange
        RequestDelegate next = _ => throw new NullReferenceException("Null reference");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - DivideByZeroException should return 500")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_DivideByZeroException_DeveRetornar500()
    {
        // Arrange
        RequestDelegate next = _ => throw new DivideByZeroException("Division by zero");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact(DisplayName = "GlobalExceptionMiddleware - Custom exception should return 500")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_CustomException_DeveRetornar500()
    {
        // Arrange
        RequestDelegate next = _ => throw new ApplicationException("Custom application exception");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }
}
