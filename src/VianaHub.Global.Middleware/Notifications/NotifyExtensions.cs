using VianaHub.Global.Middleware.Lib.Extensions;
using Microsoft.AspNetCore.Http;

namespace VianaHub.Global.Middleware.Lib.Notifications;

/// <summary>
/// Classe responsável por mapear os endpoints de NotifyExtensions.
/// </summary>
public static class NotifyExtensions
{
    #region Public Methods

    /// <summary>
    /// Retorna uma resposta customizada para o padrão de notificação, sem dados de retorno e com status code 200 por padrão.
    /// </summary>
    /// <param name="notify">Notificação.</param>
    /// <returns>Resultado da resposta.</returns>
    public static IResult CustomResponse(this INotify notify)
    {
        if (notify.HasNotify())
        {
            var statusCode = (int)notify.GetStatusCode();
            var errorMessages = notify.GetErrorMessage();
            var errorResponse = new ErrorResponse(GetErrorTitle(statusCode));
            foreach (var message in errorMessages)
            {
                if (message.Contains(':'))
                {
                    var parts = message.Split(':', 2);
                    var field = parts[0].Trim();
                    var payload = parts[1].Trim();

                    errorResponse.AddError(field, payload);
                }
                else
                {
                    errorResponse.AddError(message);
                }
            }

            return Results.Json(errorResponse, statusCode: statusCode);
        }

        return Results.Ok();
    }

    /// <summary>
    /// Retorna uma resposta customizada para o padrão de notificação, sem dados de retorno.
    /// </summary>
    /// <param name="notify">Notificação.</param>
    /// <param name="statusCode">Código de status HTTP.</param>
    /// <returns>Resultado da resposta.</returns>
    public static IResult CustomResponse(this INotify notify, int statusCode)
    {
        // Se houver notificações de erro
        if (notify.HasNotify())
        {
            statusCode = statusCode >= (int)notify.GetStatusCode() ? statusCode : 200;
            var errorMessages = notify.GetErrorMessage();
            var errorResponse = new ErrorResponse(GetErrorTitle(statusCode));

            foreach (var message in errorMessages)
            {
                if (message.Contains(':'))
                {
                    var parts = message.Split(':', 2);
                    var field = parts[0].Trim();
                    var payload = parts[1].Trim();

                    errorResponse.AddError(field, payload);
                }
                else
                {
                    errorResponse.AddError(message);
                }
            }

            return Results.Json(errorResponse, statusCode: statusCode);
        }

        // Sem erros → devolve o status code solicitado
        if (statusCode == 204)
        {
            return Results.NoContent();
        }

        return Results.StatusCode(statusCode);
    }

    /// <summary>
    /// Retorna uma resposta customizada para o padrão de notificação.
    /// </summary>
    /// <param name="notify">Notificação.</param>
    /// <param name="data">Dados a serem retornados em caso de sucesso.</param>
    /// <param name="statusCode">Código de status HTTP.</param>
    /// <typeparam name="T">Tipo dos dados a serem retornados.</typeparam>
    /// <returns>Resposta customizada.</returns>
    public static IResult CustomResponse<T>(this INotify notify, T data, int statusCode = 200)
    {
        if (notify.HasNotify())
        {
            statusCode = (int)notify.GetStatusCode();
            var errorMessages = notify.GetErrorMessage();
            var errorResponse = new ErrorResponse(GetErrorTitle(statusCode));

            foreach (var message in errorMessages)
            {
                if (message.Contains(':'))
                {
                    var parts = message.Split(':', 2);
                    var field = parts[0].Trim();
                    var payload = parts[1].Trim();

                    errorResponse.AddError(field, payload);
                }
                else
                {
                    errorResponse.AddError(message);
                }
            }

            return Results.Json(errorResponse, statusCode: statusCode);
        }

        if (statusCode == 204)
        {
            return Results.NoContent();
        }

        return Results.Json(data, statusCode: statusCode);
    }

