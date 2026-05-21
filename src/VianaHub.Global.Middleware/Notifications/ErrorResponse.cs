namespace EBL.FIG.Common.Middleware.Lib.Notifications;

/// <summary>
/// ErrorResponse class is a model that represents the structure of an error response. It contains a Title property to provide a brief description of the error and an Errors property, which is a dictionary that maps field names to arrays of error messages. This class can be used
/// </summary>
public class ErrorResponse
{
    #region Properties

    /// <summary>
    /// Title property is a string that holds a brief description of the error. It provides a concise summary of the error that occurred, allowing for easy identification and understanding of the issue. This property can be used to convey the main reason for the error in a clear and straightforward manner.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Errors property is a dictionary that maps field names (as strings) to arrays of error messages (as string arrays). This structure allows for organizing error messages based on specific fields or categories, making it easier to identify which fields are associated with which errors. The dictionary can contain multiple entries, each representing a different field and its corresponding error messages, providing a comprehensive overview of the errors that occurred.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();

    #endregion

    #region Public Constructor

    /// <summary>
    /// Constructor for the ErrorResponse class. It initializes a new instance of the ErrorResponse class with default values for the Title and Errors properties. The Title is set to an empty string, and the Errors dictionary is initialized as an empty dictionary. This constructor allows for creating an instance of the ErrorResponse class without providing any initial values, allowing for flexibility in how the error response is constructed and populated with data later on.
    /// </summary>
    public ErrorResponse()
    {
    }

    /// <summary>
    /// Constructor for the ErrorResponse class that takes a title as a parameter. It initializes a new instance of the ErrorResponse class with the provided title and an empty Errors dictionary. This constructor allows for creating an instance of the ErrorResponse class with a specific title, providing a brief description of the error while still allowing for the Errors dictionary to be populated with error messages later on.
    /// </summary>
    /// <param name="title"></param>
    public ErrorResponse(string title)
    {
        Title = title;
    }

    /// <summary>
    /// ErrorResponse constructor that takes a title, a field, and an error message as parameters. It initializes a new instance of the ErrorResponse class with the provided title and adds the error message to the Errors dictionary under the specified field. This constructor allows for creating an instance of the ErrorResponse class with a specific title and immediately associating an error message with a particular field, providing a structured way to represent errors related to specific fields in the response.
    /// </summary>
    /// <param name="title">Title of the error response</param>
    /// <param name="field">Field associated with the error</param>
    /// <param name="errorMessage">Error message</param>
    public ErrorResponse(string title, string field, string errorMessage)
    {
        Title = title;
        Errors[field] = new[] { errorMessage };
    }

    /// <summary>
    /// ErrorResponse constructor that takes a title and a dictionary of errors as parameters. It initializes a new instance of the ErrorResponse class with the provided title and sets the Errors property to the provided dictionary. This constructor allows for creating an instance of the ErrorResponse class with a specific title and a predefined set of errors, providing a comprehensive way to represent multiple errors associated with different fields in the response.
    /// </summary>
    /// <param name="title">Title of the error response</param>
    /// <param name="errors">Dictionary of errors</param>
    public ErrorResponse(string title, Dictionary<string, string[]> errors)
    {
        Title = title;
        Errors = errors;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// AddError method allows adding an error message to the Errors dictionary. It takes a single error message as a parameter and adds it under the key "Error". This method provides a way to add a general error message that is not associated with a specific field, allowing for flexibility in how errors are represented in the response. If there are already errors under the "Error" key, it will overwrite them with the new error message.
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    public void AddError(string errorMessage)
    {
        Errors["Error"] = new[] { errorMessage };
    }

    /// <summary>
    /// AddError method allows adding an error message to the Errors dictionary under a specific field. It takes a field name and an error message as parameters. If there are already error messages associated with the specified field, it appends the new error message to the existing list of errors for that field. If there are no existing errors for the field, it creates a new entry in the Errors dictionary with the provided error message. This method provides a way to organize error messages based on specific fields, allowing for a more structured representation of errors in the response.
    /// </summary>
    /// <param name="field">Field associated with the error</param>
    /// <param name="errorMessage">Error message</param>
    public void AddError(string field, string errorMessage)
    {
        if (Errors.TryGetValue(field, out string[]? value))
        {
            var existingErrors = value.ToList();
            existingErrors.Add(errorMessage);
            Errors[field] = existingErrors.ToArray();
        }
        else
        {
            Errors[field] = new[] { errorMessage };
        }
    }

    /// <summary>
    /// AddErrors method allows adding multiple error messages to the Errors dictionary under a specific field. It takes a field name and an array of error messages as parameters. If there are already error messages associated with the specified field, it appends the new error messages to the existing list of errors for that field. If there are no existing errors for the field, it creates a new entry in the Errors dictionary with the provided array of error messages. This method provides a way to efficiently add multiple error messages for a specific field, allowing for a more comprehensive representation of errors in the response.
    /// </summary>
    /// <param name="field">Field associated with the errors</param>
    /// <param name="errorMessages">Array of error messages</param>
    public void AddErrors(string field, params string[] errorMessages)
    {
        if (Errors.TryGetValue(field, out string[]? value))
        {
            var existingErrors = value.ToList();
            existingErrors.AddRange(errorMessages);
            Errors[field] = existingErrors.ToArray();
        }
        else
        {
            Errors[field] = errorMessages;
        }
    }

    #endregion
}
