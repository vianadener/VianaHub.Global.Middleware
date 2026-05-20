using System.Net;
using EBL.FIG.Common.Middleware.Lib.Notifications;

namespace EBL.FIG.Common.Middleware.Tests.Notifications;

public class ViewResultTests
{
    #region Success

    [Fact(DisplayName = "Constructor should set Response correctly")]
    [Trait("Notifications", "")]
    public void Constructor_WithResponseAndStatusCode_ShouldSetResponseCorrectly()
    {
        // Arrange
        var responseData = new { Id = 1, Name = "Test" };
        const HttpStatusCode statusCode = HttpStatusCode.OK;

        // Act
        var viewResult = new ViewResult(responseData, statusCode);

        // Assert
        Assert.Equal(responseData, viewResult.Response);
    }

    [Fact(DisplayName = "Constructor should set StatusCode correctly")]
    [Trait("Notifications", "")]
    public void Constructor_WithResponseAndStatusCode_ShouldSetStatusCodeCorrectly()
    {
        // Arrange
        var responseData = new { Id = 1 };
        const HttpStatusCode statusCode = HttpStatusCode.Created;

        // Act
        var viewResult = new ViewResult(responseData, statusCode);

        // Assert
        Assert.Equal(statusCode, viewResult.StatusCode);
    }

    [Fact(DisplayName = "Constructor with OK status code should have HttpStatusCode.OK")]
    [Trait("Notifications", "")]
    public void Constructor_WithOKStatusCode_StatusCodeShouldBeOK()
    {
        // Arrange & Act
        var viewResult = new ViewResult("Success response", HttpStatusCode.OK);

        // Assert
        Assert.Equal(HttpStatusCode.OK, viewResult.StatusCode);
    }

    [Fact(DisplayName = "Constructor with NotFound status code should have HttpStatusCode.NotFound")]
    [Trait("Notifications", "")]
    public void Constructor_WithNotFoundStatusCode_StatusCodeShouldBeNotFound()
    {
        // Arrange & Act
        var viewResult = new ViewResult("Not found", HttpStatusCode.NotFound);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, viewResult.StatusCode);
    }

    [Fact(DisplayName = "Constructor with string response should set string as Response")]
    [Trait("Notifications", "")]
    public void Constructor_WithStringResponse_ShouldSetStringAsResponse()
    {
        // Arrange
        const string response = "Simple string response";

        // Act
        var viewResult = new ViewResult(response, HttpStatusCode.OK);

        // Assert
        Assert.Equal(response, viewResult.Response);
    }

    [Fact(DisplayName = "Constructor with null response should set Response to null")]
    [Trait("Notifications", "")]
    public void Constructor_WithNullResponse_ShouldSetResponseToNull()
    {
        // Arrange & Act
        var viewResult = new ViewResult(null!, HttpStatusCode.NoContent);

        // Assert
        Assert.Null(viewResult.Response);
    }

    [Fact(DisplayName = "Response property should be mutable after construction")]
    [Trait("Notifications", "")]
    public void Response_Property_ShouldBeMutableAfterConstruction()
    {
        // Arrange
        var viewResult = new ViewResult("initial", HttpStatusCode.OK);
        var newResponse = new { Updated = true };

        // Act
        viewResult.Response = newResponse;

        // Assert
        Assert.Equal(newResponse, viewResult.Response);
    }

    [Fact(DisplayName = "StatusCode property should be mutable after construction")]
    [Trait("Notifications", "")]
    public void StatusCode_Property_ShouldBeMutableAfterConstruction()
    {
        // Arrange
        var viewResult = new ViewResult("data", HttpStatusCode.OK);

        // Act
        viewResult.StatusCode = HttpStatusCode.Accepted;

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, viewResult.StatusCode);
    }

    [Fact(DisplayName = "Constructor with complex object response should preserve object reference")]
    [Trait("Notifications", "")]
    public void Constructor_WithComplexObjectResponse_ShouldPreserveObjectReference()
    {
        // Arrange
        var complexObject = new List<string> { "item1", "item2", "item3" };

        // Act
        var viewResult = new ViewResult(complexObject, HttpStatusCode.OK);

        // Assert
        Assert.Same(complexObject, viewResult.Response);
    }

    #endregion

    #region Failure

    [Fact(DisplayName = "StatusCode should not be OK when constructed with BadRequest")]
    [Trait("Notifications", "")]
    public void Constructor_WithBadRequestStatusCode_StatusCodeShouldNotBeOK()
    {
        // Arrange & Act
        var viewResult = new ViewResult("error", HttpStatusCode.BadRequest);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, viewResult.StatusCode);
    }

    [Fact(DisplayName = "Response should not equal a different object after construction")]
    [Trait("Notifications", "")]
    public void Constructor_WithSpecificResponse_ShouldNotEqualDifferentObject()
    {
        // Arrange
        var original = new { Id = 1 };
        var other = new { Id = 2 };

        // Act
        var viewResult = new ViewResult(original, HttpStatusCode.OK);

        // Assert
        Assert.NotEqual(other, viewResult.Response);
    }

    [Fact(DisplayName = "StatusCode should not be NotFound when constructed with OK")]
    [Trait("Notifications", "")]
    public void Constructor_WithOKStatusCode_StatusCodeShouldNotBeNotFound()
    {
        // Arrange & Act
        var viewResult = new ViewResult("ok", HttpStatusCode.OK);

        // Assert
        Assert.NotEqual(HttpStatusCode.NotFound, viewResult.StatusCode);
    }

    #endregion
}
