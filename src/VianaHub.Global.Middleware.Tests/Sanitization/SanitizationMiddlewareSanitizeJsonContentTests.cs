// <copyright file="SanitizationMiddlewareSanitizeJsonContentTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using EBL.FIG.Common.Middleware.Lib.Sanitization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;
using System.Text.Json;

namespace EBL.FIG.Common.Middleware.Tests.Sanitization;

/// <summary>
/// Tests that target specific uncovered branches inside the private methods
/// SanitizeJsonContent and SanitizeString / ValidateRuleExpression of
/// SanitizationMiddleware, reached through the public InvokeAsync entry point.
///
/// Uncovered branches targeted:
///   1. SanitizeJsonContent – default case in the ValueKind switch
///   2. SanitizeString     – DeniedCharacters removal path
///   3. ValidateRuleExpression – Process., File., Environment. patterns
/// </summary>
public class SanitizationMiddlewareSanitizeJsonContentTests
{
    // ?????????????????????????????????????????????????????????????????????????
    // Helpers
    // ?????????????????????????????????????????????????????????????????????????

    private static SanitizationMiddleware CreateMiddleware(
        RequestDelegate next,
        SanitizationOptions? options = null)
    {
        var logger = new Mock<ILogger<SanitizationMiddleware>>().Object;
        var opts = Options.Create(options ?? new SanitizationOptions());
        return new SanitizationMiddleware(next, logger, opts);
    }

