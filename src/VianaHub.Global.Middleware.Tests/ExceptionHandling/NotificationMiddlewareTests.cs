// <copyright file="NotificationMiddlewareTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>

using EBL.FIG.Common.Middleware.Lib.Middleware;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling;

public class NotificationMiddlewareTests
{
    private static Mock<INotify> CreateNotifyMock(
        bool hasNotify = false,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        List<string>? messages = null)
    {
        var mock = new Mock<INotify>();
        mock.Setup(n => n.HasNotify()).Returns(hasNotify);
        mock.Setup(n => n.GetStatusCode()).Returns(statusCode);
        mock.Setup(n => n.GetErrorMessage()).Returns(messages ?? new List<string>());
        return mock;
    }

    private static Mock<ILogger<NotificationMiddleware>> CreateLoggerMock()
    {
        return new Mock<ILogger<NotificationMiddleware>>();
    }

    [Fact(DisplayName = "NotificationMiddleware - No notifications should invoke next middleware and return 200")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_SemNotificacoes_DeveInvocarProximoMiddlewareERetornar200()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var notifyMock = CreateNotifyMock(hasNotify: false);
        var loggerMock = CreateLoggerMock();
        var middleware = new NotificationMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact(DisplayName = "NotificationMiddleware - Notification with 400 and simple message should write error response with status 400")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_NotificacaoComStatus400MensagemSimples_DeveEscreverRespostaDeErroComStatus400()
    {
        // Arrange
        var messages = new List<string> { "Field validation failed" };
        var notifyMock = CreateNotifyMock(hasNotify: true, statusCode: HttpStatusCode.BadRequest, messages: messages);
        var loggerMock = CreateLoggerMock();
        var middleware = new NotificationMiddleware(_ => Task.CompletedTask, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
    }

    [Fact(DisplayName = "NotificationMiddleware - Notification with message containing colon should split into field and payload")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_NotificacaoComMensagemContendoDoisPontos_DeveDividirEmFieldEPayload()
    {
        // Arrange
        var messages = new List<string> { "Name: Field is required" };
        var notifyMock = CreateNotifyMock(hasNotify: true, statusCode: HttpStatusCode.BadRequest, messages: messages);
        var loggerMock = CreateLoggerMock();
        var middleware = new NotificationMiddleware(_ => Task.CompletedTask, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("Name", body);
        Assert.Contains("Field is required", body);
    }

    [Fact(DisplayName = "NotificationMiddleware - Notification with multiple messages should include all in response")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_NotificacaoComMultiplasMensagens_DeveIncluirTodasNaResposta()
    {
        // Arrange
        var messages = new List<string> { "Error one", "Field: Error two" };
        var notifyMock = CreateNotifyMock(hasNotify: true, statusCode: HttpStatusCode.BadRequest, messages: messages);
        var loggerMock = CreateLoggerMock();
        var middleware = new NotificationMiddleware(_ => Task.CompletedTask, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("Error one", body);
        Assert.Contains("Field", body);
        Assert.Contains("Error two", body);
    }

    [Fact(DisplayName = "NotificationMiddleware - Response already started should not modify response")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_RespostaJaIniciada_NaoDeveModificarResposta()
    {
        // Arrange
        var messages = new List<string> { "Some error" };
        var notifyMock = CreateNotifyMock(hasNotify: true, statusCode: HttpStatusCode.BadRequest, messages: messages);

        var httpResponseMock = new Mock<HttpResponse>();
        httpResponseMock.SetupGet(r => r.HasStarted).Returns(true);
        httpResponseMock.SetupGet(r => r.StatusCode).Returns(200);
        httpResponseMock.SetupSet(r => r.StatusCode = It.IsAny<int>());
        httpResponseMock.SetupSet(r => r.ContentType = It.IsAny<string>());

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(c => c.Response).Returns(httpResponseMock.Object);

        var loggerMock = CreateLoggerMock();
        var middleware = new NotificationMiddleware(_ => Task.CompletedTask, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(httpContextMock.Object, notifyMock.Object);

        // Assert
        httpResponseMock.VerifySet(r => r.StatusCode = It.IsAny<int>(), Times.Never);
    }

    [Fact(DisplayName = "NotificationMiddleware - Notification with 404 status should return 404 in response")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_NotificacaoComStatus404_DeveRetornar404NaResposta()
    {
        // Arrange
        var messages = new List<string> { "Resource not found" };
        var notifyMock = CreateNotifyMock(hasNotify: true, statusCode: HttpStatusCode.NotFound, messages: messages);
        var loggerMock = CreateLoggerMock();
        var middleware = new NotificationMiddleware(_ => Task.CompletedTask, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact(DisplayName = "NotificationMiddleware - Notification with 409 status should return 409 in response")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_NotificacaoComStatus409_DeveRetornar409NaResposta()
    {
        // Arrange
        var messages = new List<string> { "Conflict detected" };
        var notifyMock = CreateNotifyMock(hasNotify: true, statusCode: HttpStatusCode.Conflict, messages: messages);
        var loggerMock = CreateLoggerMock();
        var middleware = new NotificationMiddleware(_ => Task.CompletedTask, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }
}

