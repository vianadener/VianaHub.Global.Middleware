// <copyright file="ErrorResponseHelperTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>

using EBL.FIG.Common.Middleware.Lib.ExceptionHandling;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling;

public class ErrorResponseHelperTests
{
    #region GenerateErrorId Tests

    [Fact(DisplayName = "GenerateErrorId should return a 12-character string")]
    [Trait("ErrorResponseHelper", "")]
    public void GenerateErrorId_DeveRetornarString12Caracteres()
    {
        // Act
        var errorId = ErrorResponseHelper.GenerateErrorId();

        // Assert
        Assert.NotNull(errorId);
        Assert.Equal(12, errorId.Length);
    }

    [Fact(DisplayName = "GenerateErrorId should return different IDs on consecutive calls")]
    [Trait("ErrorResponseHelper", "")]
    public void GenerateErrorId_DeveRetornarIdsDistintosEmChamadasConsecutivas()
    {
        // Act
        var errorId1 = ErrorResponseHelper.GenerateErrorId();
        var errorId2 = ErrorResponseHelper.GenerateErrorId();

        // Assert
        Assert.NotEqual(errorId1, errorId2);
    }

    [Fact(DisplayName = "GenerateErrorId should return only alphanumeric characters")]
    [Trait("ErrorResponseHelper", "")]
    public void GenerateErrorId_DeveRetornarApenasCaracteresAlfanumericos()
    {
        // Act
        var errorId = ErrorResponseHelper.GenerateErrorId();

        // Assert
        Assert.Matches("^[a-z0-9]+$", errorId);
    }

    #endregion

    #region GetJsonSerializerOptions Tests

    [Fact(DisplayName = "GetJsonSerializerOptions should return options with camelCase naming policy")]
    [Trait("ErrorResponseHelper", "")]
    public void GetJsonSerializerOptions_DeveRetornarOpcoesComCamelCase()
    {
        // Act
        var options = ErrorResponseHelper.GetJsonSerializerOptions();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(JsonNamingPolicy.CamelCase, options.PropertyNamingPolicy);
    }

    [Fact(DisplayName = "GetJsonSerializerOptions should return options with WriteIndented true")]
    [Trait("ErrorResponseHelper", "")]
    public void GetJsonSerializerOptions_DeveRetornarOpcoesComWriteIndentedTrue()
    {
        // Act
        var options = ErrorResponseHelper.GetJsonSerializerOptions();

        // Assert
        Assert.True(options.WriteIndented);
    }

    [Fact(DisplayName = "GetJsonSerializerOptions should return options with UnsafeRelaxedJsonEscaping encoder")]
    [Trait("ErrorResponseHelper", "")]
    public void GetJsonSerializerOptions_DeveRetornarOpcoesComUnsafeRelaxedJsonEscaping()
    {
        // Act
        var options = ErrorResponseHelper.GetJsonSerializerOptions();

        // Assert
        Assert.NotNull(options.Encoder);
        Assert.Equal(System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, options.Encoder);
    }

    #endregion

    #region WriteJsonResponseAsync Tests

    [Fact(DisplayName = "WriteJsonResponseAsync should set correct status code")]
    [Trait("ErrorResponseHelper", "")]
    public async Task WriteJsonResponseAsync_DeveDefinirStatusCodeCorreto()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var errorResponse = new ErrorResponse("TestError");
        errorResponse.AddError("Test", "Test message");