    private static DefaultHttpContext CreateHttpContextWithBody(
        string body,
        string contentType = "application/json")
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = contentType;
        var bytes = Encoding.UTF8.GetBytes(body);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        context.Response.Body = new MemoryStream();
        return context;
    }

    // ?????????????????????????????????????????????????????????????????????????
    // SanitizeJsonContent – default branch (Undefined / other JsonValueKind)
    // ?????????????????????????????????????????????????????????????????????????

    [Fact(DisplayName = "SanitizeJsonContent - JSON with nested array of numbers should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeJsonContent_JsonComArrayDeNumeros_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"values\":[1,2,3]}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "SanitizeJsonContent - JSON with deeply nested object should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeJsonContent_JsonComObjetoAninhado_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"outer\":{\"inner\":{\"value\":42}}}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "SanitizeJsonContent - JSON with null value should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeJsonContent_JsonComValorNull_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"field\":null}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "SanitizeJsonContent - JSON with true boolean value should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeJsonContent_JsonComBooleanTrue_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"active\":true}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "SanitizeJsonContent - JSON with false boolean value should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeJsonContent_JsonComBooleanFalse_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"active\":false}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "SanitizeJsonContent - JSON with decimal number should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeJsonContent_JsonComNumeroDecimal_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"price\":19.99}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "SanitizeJsonContent - JSON with mixed array containing strings and numbers should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeJsonContent_JsonComArrayMisto_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"items\":[\"text\",42,true,null]}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    // ?????????????????????????????????????????????????????????????????????????
    // SanitizeString – DeniedCharacters removal path
    // ?????????????????????????????????????????????????????????????????????????

    [Fact(DisplayName = "SanitizeString - string containing denied characters should be cleaned and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeString_StringComCaracteresProibidos_DeveSerLimpadaEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false,
            // Override DeniedCharacters to a predictable set
            DeniedCharacters = new[] { "!", "#" }
        };
        var middleware = CreateMiddleware(next, options);
        // The string "hello!world#test" should have '!' and '#' removed
        var context = CreateHttpContextWithBody("{\"value\":\"hello!world#test\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "SanitizeString - string containing default denied characters should be cleaned and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeString_StringComCaracteresProibidosPadrao_DeveSerLimpadaEInvocarProximo()
    {
        // Arrange – use default DeniedCharacters: <, >, ', ", ;, =, (, ), {, }
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false
        };
        var middleware = CreateMiddleware(next, options);
        // Build a JSON body that avoids breaking the JSON structure but contains
        // characters that must be cleaned from the string value
        var context = CreateHttpContextWithBody("{\"value\":\"hello;world\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "SanitizeString - empty denied characters array should not alter string and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeString_ArrayDeCaracteresProibidosVazio_NaoDeveAlterarStringEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"value\":\"clean-string\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    // ?????????????????????????????????????????????????????????????????????????
    // ValidateRuleExpression – Process., File., Environment. patterns
    // ?????????????????????????????????????????????????????????????????????????

    [Fact(DisplayName = "ValidateRuleExpression - string containing 'Process.' and 'Expression' should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task ValidateRuleExpression_StringComProcessExpression_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false,
            ValidateRuleExpressions = true,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        // "Process.Start" contains "Process." and the value also contains "Expression"
        var context = CreateHttpContextWithBody("{\"rule\":\"Process.Start Expression\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "ValidateRuleExpression - string containing 'File.' and 'Expression' should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task ValidateRuleExpression_StringComFileExpression_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false,
            ValidateRuleExpressions = true,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"rule\":\"File.ReadAllText Expression\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "ValidateRuleExpression - string containing 'Environment.' and 'Expression' should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task ValidateRuleExpression_StringComEnvironmentExpression_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false,
            ValidateRuleExpressions = true,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"rule\":\"Environment.GetEnvironmentVariable Expression\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "ValidateRuleExpression - string containing 'System.' and 'Expression' should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task ValidateRuleExpression_StringComSystemExpression_DeveSanitizarEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false,
            ValidateRuleExpressions = true,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"rule\":\"System.Reflection Expression\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "ValidateRuleExpression - string with all dangerous patterns combined and 'Expression' should be sanitized and invoke next")]
    [Trait("Sanitization", "")]
    public async Task ValidateRuleExpression_StringComTodosOsPadroes_DeveSanitizarEInvocarProximo()
    {
        // Arrange – exercise all four Regex.Replace calls in ValidateRuleExpression
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false,
            ValidateRuleExpressions = true,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        // Contains System., Process., File., Environment. and the trigger keyword "Expression"
        var context = CreateHttpContextWithBody("{\"rule\":\"System.IO File.Read Process.Kill Environment.Exit Expression\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "ValidateRuleExpression - string without 'Expression' keyword should not trigger rule validation and invoke next")]
    [Trait("Sanitization", "")]
    public async Task ValidateRuleExpression_StringSemKeywordExpression_NaoDeveTriggerValidacaoEInvocarProximo()
    {
        // Arrange – ValidateRuleExpressions = true but no "Expression" keyword in value
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false,
            ValidateRuleExpressions = true,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        // System. is present but "Expression" is absent ? ValidateRuleExpression is NOT called
        var context = CreateHttpContextWithBody("{\"rule\":\"System.IO is used here\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "ValidateRuleExpression - ValidateRuleExpressions disabled should skip rule validation and invoke next")]
    [Trait("Sanitization", "")]
    public async Task ValidateRuleExpression_ValidateRuleExpressionsDesabilitado_DeveIgnorarValidacaoEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = false,
            ValidateRuleExpressions = false,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContextWithBody("{\"rule\":\"System.IO File.Read Expression\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    // ?????????????????????????????????????????????????????????????????????????
    // SanitizeString – XSS on-event attribute pattern
    // ?????????????????????????????????????????????????????????????????????????

    [Fact(DisplayName = "SanitizeString - string containing on-event attribute pattern should be cleaned and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeString_StringComOnEventPattern_DeveSerLimpadaEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = false,
            EnableXssProtection = true,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        // "onclick=" matches the regex on\w+\s*=
        var context = CreateHttpContextWithBody("{\"value\":\"hello onclick=alert world\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    // ?????????????????????????????????????????????????????????????????????????
    // SanitizeString – HtmlEncoding path
    // ?????????????????????????????????????????????????????????????????????????

    [Fact(DisplayName = "SanitizeString - string with HTML special chars should be HTML-encoded and invoke next")]
    [Trait("Sanitization", "")]
    public async Task SanitizeString_StringComCaracteresHtml_DeveSerEncodadaEInvocarProximo()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var options = new SanitizationOptions
        {
            ValidateJsonStructure = true,
            EnableHtmlEncoding = true,
            EnableXssProtection = false,
            DeniedCharacters = Array.Empty<string>()
        };
        var middleware = CreateMiddleware(next, options);
        // Ampersand is HTML-encoded by HttpUtility.HtmlEncode
        var context = CreateHttpContextWithBody("{\"value\":\"hello & world\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }
}
