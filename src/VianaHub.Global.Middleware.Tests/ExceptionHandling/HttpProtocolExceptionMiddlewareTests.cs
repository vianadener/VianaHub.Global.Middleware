// <copyright file="HttpProtocolExceptionMiddlewareTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>

using EBL.FIG.Common.Middleware.Lib.Middleware;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling;

public class HttpProtocolExceptionMiddlewareTests
{
    private static Mock<INotify> CreateNotifyMock()
    {
        var mock = new Mock<INotify>();
        mock.Setup(n => n.HasNotify()).Returns(false);
        mock.Setup(n => n.GetErrorMessage()).Returns(new List<string>());
        return mock;
    }

    private static Mock<ILogger<HttpProtocolExceptionMiddleware>> CreateLoggerMock()
    {
        return new Mock<ILogger<HttpProtocolExceptionMiddleware>>();
    }

    [Fact(DisplayName = "HttpProtocolExceptionMiddleware - BadHttpRequestException without JsonException inner should return 400")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_BadHttpRequestExceptionSemJsonException_DeveRetornar400()
    {
        // Arrange
        RequestDelegate next = _ => throw new BadHttpRequestException("Malformed request body");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new HttpProtocolExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact(DisplayName = "HttpProtocolExceptionMiddleware - BadHttpRequestException should set content type to application/json")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_BadHttpRequestException_DeveDefinirContentTypeComoApplicationJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new BadHttpRequestException("Invalid headers");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new HttpProtocolExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
    }

    [Fact(DisplayName = "HttpProtocolExceptionMiddleware - BadHttpRequestException should call notify.Add")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_BadHttpRequestException_DeveChamarNotifyAdd()
    {
        // Arrange
        RequestDelegate next = _ => throw new BadHttpRequestException("Protocol error");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new HttpProtocolExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        notifyMock.Verify(n => n.Add("Invalid request format", 400), Times.Once);
    }

    [Fact(DisplayName = "HttpProtocolExceptionMiddleware - BadHttpRequestException response body should contain Error ID")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_BadHttpRequestException_RespostaDeveConterErrorId()
    {
        // Arrange
        RequestDelegate next = _ => throw new BadHttpRequestException("Bad request");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new HttpProtocolExceptionMiddleware(next, loggerMock.Object);
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

    [Fact(DisplayName = "HttpProtocolExceptionMiddleware - BadHttpRequestException response body should contain friendly message (NOT technical exception message)")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_BadHttpRequestException_RespostaDeveConterMensagemAmigavel()
    {
        // Arrange
        const string errorMessage = "Custom protocol error message";
        RequestDelegate next = _ => throw new BadHttpRequestException(errorMessage);
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new HttpProtocolExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        // Deve conter mensagem amigável, NÃO a mensagem técnica da exceção
        Assert.Contains("invalid format or content", body);
        Assert.DoesNotContain(errorMessage, body); // Mensagem técnica NÃO deve aparecer (segurança)
    }

    [Fact(DisplayName = "HttpProtocolExceptionMiddleware - Response already started should not modify response")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_BadHttpRequestExceptionComRespostaJaIniciada_NaoDeveModificarResposta()
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
        var middleware = new HttpProtocolExceptionMiddleware(_ => throw new BadHttpRequestException("Error"), loggerMock.Object);

        // Act
        await middleware.InvokeAsync(httpContextMock.Object, notifyMock.Object);

        // Assert
        httpResponseMock.VerifySet(r => r.StatusCode = It.IsAny<int>(), Times.Never);
    }

    [Fact(DisplayName = "HttpProtocolExceptionMiddleware - BadHttpRequestException response title should be BadRequest")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_BadHttpRequestException_RespostaDeveTerTituloBadRequest()
    {
        // Arrange
        RequestDelegate next = _ => throw new BadHttpRequestException("Protocol violation");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new HttpProtocolExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("BadRequest", body);
    }

    [Fact(DisplayName = "HttpProtocolExceptionMiddleware - No exception should invoke next middleware")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_SemExcecao_DeveInvocarProximoMiddleware()
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
        var middleware = new HttpProtocolExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.True(nextCalled);
    }
}