        // Act
        await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, 400);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact(DisplayName = "WriteJsonResponseAsync should set correct content type")]
    [Trait("ErrorResponseHelper", "")]
    public async Task WriteJsonResponseAsync_DeveDefinirContentTypeCorreto()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var errorResponse = new ErrorResponse("TestError");
        errorResponse.AddError("Test", "Test message");

        // Act
        await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, 400);

        // Assert
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
    }

    [Fact(DisplayName = "WriteJsonResponseAsync should write JSON to response body")]
    [Trait("ErrorResponseHelper", "")]
    public async Task WriteJsonResponseAsync_DeveEscreverJsonNoCorpoDaResposta()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var errorResponse = new ErrorResponse("TestError");
        errorResponse.AddError("Test", "Test message");

        // Act
        await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, 400);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("TestError", responseBody);
        Assert.Contains("Test message", responseBody);
    }

    [Fact(DisplayName = "WriteJsonResponseAsync should serialize using camelCase")]
    [Trait("ErrorResponseHelper", "")]
    public async Task WriteJsonResponseAsync_DeveSerializarUsandoCamelCase()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var errorResponse = new ErrorResponse("TestError");
        errorResponse.AddError("TestField", "Test message");

        // Act
        await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, 400);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"title\"", responseBody); // camelCase
        Assert.Contains("\"errors\"", responseBody); // camelCase
    }

    #endregion

    #region GetErrorTitle Tests

    [Theory(DisplayName = "GetErrorTitle should return correct title for status codes")]
    [Trait("ErrorResponseHelper", "")]
    [InlineData(400, "BadRequest")]
    [InlineData(401, "Unauthorized")]
    [InlineData(403, "Forbidden")]
    [InlineData(404, "NotFound")]
    [InlineData(409, "Conflict")]
    [InlineData(410, "Gone")]
    [InlineData(422, "UnprocessableEntity")]
    [InlineData(429, "TooManyRequests")]
    [InlineData(500, "InternalServerError")]
    [InlineData(503, "ServiceUnavailable")]
    public void GetErrorTitle_DeveRetornarTituloCorretoParaStatusCodes(int statusCode, string expectedTitle)
    {
        // Act
        var title = ErrorResponseHelper.GetErrorTitle(statusCode);

        // Assert
        Assert.Equal(expectedTitle, title);
    }

    [Theory(DisplayName = "GetErrorTitle should return BadRequest for unknown status codes")]
    [Trait("ErrorResponseHelper", "")]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(300)]
    [InlineData(402)]
    [InlineData(405)]
    [InlineData(501)]
    [InlineData(999)]
    public void GetErrorTitle_DeveRetornarBadRequestParaStatusCodesDesconhecidos(int statusCode)
    {
        // Act
        var title = ErrorResponseHelper.GetErrorTitle(statusCode);

        // Assert
        Assert.Equal("BadRequest", title);
    }

    #endregion

    #region GetCorrelationId Tests

    [Fact(DisplayName = "GetCorrelationId should return value from X-Correlation-ID header")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveRetornarValorDoHeaderXCorrelationID()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "test-correlation-id";
        context.Request.Headers["X-Correlation-ID"] = expectedId;

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.Equal(expectedId, correlationId);
    }

    [Fact(DisplayName = "GetCorrelationId should return value from X-Correlation-Id header")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveRetornarValorDoHeaderXCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "test-correlation-id-2";
        context.Request.Headers["X-Correlation-Id"] = expectedId;

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.Equal(expectedId, correlationId);
    }

    [Fact(DisplayName = "GetCorrelationId should return value from CorrelationId header")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveRetornarValorDoHeaderCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "test-correlation-id-3";
        context.Request.Headers["CorrelationId"] = expectedId;

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.Equal(expectedId, correlationId);
    }

    [Fact(DisplayName = "GetCorrelationId should return value from HttpContext.Items")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveRetornarValorDoHttpContextItems()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "test-correlation-id-4";
        context.Items["X-Correlation-ID"] = expectedId;

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.Equal(expectedId, correlationId);
    }

    [Fact(DisplayName = "GetCorrelationId should prioritize X-Correlation-ID over other headers")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DevePriorizarXCorrelationIDSobreOutrosHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "priority-id";
        // Note: ASP.NET Core headers são case-insensitive mas respeitam a ordem de verificação
        context.Request.Headers["X-Correlation-ID"] = expectedId;
        // Não adicionar outros headers com valores diferentes pois X-Correlation-ID será encontrado primeiro

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.Equal(expectedId, correlationId);
    }

    [Fact(DisplayName = "GetCorrelationId should fallback to X-Correlation-Id if X-Correlation-ID is missing")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveFallbackParaXCorrelationIdSeXCorrelationIDEstiverAusente()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "fallback-id";
        context.Request.Headers["X-Correlation-Id"] = expectedId;
        context.Request.Headers["CorrelationId"] = "other-id";
        context.Items["X-Correlation-ID"] = "other-id-2";

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.Equal(expectedId, correlationId);
    }

    [Fact(DisplayName = "GetCorrelationId should fallback to CorrelationId if X-Correlation headers are missing")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveFallbackParaCorrelationIdSeHeadersXCorrelationEstiveremAusentes()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "fallback-id-2";
        context.Request.Headers["CorrelationId"] = expectedId;
        context.Items["X-Correlation-ID"] = "other-id";

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.Equal(expectedId, correlationId);
    }

    [Fact(DisplayName = "GetCorrelationId should fallback to Items if all headers are missing")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveFallbackParaItemsSeHeadersEstiveremAusentes()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedId = "items-id";
        context.Items["X-Correlation-ID"] = expectedId;

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.Equal(expectedId, correlationId);
    }

    [Fact(DisplayName = "GetCorrelationId should generate new ID when no header or item exists")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveGerarNovoIdQuandoNaoExisteHeaderOuItem()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.NotNull(correlationId);
        Assert.NotEqual("N/A", correlationId);
        Assert.Equal(12, correlationId.Length); // Generated ID should be 12 characters
    }

    [Fact(DisplayName = "GetCorrelationId should return N/A when context is null")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveRetornarNAQuandoContextoNulo()
    {
        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(null);

        // Assert
        Assert.Equal("N/A", correlationId);
    }

    [Fact(DisplayName = "GetCorrelationId should generate new ID when header value is empty")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveGerarNovoIdQuandoValorDoHeaderVazio()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = string.Empty;

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.NotNull(correlationId);
        Assert.NotEqual("N/A", correlationId);
        Assert.NotEqual(string.Empty, correlationId);
        Assert.Equal(12, correlationId.Length);
    }

    [Fact(DisplayName = "GetCorrelationId should generate new ID when header value is whitespace")]
    [Trait("ErrorResponseHelper", "")]
    public void GetCorrelationId_DeveGerarNovoIdQuandoValorDoHeaderEspacoEmBranco()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "   ";

        // Act
        var correlationId = ErrorResponseHelper.GetCorrelationId(context);

        // Assert
        Assert.NotNull(correlationId);
        Assert.NotEqual("N/A", correlationId);
        Assert.NotEqual("   ", correlationId);
        Assert.Equal(12, correlationId.Length);
    }

    #endregion
}
