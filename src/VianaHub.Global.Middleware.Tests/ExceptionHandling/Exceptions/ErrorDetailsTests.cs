using EBL.FIG.Common.Middleware.Lib.ExceptionHandling.Exceptions;
using System.Text.Json;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling.Exceptions;

public class ErrorDetailsTests
{
    #region Sucesso

    [Fact(DisplayName = "ErrorDetails - Constructor should set StatusCode, Message, and CorrelationId correctly")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComParametros_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        const int statusCode = 404;
        const string message = "Resource not found";
        const string correlationId = "abc-123";

        // Act
        var errorDetails = new ErrorDetails(statusCode, message, correlationId);

        // Assert
        Assert.Equal(statusCode, errorDetails.StatusCode);
        Assert.Equal(message, errorDetails.Message);
        Assert.Equal(correlationId, errorDetails.CorrelationId);
    }

    [Fact(DisplayName = "ErrorDetails - CurrentMachine should default to Environment.MachineName when not provided")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_SemCurrentMachine_DeveUsarEnvironmentMachineName()
    {
        // Arrange & Act
        var errorDetails = new ErrorDetails(500, "Error", "corr-id-1");

        // Assert
        Assert.Equal(Environment.MachineName, errorDetails.CurrentMachine);
    }

    [Fact(DisplayName = "ErrorDetails - Constructor with explicit CurrentMachine should use provided value")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComCurrentMachineExplicito_DeveUsarValorFornecido()
    {
        // Arrange
        const string machineName = "SERVER-PROD-01";

        // Act
        var errorDetails = new ErrorDetails(500, "Error", "corr-id-2", machineName);

        // Assert
        Assert.Equal(machineName, errorDetails.CurrentMachine);
    }

    [Fact(DisplayName = "ErrorDetails - ToString should return a valid JSON string")]
    [Trait("ExceptionHandling", "")]
    public void ToString_DeveRetornarJsonValido()
    {
        // Arrange
        var errorDetails = new ErrorDetails(400, "Bad Request", "corr-id-3");

        // Act
        var json = errorDetails.ToString();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.Equal(JsonValueKind.Object, parsed.ValueKind);
    }

    [Fact(DisplayName = "ErrorDetails - ToString JSON should contain StatusCode property")]
    [Trait("ExceptionHandling", "")]
    public void ToString_JsonDeveConterPropriedadeStatusCode()
    {
        // Arrange
        var errorDetails = new ErrorDetails(409, "Conflict", "corr-id-4");

        // Act
        var json = errorDetails.ToString();
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.True(parsed.TryGetProperty("StatusCode", out var statusCodeProp));
        Assert.Equal(409, statusCodeProp.GetInt32());
    }

    [Fact(DisplayName = "ErrorDetails - ToString JSON should contain Message property")]
    [Trait("ExceptionHandling", "")]
    public void ToString_JsonDeveConterPropriedadeMessage()
    {
        // Arrange
        const string message = "Internal Server Error";
        var errorDetails = new ErrorDetails(500, message, "corr-id-5");

        // Act
        var json = errorDetails.ToString();
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.True(parsed.TryGetProperty("Message", out var messageProp));
        Assert.Equal(message, messageProp.GetString());
    }

    [Fact(DisplayName = "ErrorDetails - ToString JSON should contain CorrelationId property")]
    [Trait("ExceptionHandling", "")]
    public void ToString_JsonDeveConterPropriedadeCorrelationId()
    {
        // Arrange
        const string correlationId = "unique-corr-id";
        var errorDetails = new ErrorDetails(500, "Error", correlationId);

        // Act
        var json = errorDetails.ToString();
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.True(parsed.TryGetProperty("CorrelationId", out var corrProp));
        Assert.Equal(correlationId, corrProp.GetString());
    }

    [Fact(DisplayName = "ErrorDetails - Two instances with same values should be equal (record equality)")]
    [Trait("ExceptionHandling", "")]
    public void Record_DuasInstanciasComMesmosValores_DevemSerIguais()
    {
        // Arrange
        var machine = Environment.MachineName;
        var a = new ErrorDetails(200, "OK", "id", machine);
        var b = new ErrorDetails(200, "OK", "id", machine);

        // Assert
        Assert.Equal(a, b);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "ErrorDetails - Two instances with different StatusCode should not be equal")]
    [Trait("ExceptionHandling", "")]
    public void Record_DuasInstanciasComStatusCodeDiferente_NaoDevemSerIguais()
    {
        // Arrange
        var machine = Environment.MachineName;
        var a = new ErrorDetails(200, "OK", "id", machine);
        var b = new ErrorDetails(500, "OK", "id", machine);

        // Assert
        Assert.NotEqual(a, b);
    }

    [Fact(DisplayName = "ErrorDetails - Two instances with different Message should not be equal")]
    [Trait("ExceptionHandling", "")]
    public void Record_DuasInstanciasComMensagemDiferente_NaoDevemSerIguais()
    {
        // Arrange
        var machine = Environment.MachineName;
        var a = new ErrorDetails(404, "Not Found", "id", machine);
        var b = new ErrorDetails(404, "Resource Gone", "id", machine);

        // Assert
        Assert.NotEqual(a, b);
    }

    [Fact(DisplayName = "ErrorDetails - ToString should not return null or empty")]
    [Trait("ExceptionHandling", "")]
    public void ToString_NaoDeveRetornarNuloOuVazio()
    {
        // Arrange
        var errorDetails = new ErrorDetails(500, "Error", "corr-id");

        // Act
        var result = errorDetails.ToString();

        // Assert
        Assert.False(string.IsNullOrEmpty(result));
    }

    #endregion
}
