using System.Net;
using EBL.FIG.Common.Middleware.Lib.Notifications;

namespace EBL.FIG.Common.Middleware.Tests.Notifications;

public class NotifyTests
{
    #region Success

    [Fact(DisplayName = "HasNotify should return false when no messages have been added")]
    [Trait("Notifications", "")]
    public void HasNotify_WithNoMessages_ShouldReturnFalse()
    {
        // Arrange
        var notify = new Notify();

        // Act
        var result = notify.HasNotify();

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "HasNotify should return true after adding a message")]
    [Trait("Notifications", "")]
    public void HasNotify_AfterAddingMessage_ShouldReturnTrue()
    {
        // Arrange
        var notify = new Notify();

        // Act
        notify.Add("Some error");

        // Assert
        Assert.True(notify.HasNotify());
    }

    [Fact(DisplayName = "GetErrorMessage should return empty list when no messages added")]
    [Trait("Notifications", "")]
    public void GetErrorMessage_WithNoMessages_ShouldReturnEmptyList()
    {
        // Arrange
        var notify = new Notify();

        // Act
        var result = notify.GetErrorMessage();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetErrorMessage should return all added messages")]
    [Trait("Notifications", "")]
    public void GetErrorMessage_AfterAddingMessages_ShouldReturnAllMessages()
    {
        // Arrange
        var notify = new Notify();
        const string firstMessage = "First error";
        const string secondMessage = "Second error";

        // Act
        notify.Add(firstMessage);
        notify.Add(secondMessage);
        var result = notify.GetErrorMessage();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(firstMessage, result);
        Assert.Contains(secondMessage, result);
    }

    [Fact(DisplayName = "GetStatusCode should return OK when there are no notifications")]
    [Trait("Notifications", "")]
    public void GetStatusCode_WithNoNotifications_ShouldReturnOK()
    {
        // Arrange
        var notify = new Notify();

        // Act
        var result = notify.GetStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result);
    }

    [Fact(DisplayName = "GetStatusCode should return BadRequest when added with default status code")]
    [Trait("Notifications", "")]
    public void GetStatusCode_WithDefaultStatusCode_ShouldReturnBadRequest()
    {
        // Arrange
        var notify = new Notify();

        // Act
        notify.Add("Validation error");
        var result = notify.GetStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result);
    }

    [Fact(DisplayName = "GetStatusCode should return the explicitly provided status code")]
    [Trait("Notifications", "")]
    public void GetStatusCode_WithExplicitStatusCode_ShouldReturnProvidedStatusCode()
    {
        // Arrange
        var notify = new Notify();

        // Act
        notify.Add("Not found", 404);
        var result = notify.GetStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result);
    }

    [Fact(DisplayName = "Add should accept custom HTTP status code")]
    [Trait("Notifications", "")]
    public void Add_WithCustomStatusCode_ShouldSetCorrectStatusCode()
    {
        // Arrange
        var notify = new Notify();

        // Act
        notify.Add("Conflict error", 409);
        var result = notify.GetStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, result);
    }

    [Fact(DisplayName = "Add multiple messages should update StatusCode to the last one provided")]
    [Trait("Notifications", "")]
    public void Add_MultipleMessages_ShouldUpdateStatusCodeToLastProvided()
    {
        // Arrange
        var notify = new Notify();

        // Act
        notify.Add("Bad request error", 400);
        notify.Add("Internal server error", 500);
        var result = notify.GetStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, result);
    }

    [Fact(DisplayName = "GetErrorMessage should return list with single item when one message is added")]
    [Trait("Notifications", "")]
    public void GetErrorMessage_WithOneMessage_ShouldReturnSingleItemList()
    {
        // Arrange
        var notify = new Notify();
        const string message = "Single error";

        // Act
        notify.Add(message);
        var result = notify.GetErrorMessage();

        // Assert
        Assert.Single(result);
        Assert.Equal(message, result[0]);
    }

    #endregion

    #region Failure

    [Fact(DisplayName = "GetStatusCode should not return BadRequest when there are no notifications")]
    [Trait("Notifications", "")]
    public void GetStatusCode_WithNoNotifications_ShouldNotReturnBadRequest()
    {
        // Arrange
        var notify = new Notify();

        // Act
        var result = notify.GetStatusCode();

        // Assert
        Assert.NotEqual(HttpStatusCode.BadRequest, result);
    }

    [Fact(DisplayName = "HasNotify should not return true when no messages added")]
    [Trait("Notifications", "")]
    public void HasNotify_WithNoMessages_ShouldNotReturnTrue()
    {
        // Arrange
        var notify = new Notify();

        // Act
        var result = notify.HasNotify();

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "GetErrorMessage should not contain unregistered messages")]
    [Trait("Notifications", "")]
    public void GetErrorMessage_ShouldNotContainUnregisteredMessages()
    {
        // Arrange
        var notify = new Notify();
        notify.Add("Registered error");

        // Act
        var result = notify.GetErrorMessage();

        // Assert
        Assert.DoesNotContain("Unregistered error", result);
    }

    [Fact(DisplayName = "GetStatusCode should not return OK after adding an error notification")]
    [Trait("Notifications", "")]
    public void GetStatusCode_AfterAddingError_ShouldNotReturnOK()
    {
        // Arrange
        var notify = new Notify();

        // Act
        notify.Add("Error", 400);
        var result = notify.GetStatusCode();

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, result);
    }

    #endregion
}
