using System.Net;

namespace VianaHub.Global.Middleware.Lib.Notifications;

/// <summary>
/// Notify class is responsible for managing notifications and error messages within the application. It allows adding messages with associated HTTP status codes, checking if there are any notifications, retrieving error messages, and getting the overall status code based on the notifications present.
/// </summary>
public class Notify : INotify
{
    #region Properties

    /// <summary>
    /// Notifications property is an instance of the ErrorNotify class, which holds the list of error messages and their associated HTTP status codes. This property is used to store and manage the notifications within the Notify class.
    /// </summary>
    private ErrorNotify Notifications { get; set; } = new ErrorNotify();

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a notification message along with an optional HTTP status code (defaulting to 400). The message is stored in the Notifications property, and the status code is set accordingly. This allows for tracking multiple notifications and their associated status codes within the application.
    /// </summary>
    /// <param name="message">Message to be added</param>
    /// <param name="statusCode">HTTP status code associated with the message</param>
    public void Add(string message, int statusCode = 400)
    {
        HttpStatusCode httpStatusCode = (HttpStatusCode)Enum.ToObject(typeof(HttpStatusCode), statusCode);

        Notifications.Add(message, httpStatusCode);
    }

    /// <summary>
    /// HasNotify method checks if there are any notifications present by verifying if the count of messages in the Notifications property is not zero. It returns true if there are notifications, indicating that there are messages to be addressed, and false otherwise. This method is useful for determining whether any errors or notifications have been recorded in the system.
    /// </summary>
    /// <returns>Returns true if there are notifications, false otherwise.</returns>
    public bool HasNotify() => Notifications.Message.Count != 0;

    /// <summary>
    /// GetErrorMessage method retrieves the list of error messages stored in the Notifications property. It returns a list of strings containing all the messages that have been added as notifications. This allows for easy access to the error messages for display or logging purposes within the application.
    /// </summary>
    /// <returns>Returns a list of error messages.</returns>
    public List<string> GetErrorMessage()
    {
        return Notifications.Message;
    }

    /// <summary>
    /// GetStatusCode method determines the overall HTTP status code based on the notifications present. If there are no notifications, it returns HttpStatusCode.OK (200). If there are notifications, it returns the status code associated with the notifications, which is stored in the Notifications property. This method provides a way to determine the appropriate HTTP status code to return in response to a request based on the presence of notifications or errors.
    /// </summary>
    /// <returns>Returns the HTTP status code.</returns>
    public HttpStatusCode GetStatusCode()
    {
        return !HasNotify() ? HttpStatusCode.OK : Notifications.StatusCode;
    }

    #endregion
}
