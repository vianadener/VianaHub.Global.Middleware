using EBL.FIG.Common.Middleware.Lib.Notifications;

namespace EBL.FIG.Common.Middleware.Tests.Notifications;

public class ErrorResponseTests
{
    #region Success

    [Fact(DisplayName = "Default constructor should initialize Title as empty string")]
    [Trait("Notifications", "")]
    public void Constructor_Default_TitleShouldBeEmptyString()
    {
        // Arrange & Act
        var errorResponse = new ErrorResponse();

        // Assert
        Assert.Equal(string.Empty, errorResponse.Title);
    }

    [Fact(DisplayName = "Default constructor should initialize Errors as empty dictionary")]
    [Trait("Notifications", "")]
    public void Constructor_Default_ErrorsShouldBeEmptyDictionary()
    {
        // Arrange & Act
        var errorResponse = new ErrorResponse();

        // Assert
        Assert.NotNull(errorResponse.Errors);
        Assert.Empty(errorResponse.Errors);
    }

    [Fact(DisplayName = "Constructor with title should set Title correctly")]
    [Trait("Notifications", "")]
    public void Constructor_WithTitle_ShouldSetTitleCorrectly()
    {
        // Arrange
        const string title = "BadRequest";

        // Act
        var errorResponse = new ErrorResponse(title);

        // Assert
        Assert.Equal(title, errorResponse.Title);
    }

    [Fact(DisplayName = "Constructor with title should initialize Errors as empty dictionary")]
    [Trait("Notifications", "")]
    public void Constructor_WithTitle_ErrorsShouldBeEmptyDictionary()
    {
        // Arrange & Act
        var errorResponse = new ErrorResponse("NotFound");

        // Assert
        Assert.NotNull(errorResponse.Errors);
        Assert.Empty(errorResponse.Errors);
    }

    [Fact(DisplayName = "Constructor with title, field, and errorMessage should set all properties")]
    [Trait("Notifications", "")]
    public void Constructor_WithTitleFieldAndErrorMessage_ShouldSetAllProperties()
    {
        // Arrange
        const string title = "BadRequest";
        const string field = "Name";
        const string errorMessage = "Name is required";

        // Act
        var errorResponse = new ErrorResponse(title, field, errorMessage);

        // Assert
        Assert.Equal(title, errorResponse.Title);
        Assert.True(errorResponse.Errors.ContainsKey(field));
        Assert.Contains(errorMessage, errorResponse.Errors[field]);
    }

    [Fact(DisplayName = "Constructor with title and dictionary should set Errors correctly")]
    [Trait("Notifications", "")]
    public void Constructor_WithTitleAndDictionary_ShouldSetErrorsCorrectly()
    {
        // Arrange
        const string title = "ValidationError";
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is invalid" } },
            { "Phone", new[] { "Phone is required" } }
        };

        // Act
        var errorResponse = new ErrorResponse(title, errors);

