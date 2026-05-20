// <copyright file="SanitizationMiddlewareAdditionalTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>

using EBL.FIG.Common.Middleware.Lib.Sanitization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace EBL.FIG.Common.Middleware.Tests.Sanitization;

/// <summary>
/// Testes adicionais para aumentar a cobertura do SanitizationMiddleware.
/// </summary>
public class SanitizationMiddlewareAdditionalTests
{
    private static SanitizationMiddleware CreateMiddleware(
        RequestDelegate next,
        SanitizationOptions? options = null)
    {
        var logger = new Mock<ILogger<SanitizationMiddleware>>().Object;
        var opts = Options.Create(options ?? new SanitizationOptions());
        return new SanitizationMiddleware(next, logger, opts);
    }

    private static DefaultHttpContext CreateHttpContext(
        string? contentType = null,
        string? body = null,
        long? contentLength = null)
    {
        var context = new DefaultHttpContext();

        if (contentType != null)
            context.Request.ContentType = contentType;

        if (body != null)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            context.Request.Body = new MemoryStream(bytes);
            context.Request.ContentLength = contentLength ?? bytes.Length;
        }

        context.Response.Body = new MemoryStream();
        return context;
    }

    #region Casos de sanitização de strings

    [Fact(DisplayName = "String containing HTML tags should be encoded when EnableHtmlEncoding is true")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComHtml_DeveSerCodificadaQuandoHtmlEncodingAtivado()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions { EnableHtmlEncoding = true, EnableXssProtection = false };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"html\":\"<div>Test</div>\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing 'javascript:' should be removed when EnableXssProtection is true")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComJavascript_DeveSerRemovidaQuandoXssProtectionAtivado()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions { EnableXssProtection = true, EnableHtmlEncoding = false };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"link\":\"javascript:alert('xss')\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing 'onclick' event handler should be removed when EnableXssProtection is true")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComEventHandler_DeveSerRemovidaQuandoXssProtectionAtivado()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions { EnableXssProtection = true, EnableHtmlEncoding = false };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"tag\":\"<div onclick='alert()'>Test</div>\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing denied characters should have them removed")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComCaracteresNegados_DevemSerRemovidos()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions
        {
            DeniedCharacters = new[] { "@", "#", "$" },
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"email\":\"test@example#$.com\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Empty string should pass through sanitization")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringVazia_DevePassarPelaSanitizacao()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"value\":\"\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing 'Expression' should be validated when ValidateRuleExpressions is true")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComExpression_DeveSerValidadaQuandoValidateRuleExpressionsAtivado()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions
        {
            ValidateRuleExpressions = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"rule\":\"Expression with System.IO.File access\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing 'System.' references should be removed when ValidateRuleExpressions is true")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComSystemReferences_DevemSerRemovidasQuandoValidateRuleExpressionsAtivado()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions
        {
            ValidateRuleExpressions = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"code\":\"Expression calling System.Environment.Exit()\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing 'Process.' references should be removed when ValidateRuleExpressions is true")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComProcessReferences_DevemSerRemovidasQuandoValidateRuleExpressionsAtivado()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions
        {
            ValidateRuleExpressions = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"cmd\":\"Expression using Process.Start()\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing 'File.' references should be removed when ValidateRuleExpressions is true")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComFileReferences_DevemSerRemovidasQuandoValidateRuleExpressionsAtivado()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions
        {
            ValidateRuleExpressions = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"path\":\"Expression reading File.ReadAllText()\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    #endregion

    #region Casos de ValueKind do JSON

    [Fact(DisplayName = "JSON with number values should be sanitized correctly")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComNumeros_DeveSanitizarCorretamente()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"age\":30,\"price\":99.99,\"count\":0}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "JSON with mixed types in array should be sanitized correctly")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComArrayMisto_DeveSanitizarCorretamente()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"mixed\":[\"text\",123,true,null,{\"nested\":\"value\"}]}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "JSON with deep nesting should be sanitized correctly")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComAninhamentoProfundo_DeveSanitizarCorretamente()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"level1\":{\"level2\":{\"level3\":{\"level4\":\"deep\"}}}}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "JSON with array of arrays should be sanitized correctly")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComArrayDeArrays_DeveSanitizarCorretamente()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"matrix\":[[1,2],[3,4],[5,6]]}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "JSON with undefined value kind should be handled")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComValorTipoUndefined_DeveSerTratado()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"test\":\"value\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    #endregion

    #region Casos de Content-Type

    [Fact(DisplayName = "Request with application/json; charset=utf-8 should be accepted")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentTypeComCharset_DeveSerAceito()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(
            contentType: "application/json; charset=utf-8",
            body: "{\"test\":\"value\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Request with APPLICATION/JSON in uppercase should be accepted")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentTypeEmMaiusculas_DeveSerAceito()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(
            contentType: "APPLICATION/JSON",
            body: "{\"test\":\"value\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Request with application/xml content type should return 415")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentTypeXml_DeveRetornar415()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext(contentType: "application/xml");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, context.Response.StatusCode);
    }

    #endregion

    #region Casos de ContentLength

    [Fact(DisplayName = "Request with ContentLength of 0 should skip body processing")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentLengthZero_DevePularProcessamentoDoBody()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(contentType: "application/json");
        context.Request.ContentLength = 0;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Request with null ContentLength but empty body should skip body processing")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentLengthNullComBodyVazio_DevePularProcessamentoDoBody()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext(contentType: "application/json");
        context.Request.ContentLength = null;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    #endregion

    #region Casos especiais de property names

    [Fact(DisplayName = "JSON with special characters in property names should be sanitized")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComCaracteresEspeciaisNasPropriedades_DeveSanitizar()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions
        {
            EnableXssProtection = true,
            EnableHtmlEncoding = true
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"user<script>\":\"value\",\"data\":\"test\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    #endregion
}
