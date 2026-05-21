using System.Net;

namespace EBL.FIG.Common.Middleware.Lib.Notifications;

public interface INotify
{
    /// <summary>
    /// Add a message to the notification list with an optional status code (default is 400 Bad Request).
    /// </summary>
    /// <param name="message">Message to be added</param>
    /// <param name="statusCode">HTTP status code associated with the message</param>
    void Add(string message, int statusCode = 400);

    /// <summary>
    /// HasNotify checks if there are any notifications present in the list.
    /// </summary>
    /// <returns>Returns true if there are notifications, false otherwise.</returns>
    bool HasNotify();

    /// <summary>
    /// GetErrorMessage retrieves the list of error messages that have been added to the notification list.
    /// </summary>
    /// <returns>Returns a list of error messages.</returns>
    List<string> GetErrorMessage();

    /// <summary>
    /// GetStatusCode retrieves the HTTP status code associated with the notifications. If multiple notifications have been added with different status codes, it returns the most recent one.
    /// </summary>
    /// <returns>Returns the HTTP status code.</returns>
    HttpStatusCode GetStatusCode();
}
