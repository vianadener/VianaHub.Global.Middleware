using System.Net;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.Notifications;

public class NotifyExtensionsTests
{
    // Private DTO used to produce a concrete, named generic type T
    // so that Assert.IsType<JsonHttpResult<TestDataDto>> resolves correctly.
    private record TestDataDto(int Id, string Name);

    #region Success - CustomResponse()

    [Fact(DisplayName = "CustomResponse without data should return Ok when there are no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithNoNotifications_ShouldReturnOkResult()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(false);

        // Act
        var result = mockNotify.Object.CustomResponse();

        // Assert
        Assert.IsType<Ok>(result);
    }

    [Fact(DisplayName = "CustomResponse without data should return Json error when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithNotifications_ShouldReturnJsonResult()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Validation error" });

        // Act
        var result = mockNotify.Object.CustomResponse();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
    }

    [Fact(DisplayName = "CustomResponse without data should parse field:message format correctly")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithFieldColonMessageFormat_ShouldParseCorrectly()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Name: Name is required" });

        // Act
        var result = mockNotify.Object.CustomResponse();

        // Assert
        Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
        var jsonResult = (JsonHttpResult<ErrorResponse>)result;
        Assert.True(jsonResult.Value!.Errors.ContainsKey("Name"));
    }

    #endregion

    #region Success - CustomResponse(int statusCode)

    [Fact(DisplayName = "CustomResponse with statusCode should return NoContent when statusCode is 204 and no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCode204AndNoNotifications_ShouldReturnNoContent()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(false);

        // Act
        var result = mockNotify.Object.CustomResponse(204);

        // Assert
        Assert.IsType<NoContent>(result);
    }

    [Fact(DisplayName = "CustomResponse with statusCode should return StatusCode result when no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeAndNoNotifications_ShouldReturnStatusCodeResult()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(false);

        // Act
        var result = mockNotify.Object.CustomResponse(201);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<StatusCodeHttpResult>(result);
    }

    [Fact(DisplayName = "CustomResponse with statusCode should return Json error when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeAndNotifications_ShouldReturnJsonResult()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Bad request" });

        // Act
        var result = mockNotify.Object.CustomResponse(400);

        // Assert
        Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
    }

    [Fact(DisplayName = "CustomResponse with statusCode should parse field:message format correctly")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithStatusCodeAndFieldColonMessage_ShouldParseCorrectly()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Email: Email is invalid" });

        // Act
        var result = mockNotify.Object.CustomResponse(400);

        // Assert
        var jsonResult = (JsonHttpResult<ErrorResponse>)result;
        Assert.True(jsonResult.Value!.Errors.ContainsKey("Email"));
    }

    #endregion

    #region Success - CustomResponse<T>(T data, int statusCode)

    [Fact(DisplayName = "CustomResponse with data should return Json data when no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithData_WithNoNotifications_ShouldReturnJsonData()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(false);
        var data = new TestDataDto(1, "Test");

        // Act
        var result = mockNotify.Object.CustomResponse(data);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonHttpResult<TestDataDto>>(result);
    }

    [Fact(DisplayName = "CustomResponse with data should return Json error when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithData_WithNotifications_ShouldReturnJsonError()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Validation failed" });
        var data = new { Id = 1 };

        // Act
        var result = mockNotify.Object.CustomResponse(data);

        // Assert
        Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
    }

    [Fact(DisplayName = "CustomResponse with data should return NoContent when statusCode is 204 and no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithData_WithStatusCode204AndNoNotifications_ShouldReturnNoContent()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(false);
        var data = new { Id = 1 };

        // Act
        var result = mockNotify.Object.CustomResponse(data, 204);

        // Assert
        Assert.IsType<NoContent>(result);
    }

    [Fact(DisplayName = "CustomResponse with data should parse field:message format correctly")]
    [Trait("Notifications", "")]
    public void CustomResponseWithData_WithFieldColonMessage_ShouldParseCorrectly()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.NotFound);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Resource: Not found" });

        // Act
        var result = mockNotify.Object.CustomResponse(new { });

        // Assert
        var jsonResult = (JsonHttpResult<ErrorResponse>)result;
        Assert.True(jsonResult.Value!.Errors.ContainsKey("Resource"));
    }

    #endregion

    #region Success - CustomResponse(byte[] data, int statusCode)