        // Assert
        Assert.Equal(title, errorResponse.Title);
        Assert.Equal(errors, errorResponse.Errors);
    }

    [Fact(DisplayName = "AddError with message should add error under 'Error' key")]
    [Trait("Notifications", "")]
    public void AddError_WithMessage_ShouldAddUnderErrorKey()
    {
        // Arrange
        var errorResponse = new ErrorResponse("BadRequest");
        const string errorMessage = "General error occurred";

        // Act
        errorResponse.AddError(errorMessage);

        // Assert
        Assert.True(errorResponse.Errors.ContainsKey("Error"));
        Assert.Contains(errorMessage, errorResponse.Errors["Error"]);
    }

    [Fact(DisplayName = "AddError with field and message should add error under that field")]
    [Trait("Notifications", "")]
    public void AddError_WithFieldAndMessage_ShouldAddUnderSpecificField()
    {
        // Arrange
        var errorResponse = new ErrorResponse("BadRequest");
        const string field = "Username";
        const string errorMessage = "Username is already taken";

        // Act
        errorResponse.AddError(field, errorMessage);

        // Assert
        Assert.True(errorResponse.Errors.ContainsKey(field));
        Assert.Contains(errorMessage, errorResponse.Errors[field]);
    }

    [Fact(DisplayName = "AddError with same field twice should accumulate messages")]
    [Trait("Notifications", "")]
    public void AddError_SameFieldTwice_ShouldAccumulateMessages()
    {
        // Arrange
        var errorResponse = new ErrorResponse("BadRequest");
        const string field = "Password";

        // Act
        errorResponse.AddError(field, "Password is too short");
        errorResponse.AddError(field, "Password must contain a number");

        // Assert
        Assert.Equal(2, errorResponse.Errors[field].Length);
    }

    [Fact(DisplayName = "AddErrors with multiple messages should add all under the specified field")]
    [Trait("Notifications", "")]
    public void AddErrors_WithMultipleMessages_ShouldAddAllUnderField()
    {
        // Arrange
        var errorResponse = new ErrorResponse("ValidationError");
        const string field = "Address";

        // Act
        errorResponse.AddErrors(field, "Street is required", "City is required", "ZipCode is required");

        // Assert
        Assert.Equal(3, errorResponse.Errors[field].Length);
    }

    [Fact(DisplayName = "AddErrors on existing field should accumulate all messages")]
    [Trait("Notifications", "")]
    public void AddErrors_OnExistingField_ShouldAccumulateAllMessages()
    {
        // Arrange
        var errorResponse = new ErrorResponse("ValidationError");
        const string field = "Name";
        errorResponse.AddError(field, "Name is required");

        // Act
        errorResponse.AddErrors(field, "Name is too short", "Name must start with uppercase");

        // Assert
        Assert.Equal(3, errorResponse.Errors[field].Length);
    }

    [Fact(DisplayName = "AddError with new message should overwrite previous error under 'Error' key")]
    [Trait("Notifications", "")]
    public void AddError_CalledTwiceWithGeneralMessage_ShouldOverwritePreviousError()
    {
        // Arrange
        var errorResponse = new ErrorResponse("BadRequest");

        // Act
        errorResponse.AddError("First error");
        errorResponse.AddError("Second error");

        // Assert
        Assert.Single(errorResponse.Errors["Error"]);
        Assert.Equal("Second error", errorResponse.Errors["Error"][0]);
    }

    #endregion

    #region Failure

    [Fact(DisplayName = "Errors should not contain key when no error was added for that field")]
    [Trait("Notifications", "")]
    public void Errors_WithNoAddedErrors_ShouldNotContainSpecificKey()
    {
        // Arrange
        var errorResponse = new ErrorResponse("BadRequest");

        // Act & Assert
        Assert.False(errorResponse.Errors.ContainsKey("NonExistentField"));
    }

    [Fact(DisplayName = "AddError with field should not add under 'Error' key")]
    [Trait("Notifications", "")]
    public void AddError_WithFieldSpecified_ShouldNotAddUnderDefaultErrorKey()
    {
        // Arrange
        var errorResponse = new ErrorResponse("BadRequest");

        // Act
        errorResponse.AddError("Email", "Email is invalid");

        // Assert
        Assert.False(errorResponse.Errors.ContainsKey("Error"));
    }

    [Fact(DisplayName = "Constructor with title should not set Errors from another instance")]
    [Trait("Notifications", "")]
    public void Constructor_WithTitle_ErrorsShouldBeIndependentFromOtherInstances()
    {
        // Arrange
        var first = new ErrorResponse("BadRequest");
        first.AddError("Field", "Error msg");

        // Act
        var second = new ErrorResponse("NotFound");

        // Assert
        Assert.Empty(second.Errors);
    }

    [Fact(DisplayName = "AddErrors with no messages should add empty array for field")]
    [Trait("Notifications", "")]
    public void AddErrors_WithNoMessages_ShouldAddEmptyArrayForField()
    {
        // Arrange
        var errorResponse = new ErrorResponse("BadRequest");
        const string field = "OptionalField";

        // Act
        errorResponse.AddErrors(field);

        // Assert
        Assert.True(errorResponse.Errors.ContainsKey(field));
        Assert.Empty(errorResponse.Errors[field]);
    }

    #endregion
}
