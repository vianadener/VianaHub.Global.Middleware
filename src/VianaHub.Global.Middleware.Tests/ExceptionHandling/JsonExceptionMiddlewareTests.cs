// <copyright file="JsonExceptionMiddlewareTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>

using EBL.FIG.Common.Middleware.Lib.Middleware;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling;

public class JsonExceptionMiddlewareTests
{
    private static Mock<INotify> CreateNotifyMock()
    {
        var mock = new Mock<INotify>();
        mock.Setup(n => n.HasNotify()).Returns(false);
        mock.Setup(n => n.GetErrorMessage()).Returns(new List<string>());
        return mock;
    }

    private static Mock<ILogger<JsonExceptionMiddleware>> CreateLoggerMock()
    {
        return new Mock<ILogger<JsonExceptionMiddleware>>();
    }

    private static JsonException CreateJsonExceptionWithPosition(
        string message,
        long lineNumber,
        long bytePositionInLine) =>
            new(message, "$.field", lineNumber, bytePositionInLine, null);

    private static JsonException CreateJsonExceptionWithoutPosition(string message) =>
        new(message);

    [Fact(DisplayName = "JsonExceptionMiddleware - JsonException should return 400 status code")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonException_DeveRetornar400()
    {
        // Arrange
        RequestDelegate next = _ => throw new JsonException("Unexpected character");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact(DisplayName = "JsonExceptionMiddleware - JsonException should set content type to application/json")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonException_DeveDefinirContentTypeComoApplicationJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new JsonException("Invalid JSON");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
    }

    [Fact(DisplayName = "JsonExceptionMiddleware - JsonException should call notify.Add")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonException_DeveChamarNotifyAdd()
    {
        // Arrange
        RequestDelegate next = _ => throw new JsonException("Trailing comma");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        notifyMock.Verify(n => n.Add(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [Fact(DisplayName = "JsonExceptionMiddleware - JsonException response body should contain Error ID")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonException_RespostaDeveConterErrorId()
    {
        // Arrange
        RequestDelegate next = _ => throw new JsonException("Invalid character");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
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

    [Fact(DisplayName = "JsonExceptionMiddleware - BadHttpRequestException wrapping JsonException should return 400")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_BadHttpRequestExceptionWrapJsonException_DeveRetornar400()
    {
        // Arrange
        var jsonEx = new JsonException("Unexpected character");
        RequestDelegate next = _ => throw new BadHttpRequestException("Bad request", jsonEx);
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact(DisplayName = "JsonExceptionMiddleware - JsonException with LineNumber should include 'Line' entry in response body")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonExceptionComLineNumber_DeveIncluirLineNaResposta()
    {
        // Arrange
        var jsonEx = CreateJsonExceptionWithPosition("Unexpected character", lineNumber: 4, bytePositionInLine: 12);
        RequestDelegate next = _ => throw jsonEx;
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("Line", body);
        Assert.Contains("Line 4", body);
    }

    [Fact(DisplayName = "JsonExceptionMiddleware - JsonException with BytePositionInLine should include 'Position' entry in response body")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonExceptionComBytePosition_DeveIncluirPositionNaResposta()
    {
        // Arrange
        var jsonEx = CreateJsonExceptionWithPosition("Invalid character", lineNumber: 2, bytePositionInLine: 9);
        RequestDelegate next = _ => throw jsonEx;
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("Position", body);
        Assert.Contains("Position 9", body);
    }

    [Fact(DisplayName = "JsonExceptionMiddleware - JsonException without LineNumber should not include 'Line' entry in response body")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonExceptionSemLineNumber_NaoDeveIncluirLineNaResposta()
    {
        // Arrange
        var jsonEx = CreateJsonExceptionWithoutPosition("JSON is invalid");
        RequestDelegate next = _ => throw jsonEx;
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("\"Line\"", body);
    }

    [Fact(DisplayName = "JsonExceptionMiddleware - JsonException without BytePositionInLine should not include 'Position' entry in response body")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonExceptionSemBytePosition_NaoDeveIncluirPositionNaResposta()
    {
        // Arrange
        var jsonEx = CreateJsonExceptionWithoutPosition("JSON is incomplete");
        RequestDelegate next = _ => throw jsonEx;
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("\"Position\"", body);
    }

    [Fact(DisplayName = "JsonExceptionMiddleware - Response already started should not modify response")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonExceptionComRespostaJaIniciada_NaoDeveModificarResposta()
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
        var middleware = new JsonExceptionMiddleware(_ => throw new JsonException("Invalid JSON"), loggerMock.Object);

        // Act
        await middleware.InvokeAsync(httpContextMock.Object, notifyMock.Object);

        // Assert
        httpResponseMock.VerifySet(r => r.StatusCode = It.IsAny<int>(), Times.Never);
    }

    [Fact(DisplayName = "JsonExceptionMiddleware - JsonException response title should be Invalid JSON Format")]
    [Trait("Middleware", "")]
    public async Task InvokeAsync_JsonException_RespostaDeveTerTituloInvalidJsonFormat()
    {
        // Arrange
        RequestDelegate next = _ => throw new JsonException("Unexpected character");
        var notifyMock = CreateNotifyMock();
        var loggerMock = CreateLoggerMock();
        var middleware = new JsonExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, notifyMock.Object);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("Invalid JSON Format", body);
    }
}