    /// <summary>
    /// Retorna uma resposta customizada para o padrão de notificação, com um arquivo ZIP.
    /// </summary>
    /// <param name="notify">Notificação.</param>
    /// <param name="data">Dados do arquivo ZIP.</param>
    /// <param name="statusCode">Código de status HTTP.</param>
    /// <returns>Resultado da resposta.</returns>
    public static IResult CustomResponse<T>(this INotify notify, MemoryStream data, int statusCode = 200) where T : MemoryStream
    {
        if (notify.HasNotify())
        {
            statusCode = (int)notify.GetStatusCode();
            var errorMessages = notify.GetErrorMessage();
            var errorResponse = new ErrorResponse(GetErrorTitle(statusCode));

            foreach (var message in errorMessages)
            {
                if (message.Contains(':'))
                {
                    var parts = message.Split(':', 2);
                    var field = parts[0].Trim();
                    var payload = parts[1].Trim();

                    errorResponse.AddError(field, payload);
                }
                else
                {
                    errorResponse.AddError(message);
                }
            }

            return Results.Json(errorResponse, statusCode: statusCode);
        }

        if (statusCode == 204)
        {
            return Results.NoContent();
        }

        return Results.File(data.ToArray(), "application/zip", $"{Guid.NewGuid()}.zip");
    }

    /// <summary>
    /// Retorna uma resposta customizada para o padrão de notificação, com um arquivo PDF.
    /// </summary>
    /// <param name="notify">Notificação.</param>
    /// <param name="data">Dados do arquivo PDF.</param>
    /// <param name="statusCode">Código de status HTTP.</param>
    /// <returns>Resultado da resposta.</returns>
    public static IResult CustomResponse(this INotify notify, byte[] data, int statusCode = 200)
    {
        if (notify.HasNotify())
        {
            statusCode = (int)notify.GetStatusCode();
            var errorMessages = notify.GetErrorMessage();
            var errorResponse = new ErrorResponse(GetErrorTitle(statusCode));

            foreach (var message in errorMessages)
            {
                if (message.Contains(':'))
                {
                    var parts = message.Split(':', 2);
                    var field = parts[0].Trim();
                    var payload = parts[1].Trim();

                    errorResponse.AddError(field, payload);
                }
                else
                {
                    errorResponse.AddError(message);
                }
            }

            return Results.Json(errorResponse, statusCode: statusCode);
        }

        if (statusCode == 204)
        {
            return Results.NoContent();
        }

        return Results.File(data, "application/pdf", $"{Guid.NewGuid()}.pdf");
    }

    /// <summary>
    ///  Retorna uma resposta customizada para o padrão de notificação, sem dados de retorno e com status code 200 por padrão.
    /// </summary>
    /// <param name="notify">Notificação.</param>
    /// <param name="field">Campo do erro.</param>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="statusCode">Código de status HTTP.</param>
    public static void AddFieldError(this INotify notify, string field, string message, int statusCode = 400)
    {
        // Mantém formato "field: payload" onde payload pode ser uma chave ou texto literal.
        notify.Add($"{field}: {message}", statusCode);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// GetErrorTitle method maps HTTP status codes to corresponding error titles. It uses a switch expression to return a string title based on the provided status code. If the status code does not match any of the predefined cases, it defaults to returning "BadRequest". This method helps in providing a standardized error title for different HTTP status codes when generating error responses.
    /// </summary>
    /// <param name="statusCode">Status code to map</param>
    /// <returns>Error title</returns>
    private static string GetErrorTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "BadRequest",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "NotFound",
            409 => "Conflict",
            410 => "Gone",
            422 => "UnprocessableEntity",
            429 => "TooManyRequests",
            500 => "InternalServerError",
            503 => "ServiceUnavailable",
            _ => "BadRequest"
        };
    }

    #endregion
}
