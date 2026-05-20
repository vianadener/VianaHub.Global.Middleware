// <copyright file="NotifyExtensionsCustomResponseStatusCodeTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using System.Net;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.Notifications;

/// <summary>
/// Tests that target the uncovered branch in CustomResponse(int statusCode):
///
///   statusCode = statusCode >= (int)notify.GetStatusCode() ? statusCode : 200;
///
/// The branch "statusCode = 200" is reached when the caller passes a statusCode
/// that is LESS than the statusCode stored in the notify instance.
/// </summary>
public class NotifyExtensionsCustomResponseStatusCodeTests
{
    // ?????????????????????????????????????????????????????????????????????????
    // Helpers
    // ?????????????????????????????????????????????????????????????????????????

    private static Mock<INotify> CreateNotifyWithStatus(
        HttpStatusCode notifyStatus,
        IEnumerable<string>? messages = null)
    {
        var mock = new Mock<INotify>();
        mock.Setup(n => n.HasNotify()).Returns(true);
        mock.Setup(n => n.GetStatusCode()).Returns(notifyStatus);
        mock.Setup(n => n.GetErrorMessage())
            .Returns(messages?.ToList() ?? new List<string> { "error" });
        return mock;
    }

    // ?????????????????????????????????????????????????????????????????????????
    // Success – statusCode < notify.GetStatusCode() ? effective statusCode becomes 200
    // ?????????????????????????????????????????????????????????????????????????

    [Fact(DisplayName = "CustomResponse with statusCode lower than notify status should use 200 as effective status")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeLowerThanNotifyStatus_ShouldUse200AsEffectiveStatus()
    {
        // Arrange – caller passes 200 but notify has 400; 200 < 400, so effective becomes 200
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(200);

        // Assert – result must be a JsonHttpResult with status 200
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal(200, jsonResult.StatusCode);
    }

    [Fact(DisplayName = "CustomResponse with statusCode lower than notify status should return JsonHttpResult")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeLowerThanNotifyStatus_ShouldReturnJsonHttpResult()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.InternalServerError);

        // Act – 201 < 500
        var result = notifyMock.Object.CustomResponse(201);

        // Assert
        Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
    }

    [Fact(DisplayName = "CustomResponse with statusCode equal to notify status should use that status code")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeEqualToNotifyStatus_ShouldUseThatStatusCode()
    {
        // Arrange – caller passes 400 and notify also has 400; 400 >= 400, so stays 400
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(400);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal(400, jsonResult.StatusCode);
    }

    [Fact(DisplayName = "CustomResponse with statusCode greater than notify status should use caller's status code")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeGreaterThanNotifyStatus_ShouldUseCallerStatusCode()
    {
        // Arrange – caller passes 500, notify has 400; 500 >= 400, so stays 500
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(500);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal(500, jsonResult.StatusCode);
    }

    [Fact(DisplayName = "CustomResponse with statusCode lower than notify status should include error messages in response body")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeLowerThanNotifyStatus_ShouldIncludeErrorMessages()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(
            HttpStatusCode.BadRequest,
            new[] { "Validation failed" });

        // Act
        var result = notifyMock.Object.CustomResponse(200);

        // Assert – at least one error was stored
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.NotNull(jsonResult.Value);
        Assert.NotEmpty(jsonResult.Value!.Errors);
    }

    [Fact(DisplayName = "CustomResponse with statusCode lower than notify status should include field colon message formatted errors")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeLowerThanNotifyStatus_ShouldIncludeFieldColonMessageErrors()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(
            HttpStatusCode.BadRequest,
            new[] { "Name: Name is required" });

        // Act
        var result = notifyMock.Object.CustomResponse(200);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.True(jsonResult.Value!.Errors.ContainsKey("Name"));
    }

    [Fact(DisplayName = "CustomResponse with statusCode lower than notify status should not return NoContent")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeLowerThanNotifyStatus_ShouldNotReturnNoContent()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(200);

        // Assert
        Assert.IsNotType<NoContent>(result);
    }

    [Fact(DisplayName = "CustomResponse with statusCode 201 lower than notify 500 should produce 200 effective status")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status201LowerThanNotify500_ShouldProduce200EffectiveStatus()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(
            HttpStatusCode.InternalServerError,
            new[] { "Internal failure" });

        // Act – 201 < 500, so effective becomes 200
        var result = notifyMock.Object.CustomResponse(201);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal(200, jsonResult.StatusCode);
    }

    // ?????????????????????????????????????????????????????????????????????????
    // GetErrorTitle branches via CustomResponse(int statusCode) when
    // statusCode >= notify.GetStatusCode()
    // ?????????????????????????????????????????????????????????????????????????

    [Fact(DisplayName = "CustomResponse with statusCode 401 greater than notify 400 should produce title Unauthorized")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status401GreaterThanNotify400_ShouldProduceTitleUnauthorized()
    {
        // Arrange – 401 >= 400 ? stays 401
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(401);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("Unauthorized", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with statusCode 403 greater than notify 400 should produce title Forbidden")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status403GreaterThanNotify400_ShouldProduceTitleForbidden()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(403);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("Forbidden", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with statusCode 404 greater than notify 400 should produce title NotFound")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status404GreaterThanNotify400_ShouldProduceTitleNotFound()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(404);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("NotFound", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with statusCode 409 greater than notify 400 should produce title Conflict")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status409GreaterThanNotify400_ShouldProduceTitleConflict()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(409);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("Conflict", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with statusCode 410 greater than notify 400 should produce title Gone")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status410GreaterThanNotify400_ShouldProduceTitleGone()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(410);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("Gone", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with statusCode 422 greater than notify 400 should produce title UnprocessableEntity")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status422GreaterThanNotify400_ShouldProduceTitleUnprocessableEntity()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(422);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("UnprocessableEntity", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with statusCode 429 greater than notify 400 should produce title TooManyRequests")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status429GreaterThanNotify400_ShouldProduceTitleTooManyRequests()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(429);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("TooManyRequests", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with statusCode 500 greater than notify 400 should produce title InternalServerError")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status500GreaterThanNotify400_ShouldProduceTitleInternalServerError()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(500);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("InternalServerError", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with statusCode 503 greater than notify 400 should produce title ServiceUnavailable")]
    [Trait("Notifications", "")]
    public void CustomResponse_Status503GreaterThanNotify400_ShouldProduceTitleServiceUnavailable()
    {
        // Arrange
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(503);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("ServiceUnavailable", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with unmapped statusCode greater than notify 400 should fall back to title BadRequest")]
    [Trait("Notifications", "")]
    public void CustomResponse_UnmappedStatusCodeGreaterThanNotify400_ShouldFallBackToBadRequestTitle()
    {
        // Arrange – 418 is not in the GetErrorTitle switch
        var notifyMock = CreateNotifyWithStatus(HttpStatusCode.BadRequest);

        // Act
        var result = notifyMock.Object.CustomResponse(418);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("BadRequest", jsonResult.Value!.Title);
    }
}
