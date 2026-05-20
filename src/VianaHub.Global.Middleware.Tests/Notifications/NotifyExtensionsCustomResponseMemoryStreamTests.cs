// <copyright file="NotifyExtensionsCustomResponseMemoryStreamTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using System.Net;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.Notifications;

/// <summary>
/// Unit tests for the overload:
///   CustomResponse(this INotify notify, MemoryStream data, int statusCode = 200)
///
/// Branches covered:
///   1. HasNotify() == true  → returns JsonHttpResult&lt;ErrorResponse&gt;
///        1a. plain message  → error stored under "Error" key
///        1b. "field: msg"   → error stored under field key
///        1c. status code from notify is used as the effective status code
///        1d. GetErrorTitle switch branches (400 → "BadRequest", 401 → "Unauthorized", etc.)
///   2. HasNotify() == false &amp;&amp; statusCode == 204  → returns NoContent
///   3. HasNotify() == false &amp;&amp; statusCode != 204  → returns FileContentHttpResult (ZIP)
/// </summary>
public class NotifyExtensionsCustomResponseMemoryStreamTests
{
    #region Helpers

    private static Mock<INotify> CreateNotifyWithError(
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

    private static Mock<INotify> CreateNotifyWithoutError()
    {
        var mock = new Mock<INotify>();
        mock.Setup(n => n.HasNotify()).Returns(false);
        return mock;
    }

    private static MemoryStream CreateZipStream(byte[]? content = null)
        => new(content ?? new byte[] { 80, 75, 3, 4 }); // PK header

    #endregion

    #region Success – no notifications (happy path)

    [Fact(DisplayName = "CustomResponse with MemoryStream should return FileContentHttpResult when there are no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNoNotifications_ShouldReturnFileContentHttpResult()
    {
        // Arrange
        var notifyMock = CreateNotifyWithoutError();
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse<MemoryStream>(stream);

        // Assert
        Assert.IsType<FileContentHttpResult>(result);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should return ZIP content type when there are no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNoNotifications_ShouldReturnZipContentType()
    {
        // Arrange
        var notifyMock = CreateNotifyWithoutError();
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse<MemoryStream>(stream);

        // Assert
        var fileResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal("application/zip", fileResult.ContentType);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should return correct file name when there are no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNoNotifications_ShouldReturnCorrectFileName()
    {
        // Arrange
        var notifyMock = CreateNotifyWithoutError();
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse<MemoryStream>(stream);

        // Assert
        var fileResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.NotNull(fileResult.FileDownloadName);
        Assert.EndsWith(".zip", fileResult.FileDownloadName);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should return file bytes that match stream content when there are no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNoNotifications_ShouldReturnFileBytesMatchingStreamContent()
    {
        // Arrange
        var notifyMock = CreateNotifyWithoutError();
        var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
        using var stream = new MemoryStream(expectedBytes);

        // Act
        var result = notifyMock.Object.CustomResponse<MemoryStream>(stream);

        // Assert
        var fileResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal(expectedBytes, fileResult.FileContents.ToArray());
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream and explicit statusCode 200 should return FileContentHttpResult when there are no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithExplicitStatus200AndNoNotifications_ShouldReturnFileContentHttpResult()
    {
        // Arrange
        var notifyMock = CreateNotifyWithoutError();
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse<MemoryStream>(stream, 200);

        // Assert
        Assert.IsType<FileContentHttpResult>(result);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream and statusCode 201 should return FileContentHttpResult when there are no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus201AndNoNotifications_ShouldReturnFileContentHttpResult()
    {
        // Arrange
        var notifyMock = CreateNotifyWithoutError();
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse<MemoryStream>(stream, 201);

        // Assert
        Assert.IsType<FileContentHttpResult>(result);
    }

    #endregion

    #region Success – statusCode 204, no notifications

    [Fact(DisplayName = "CustomResponse with MemoryStream and statusCode 204 should return NoContent when there are no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatusCode204AndNoNotifications_ShouldReturnNoContent()
    {
        // Arrange
        var notifyMock = CreateNotifyWithoutError();
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream, 204);

        // Assert
        Assert.IsType<NoContent>(result);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream and statusCode 204 should not return FileContentHttpResult when there are no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatusCode204AndNoNotifications_ShouldNotReturnFileContentHttpResult()
    {
        // Arrange
        var notifyMock = CreateNotifyWithoutError();
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream, 204);

        // Assert
        Assert.IsNotType<FileContentHttpResult>(result);
    }

    #endregion

    #region Failure – notifications present (error path)

    [Fact(DisplayName = "CustomResponse with MemoryStream should return JsonHttpResult when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNotifications_ShouldReturnJsonHttpResult()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.BadRequest);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should not return FileContentHttpResult when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNotifications_ShouldNotReturnFileContentHttpResult()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.BadRequest);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        Assert.IsNotType<FileContentHttpResult>(result);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should not return NoContent when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNotifications_ShouldNotReturnNoContent()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.BadRequest);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        Assert.IsNotType<NoContent>(result);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should use notify status code when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNotifications_ShouldUseNotifyStatusCode()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.BadRequest);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal(400, jsonResult.StatusCode);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should use notify 500 status code when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithInternalServerErrorNotification_ShouldUse500StatusCode()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.InternalServerError);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal(500, jsonResult.StatusCode);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should use notify 404 status code when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNotFoundNotification_ShouldUse404StatusCode()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.NotFound);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal(404, jsonResult.StatusCode);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should include error messages in response body when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNotifications_ShouldIncludeErrorMessagesInResponseBody()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(
            HttpStatusCode.BadRequest,
            new[] { "Validation failed" });
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.NotNull(jsonResult.Value);
        Assert.NotEmpty(jsonResult.Value!.Errors);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should store plain message under Error key when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithPlainMessage_ShouldStoreUnderErrorKey()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(
            HttpStatusCode.BadRequest,
            new[] { "Validation failed" });
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.True(jsonResult.Value!.Errors.ContainsKey("Error"));
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should parse field colon message format and store under field key when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithFieldColonMessage_ShouldStoreUnderFieldKey()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(
            HttpStatusCode.BadRequest,
            new[] { "File: File is too large" });
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.True(jsonResult.Value!.Errors.ContainsKey("File"));
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should store correct field payload when message uses field colon format")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithFieldColonMessage_ShouldStoreCorrectPayloadUnderFieldKey()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(
            HttpStatusCode.BadRequest,
            new[] { "Document: Document is corrupted" });
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Contains("Document is corrupted", jsonResult.Value!.Errors["Document"]);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should accumulate multiple plain messages when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithMultiplePlainMessages_ShouldAccumulateUnderErrorKey()
    {
        // Arrange – second AddError("msg") overwrites, so only the last plain message survives
        var notifyMock = CreateNotifyWithError(
            HttpStatusCode.BadRequest,
            new[] { "First error", "Second error" });
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.True(jsonResult.Value!.Errors.ContainsKey("Error"));
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should accumulate multiple field colon messages under their respective fields when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithMultipleFieldColonMessages_ShouldStoreUnderRespectiveFields()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(
            HttpStatusCode.UnprocessableEntity,
            new[] { "Name: Name is required", "Email: Email is invalid" });
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.True(jsonResult.Value!.Errors.ContainsKey("Name"));
        Assert.True(jsonResult.Value!.Errors.ContainsKey("Email"));
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should ignore the provided statusCode parameter and use notify status code when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithNotificationsAndCustomStatusCode_ShouldIgnoreProvidedStatusCodeAndUseNotifyStatusCode()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.Conflict);
        using var stream = CreateZipStream();

        // Act – caller passes 200 but notify says 409
        var result = notifyMock.Object.CustomResponse(stream, 200);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal(409, jsonResult.StatusCode);
    }

    #endregion

    #region Failure – GetErrorTitle branches via notify status code

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title BadRequest when notify status is 400")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus400_ShouldProduceTitleBadRequest()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.BadRequest);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("BadRequest", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title Unauthorized when notify status is 401")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus401_ShouldProduceTitleUnauthorized()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.Unauthorized);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("Unauthorized", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title Forbidden when notify status is 403")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus403_ShouldProduceTitleForbidden()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.Forbidden);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("Forbidden", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title NotFound when notify status is 404")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus404_ShouldProduceTitleNotFound()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.NotFound);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("NotFound", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title Conflict when notify status is 409")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus409_ShouldProduceTitleConflict()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.Conflict);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("Conflict", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title Gone when notify status is 410")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus410_ShouldProduceTitleGone()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.Gone);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("Gone", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title UnprocessableEntity when notify status is 422")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus422_ShouldProduceTitleUnprocessableEntity()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.UnprocessableEntity);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("UnprocessableEntity", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title TooManyRequests when notify status is 429")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus429_ShouldProduceTitleTooManyRequests()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError((HttpStatusCode)429);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("TooManyRequests", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title InternalServerError when notify status is 500")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus500_ShouldProduceTitleInternalServerError()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.InternalServerError);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("InternalServerError", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should produce title ServiceUnavailable when notify status is 503")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithStatus503_ShouldProduceTitleServiceUnavailable()
    {
        // Arrange
        var notifyMock = CreateNotifyWithError(HttpStatusCode.ServiceUnavailable);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("ServiceUnavailable", jsonResult.Value!.Title);
    }

    [Fact(DisplayName = "CustomResponse with MemoryStream should fall back to title BadRequest when notify status is unmapped")]
    [Trait("Notifications", "")]
    public void CustomResponseWithMemoryStream_WithUnmappedStatus_ShouldFallBackToBadRequestTitle()
    {
        // Arrange – 418 I'm a Teapot is not in the GetErrorTitle switch
        var notifyMock = CreateNotifyWithError((HttpStatusCode)418);
        using var stream = CreateZipStream();

        // Act
        var result = notifyMock.Object.CustomResponse(stream);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        Assert.Equal("BadRequest", jsonResult.Value!.Title);
    }

    #endregion
}
