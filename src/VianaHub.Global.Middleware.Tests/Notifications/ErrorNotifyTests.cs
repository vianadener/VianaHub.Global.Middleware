using System.Net;
using EBL.FIG.Common.Middleware.Lib.Notifications;

namespace EBL.FIG.Common.Middleware.Tests.Notifications;

public class ErrorNotifyTests
{
    #region Success

    [Fact(DisplayName = "Message list should be empty on initialization")]
    [Trait("Notifications", "")]
    public void Constructor_Initialization_MessageListShouldBeEmpty()
    {
        // Arrange & Act
        var errorNotify = new ErrorNotify();

        // Assert
        Assert.NotNull(errorNotify.Message);
        Assert.Empty(errorNotify.Message);
    }

    [Fact(DisplayName = "Add should append message to Message list")]
    [Trait("Notifications", "")]
    public void Add_WithMessage_ShouldAppendMessageToList()
    {
        // Arrange
        var errorNotify = new ErrorNotify();
        const string message = "An error occurred";

        // Act
        errorNotify.Add(message);

        // Assert
        Assert.Single(errorNotify.Message);
        Assert.Contains(message, errorNotify.Message);
    }

    [Fact(DisplayName = "Add should set StatusCode to provided value")]
    [Trait("Notifications", "")]
    public void Add_WithStatusCode_ShouldSetStatusCode()
    {
        // Arrange
        var errorNotify = new ErrorNotify();

        // Act
        errorNotify.Add("Not found", HttpStatusCode.NotFound);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, errorNotify.StatusCode);
    }

    [Fact(DisplayName = "Add without StatusCode should default to BadRequest")]
    [Trait("Notifications", "")]
    public void Add_WithoutStatusCode_ShouldDefaultToBadRequest()
    {
        // Arrange
        var errorNotify = new ErrorNotify();

        // Act
        errorNotify.Add("Validation error");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, errorNotify.StatusCode);
    }

    [Fact(DisplayName = "Add multiple messages should accumulate all messages in list")]
    [Trait("Notifications", "")]
    public void Add_MultipleMessages_ShouldAccumulateAllMessages()
    {
        // Arrange
        var errorNotify = new ErrorNotify();

        // Act
        errorNotify.Add("First error");
        errorNotify.Add("Second error");
        errorNotify.Add("Third error");

        // Assert
        Assert.Equal(3, errorNotify.Message.Count);
    }

    [Fact(DisplayName = "Add multiple messages should update StatusCode to last provided value")]
    [Trait("Notifications", "")]
    public void Add_MultipleMessages_ShouldUpdateStatusCodeToLastValue()
    {
        // Arrange
        var errorNotify = new ErrorNotify();

        // Act
        errorNotify.Add("Bad request error", HttpStatusCode.BadRequest);
        errorNotify.Add("Internal error", HttpStatusCode.InternalServerError);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, errorNotify.StatusCode);
    }

    [Fact(DisplayName = "Message list should be a mutable list of strings")]
    [Trait("Notifications", "")]
    public void Message_Property_ShouldBeMutableListOfStrings()
    {
        // Arrange
        var errorNotify = new ErrorNotify();
        var customList = new List<string> { "custom error" };

        // Act
        errorNotify.Message = customList;

        // Assert
        Assert.Equal(customList, errorNotify.Message);
    }

    #endregion

    #region Failure

    [Fact(DisplayName = "Add with empty message should still add to list")]
    [Trait("Notifications", "")]
    public void Add_WithEmptyMessage_ShouldStillAddToList()
    {
        // Arrange
        var errorNotify = new ErrorNotify();

        // Act
        errorNotify.Add(string.Empty);

        // Assert
        Assert.Single(errorNotify.Message);
    }

    [Fact(DisplayName = "StatusCode should not be OK when a message is added with BadRequest")]
    [Trait("Notifications", "")]
    public void Add_WithBadRequest_StatusCodeShouldNotBeOK()
    {
        // Arrange
        var errorNotify = new ErrorNotify();

        // Act
        errorNotify.Add("Validation failed", HttpStatusCode.BadRequest);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, errorNotify.StatusCode);
    }

    [Fact(DisplayName = "Message list should not be null after multiple adds")]
    [Trait("Notifications", "")]
    public void Add_MultipleMessages_MessageListShouldNeverBeNull()
    {
        // Arrange
        var errorNotify = new ErrorNotify();

        // Act
        errorNotify.Add("msg1");
        errorNotify.Add("msg2");

        // Assert
        Assert.NotNull(errorNotify.Message);
    }

    #endregion
}
