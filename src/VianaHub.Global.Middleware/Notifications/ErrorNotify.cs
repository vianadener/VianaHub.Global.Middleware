using System.Net;

namespace VianaHub.Global.Middleware.Lib.Notifications;

/// <summary>
/// ErrorNotify is a class that represents a notification for errors. It contains a list of error messages and an associated HTTP status code. The Add method allows adding a message to the notification list with an optional status code (default is 400 Bad Request). This class can be used
/// </summary>
public class ErrorNotify
{
    #region Properties

    /// <summary>
    /// StatusCode property holds the HTTP status code associated with the error notifications. It is set when a message is added to the notification list using the Add method. This allows for tracking the status code related to the error messages, which can be useful for determining the appropriate response to return in case of errors.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Message property is a list of strings that holds the error messages associated with the notifications. When a message is added using the Add method, it is stored in this list. This allows for managing multiple error messages within a single notification instance, providing a way to track and display all relevant error information when needed.
    /// </summary>
    public List<string> Message { get; set; } = [];

    #endregion

    #region Public Methods

    /// <summary>
    /// Add a message to the notification list with an optional status code (default is 400 Bad Request).
    /// </summary>
    /// <param name="message">Message to be added</param>
    /// <param name="statusCode">HTTP status code associated with the message</param>
    public void Add(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        Message.Add(message);
        StatusCode = statusCode;
    }

    #endregion
}
