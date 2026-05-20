// <copyright file="GetFriendlyJsonErrorKeyTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using EBL.FIG.Common.Middleware.Lib.Middleware;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling;

/// <summary>
/// Tests that target the GetFriendlyJsonErrorKey private method inside JsonExceptionMiddleware.
/// Because the method is private, every scenario is driven through the public InvokeAsync entry
/// point by throwing a JsonException whose Message contains the relevant keyword.
/// </summary>
public class GetFriendlyJsonErrorKeyTests
{
    // ?????????????????????????????????????????????????????????????????????????
    // Helpers
    // ?????????????????????????????????????????????????????????????????????????

    private static Mock<INotify> CreateNotifyMock()
    {
        var mock = new Mock<INotify>();
        mock.Setup(n => n.HasNotify()).Returns(false);
        mock.Setup(n => n.GetStatusCode()).Returns(System.Net.HttpStatusCode.OK);
        mock.Setup(n => n.GetErrorMessage()).Returns(new List<string>());
        return mock;
    }

    private static JsonExceptionMiddleware CreateMiddleware(RequestDelegate next)
    {
        var loggerMock = new Mock<ILogger<JsonExceptionMiddleware>>();
        return new JsonExceptionMiddleware(next, loggerMock.Object);
    }

    private static async Task<(int statusCode, string body, Mock<INotify> notifyMock)>
        InvokeWithJsonExceptionMessageAsync(string jsonExceptionMessage)
    {
        var notifyMock = CreateNotifyMock();
        var middleware = CreateMiddleware(_ => throw new JsonException(jsonExceptionMessage));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, notifyMock.Object);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context.Response.StatusCode, body, notifyMock);
    }

    // ?????????????????????????????????????????????????????????????????????????
    // Success – each branch of GetFriendlyJsonErrorKey
    // ?????????????????????????????????????????????????????????????????????????

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'trailing comma' should call notify.Add with trailing-comma key")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemTrailingComma_DeveChamarNotifyAddComChaveCorreta()
    {
        // Arrange / Act
        var (_, _, notifyMock) = await InvokeWithJsonExceptionMessageAsync("JSON has a trailing comma near line 1");

        // Assert – the friendly key must contain information about the trailing comma
        notifyMock.Verify(
            n => n.Add(It.Is<string>(s => s.Contains("trailing comma", StringComparison.OrdinalIgnoreCase)), It.IsAny<int>()),
            Times.Once);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'trailing comma' should return 400 status code")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemTrailingComma_DeveRetornar400()
    {
        // Act
        var (statusCode, _, _) = await InvokeWithJsonExceptionMessageAsync("trailing comma found");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'unexpected character' should call notify.Add with invalid-character key")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemUnexpectedCharacter_DeveChamarNotifyAddComChaveCorreta()
    {
        // Arrange / Act
        var (_, _, notifyMock) = await InvokeWithJsonExceptionMessageAsync("unexpected character '&' at position 5");

        // Assert
        notifyMock.Verify(
            n => n.Add(It.Is<string>(s => s.Contains("invalid character", StringComparison.OrdinalIgnoreCase)), It.IsAny<int>()),
            Times.Once);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'invalid character' should call notify.Add with invalid-character key")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemInvalidCharacter_DeveChamarNotifyAddComChaveCorreta()
    {
        // Arrange / Act
        var (_, _, notifyMock) = await InvokeWithJsonExceptionMessageAsync("invalid character detected in the input");

        // Assert
        notifyMock.Verify(
            n => n.Add(It.Is<string>(s => s.Contains("invalid character", StringComparison.OrdinalIgnoreCase)), It.IsAny<int>()),
            Times.Once);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'unterminated string' should call notify.Add with unterminated-string key")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemUnterminatedString_DeveChamarNotifyAddComChaveCorreta()
    {
        // Arrange / Act
        var (_, _, notifyMock) = await InvokeWithJsonExceptionMessageAsync("unterminated string literal found");

        // Assert
        notifyMock.Verify(
            n => n.Add(It.Is<string>(s => s.Contains("unterminated string", StringComparison.OrdinalIgnoreCase)), It.IsAny<int>()),
            Times.Once);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'unterminated string' should return 400")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemUnterminatedString_DeveRetornar400()
    {
        // Act
        var (statusCode, _, _) = await InvokeWithJsonExceptionMessageAsync("unterminated string in JSON body");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing both 'expected' and 'got' should call notify.Add with malformed-json key")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemExpectedGot_DeveChamarNotifyAddComChaveMalformed()
    {
        // Arrange / Act
        var (_, _, notifyMock) = await InvokeWithJsonExceptionMessageAsync("expected '{' but got ']'");

        // Assert
        notifyMock.Verify(
            n => n.Add(It.Is<string>(s => s.Contains("malformed", StringComparison.OrdinalIgnoreCase)), It.IsAny<int>()),
            Times.Once);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'depth' should call notify.Add with nesting-levels key")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemDepth_DeveChamarNotifyAddComChaveNestingLevels()
    {
        // Arrange / Act
        var (_, _, notifyMock) = await InvokeWithJsonExceptionMessageAsync("JSON depth exceeds maximum allowed depth of 64");

        // Assert
        notifyMock.Verify(
            n => n.Add(It.Is<string>(s => s.Contains("nesting levels", StringComparison.OrdinalIgnoreCase)), It.IsAny<int>()),
            Times.Once);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'depth' should return 400")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemDepth_DeveRetornar400()
    {
        // Act
        var (statusCode, _, _) = await InvokeWithJsonExceptionMessageAsync("max depth exceeded");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'property name' should call notify.Add with invalid-property-name key")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemPropertyName_DeveChamarNotifyAddComChavePropertyName()
    {
        // Arrange / Act
        var (_, _, notifyMock) = await InvokeWithJsonExceptionMessageAsync("property name must be a string");

        // Assert
        notifyMock.Verify(
            n => n.Add(It.Is<string>(s => s.Contains("property name", StringComparison.OrdinalIgnoreCase)), It.IsAny<int>()),
            Times.Once);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'property name' should return 400")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemPropertyName_DeveRetornar400()
    {
        // Act
        var (statusCode, _, _) = await InvokeWithJsonExceptionMessageAsync("The property name is invalid");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'missing' should call notify.Add with incomplete-json key")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemMissing_DeveChamarNotifyAddComChaveIncomplete()
    {
        // Arrange / Act
        var (_, _, notifyMock) = await InvokeWithJsonExceptionMessageAsync("missing closing bracket for array");

        // Assert
        notifyMock.Verify(
            n => n.Add(It.Is<string>(s => s.Contains("incomplete", StringComparison.OrdinalIgnoreCase)), It.IsAny<int>()),
            Times.Once);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - message containing 'missing' should return 400")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemMissing_DeveRetornar400()
    {
        // Act
        var (statusCode, _, _) = await InvokeWithJsonExceptionMessageAsync("missing end of object");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - generic/unrecognised message should call notify.Add with generic invalid-json key")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemGenerica_DeveChamarNotifyAddComChaveGenerica()
    {
        // Arrange / Act – message does not match any specific keyword
        var (_, _, notifyMock) = await InvokeWithJsonExceptionMessageAsync("some unknown json error");

        // Assert – fallback returns "Invalid JSON format: JSON is invalid"
        notifyMock.Verify(
            n => n.Add("Invalid JSON format: JSON is invalid", 400),
            Times.Once);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - generic message should return 400")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_MensagemGenerica_DeveRetornar400()
    {
        // Act
        var (statusCode, _, _) = await InvokeWithJsonExceptionMessageAsync("completely unknown json problem");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
    }

    [Fact(DisplayName = "GetFriendlyJsonErrorKey - response body should be valid JSON for all error keys")]
    [Trait("ExceptionHandling", "")]
    public async Task GetFriendlyJsonErrorKey_TodosOsBranches_RespostaDeveSerJsonValido()
    {
        // Arrange – exercise each branch and verify valid JSON is returned
        var messages = new[]
        {
            "trailing comma in JSON",
            "unexpected character found",
            "invalid character at start",
            "unterminated string token",
            "expected '}' got ']'",
            "depth limit reached",
            "property name invalid",
            "missing token",
            "totally generic error"
        };

        foreach (var msg in messages)
        {
            // Act
            var (_, body, _) = await InvokeWithJsonExceptionMessageAsync(msg);

            // Assert
            var parsed = JsonSerializer.Deserialize<JsonElement>(body);
            Assert.Equal(JsonValueKind.Object, parsed.ValueKind);
        }
    }
}
