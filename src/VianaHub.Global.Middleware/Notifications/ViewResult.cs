using System.Net;

namespace EBL.FIG.Common.Middleware.Lib.Notifications;

/// <summary>
/// ViewResult is a class that encapsulates the response and the HTTP status code for a view. It is used to return a response from a controller action along with the appropriate status code. This allows for better handling of responses and errors in the application.
/// </summary>
public class ViewResult
{
    #region Properties

    public object Response { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    #endregion

    #region Public Methods

    public ViewResult(object response, HttpStatusCode statusCode)
    {
        Response = response;
        StatusCode = statusCode;
    }

    #endregion
}