    [Fact(DisplayName = "CustomResponse with byte array should return File result when no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithByteArray_WithNoNotifications_ShouldReturnFileResult()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(false);
        var pdfData = new byte[] { 1, 2, 3, 4 };

        // Act
        var result = mockNotify.Object.CustomResponse(pdfData);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileContentHttpResult>(result);
    }

    [Fact(DisplayName = "CustomResponse with byte array should return Json error when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithByteArray_WithNotifications_ShouldReturnJsonError()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "PDF error" });
        var pdfData = new byte[] { 1, 2, 3 };

        // Act
        var result = mockNotify.Object.CustomResponse(pdfData);

        // Assert
        Assert.IsType<JsonHttpResult<ErrorResponse>>(result);
    }

    [Fact(DisplayName = "CustomResponse with byte array should return NoContent when statusCode is 204 and no notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithByteArray_WithStatusCode204AndNoNotifications_ShouldReturnNoContent()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(false);

        // Act
        var result = mockNotify.Object.CustomResponse(new byte[] { 1 }, 204);

        // Assert
        Assert.IsType<NoContent>(result);
    }

    #endregion

    #region Success - AddFieldError

    [Fact(DisplayName = "AddFieldError should call Add with field:message format and status code")]
    [Trait("Notifications", "")]
    public void AddFieldError_WithFieldAndMessage_ShouldCallAddWithCorrectFormat()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        const string field = "Username";
        const string message = "Username is required";
        const int statusCode = 400;

        // Act
        mockNotify.Object.AddFieldError(field, message, statusCode);

        // Assert
        mockNotify.Verify(n => n.Add($"{field}: {message}", statusCode), Times.Once);
    }

    [Fact(DisplayName = "AddFieldError should use default status code 400 when not provided")]
    [Trait("Notifications", "")]
    public void AddFieldError_WithoutStatusCode_ShouldUseDefaultStatusCode400()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        const string field = "Email";
        const string message = "Email is invalid";

        // Act
        mockNotify.Object.AddFieldError(field, message);

        // Assert
        mockNotify.Verify(n => n.Add($"{field}: {message}", 400), Times.Once);
    }

    [Fact(DisplayName = "AddFieldError with custom status code should pass correct status code to Add")]
    [Trait("Notifications", "")]
    public void AddFieldError_WithCustomStatusCode_ShouldPassCorrectStatusCode()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        const string field = "Document";
        const string message = "Document not found";
        const int statusCode = 404;

        // Act
        mockNotify.Object.AddFieldError(field, message, statusCode);

        // Assert
        mockNotify.Verify(n => n.Add($"{field}: {message}", statusCode), Times.Once);
    }

    #endregion

    #region Failure

    [Fact(DisplayName = "CustomResponse without data should not return Ok when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithNotifications_ShouldNotReturnOkResult()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Error" });

        // Act
        var result = mockNotify.Object.CustomResponse();

        // Assert
        Assert.IsNotType<Ok>(result);
    }

    [Fact(DisplayName = "CustomResponse with data should not return data Json when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithData_WithNotifications_ShouldNotReturnDataJson()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Error" });
        var data = new { Id = 99 };

        // Act
        var result = mockNotify.Object.CustomResponse(data);

        // Assert
        Assert.IsNotType<JsonHttpResult<object>>(result);
    }

    [Fact(DisplayName = "CustomResponse with byte array should not return File when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponseWithByteArray_WithNotifications_ShouldNotReturnFileResult()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.InternalServerError);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Processing error" });
        var pdfData = new byte[] { 1, 2, 3 };

        // Act
        var result = mockNotify.Object.CustomResponse(pdfData);

        // Assert
        Assert.IsNotType<FileContentHttpResult>(result);
    }

    [Fact(DisplayName = "CustomResponse with statusCode should not return NoContent when there are notifications")]
    [Trait("Notifications", "")]
    public void CustomResponse_WithNotificationsAndStatusCode204_ShouldNotReturnNoContent()
    {
        // Arrange
        var mockNotify = new Mock<INotify>();
        mockNotify.Setup(n => n.HasNotify()).Returns(true);
        mockNotify.Setup(n => n.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        mockNotify.Setup(n => n.GetErrorMessage()).Returns(new List<string> { "Error" });

        // Act
        var result = mockNotify.Object.CustomResponse(204);

        // Assert
        Assert.IsNotType<NoContent>(result);
    }

    #endregion
}
