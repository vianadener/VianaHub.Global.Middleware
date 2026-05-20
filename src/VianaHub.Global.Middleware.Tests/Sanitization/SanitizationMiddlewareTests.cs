using EBL.FIG.Common.Middleware.Lib.Sanitization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace EBL.FIG.Common.Middleware.Tests.Sanitization;

public class SanitizationMiddlewareTests
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

    #region Sucesso

    [Fact(DisplayName = "Request with no content type should invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_SemContentType_DevePassarParaProximoMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Request with application/json content type should invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentTypeApplicationJson_DevePassarParaProximoMiddleware()
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

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Valid JSON body should be sanitized and invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonValido_DeveSanitizarEPassarParaProximoMiddleware()
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
            body: "{\"name\":\"John\",\"age\":30}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Request with no body should invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_SemBody_DevePassarParaProximoMiddleware()
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

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Request within size limit should invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_TamanhoDentroDoLimite_DevePassarParaProximoMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions { MaxRequestSize = 1048576 };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"value\":\"small\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "JSON with nested object should be sanitized and invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComObjetoAninhado_DeveSanitizarEPassarParaProximoMiddleware()
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
            body: "{\"user\":{\"name\":\"Alice\",\"active\":true}}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "JSON with array should be sanitized and invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComArray_DeveSanitizarEPassarParaProximoMiddleware()
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
            body: "{\"items\":[\"a\",\"b\",\"c\"]}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "JSON with boolean fields should be sanitized and invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComBoolean_DeveSanitizarEPassarParaProximoMiddleware()
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
            body: "{\"enabled\":true,\"disabled\":false}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "JSON with null field should be sanitized and invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonComNull_DeveSanitizarEPassarParaProximoMiddleware()
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
            body: "{\"value\":null}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String value exceeding MaxStringLength should be truncated")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringAcimaDoTamanhoMaximo_DeveSerTruncada()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions { MaxStringLength = 5, EnableHtmlEncoding = false, EnableXssProtection = false };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"name\":\"ABCDEFGHIJ\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "ValidateJsonStructure disabled should bypass JSON parsing and invoke next")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ValidateJsonStructureDesabilitado_DevePassarParaProximoMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions { ValidateJsonStructure = false };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "not a json at all");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing XSS script tag should be cleaned when XSS protection is enabled")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComXssScriptTag_DeveSerLimpadaComXssProtectionAtivada()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions { EnableHtmlEncoding = false, EnableXssProtection = true };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"value\":\"hello<script>alert(1)</script>world\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing javascript: URI should be cleaned when XSS protection is enabled")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComJavascriptUri_DeveSerLimpadaComXssProtectionAtivada()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions { EnableHtmlEncoding = false, EnableXssProtection = true };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"url\":\"javascript:void(0)\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "String containing System. expression should be sanitized when ValidateRuleExpressions is enabled")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_StringComSystemExpression_DeveSerSanitizadaComValidateRuleExpressionsAtivado()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var options = new SanitizationOptions { EnableHtmlEncoding = false, EnableXssProtection = false, ValidateRuleExpressions = true };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{\"rule\":\"System.IO.File.ReadAllText Expression\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Request with application/json charset variant should invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentTypeApplicationJsonComCharset_DevePassarParaProximoMiddleware()
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
            body: "{\"key\":\"value\"}");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "Request with unsupported content type should return 415")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentTypeNaoSuportado_DeveRetornar415()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext(contentType: "text/plain");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Request with unsupported content type should not invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentTypeNaoSuportado_NaoDeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext(contentType: "text/plain");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact(DisplayName = "Request with multipart/form-data content type should return 415")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentTypeMultipart_DeveRetornar415()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext(contentType: "multipart/form-data");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Request exceeding MaxRequestSize should return 413")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_TamanhoAcimaDoMaximo_DeveRetornar413()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var options = new SanitizationOptions { MaxRequestSize = 10 };
        var middleware = CreateMiddleware(nextMock.Object, options);
        var context = CreateHttpContext(contentType: "application/json");
        context.Request.ContentLength = 100; // explicitly exceed the limit

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Request exceeding MaxRequestSize should not invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_TamanhoAcimaDoMaximo_NaoDeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var options = new SanitizationOptions { MaxRequestSize = 10 };
        var middleware = CreateMiddleware(nextMock.Object, options);
        var context = CreateHttpContext(contentType: "application/json");
        context.Request.ContentLength = 100; // explicitly exceed the limit

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact(DisplayName = "Invalid JSON body should return 400 when ValidateJsonStructure is enabled")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonInvalido_DeveRetornar400()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var options = new SanitizationOptions { ValidateJsonStructure = true };
        var middleware = CreateMiddleware(nextMock.Object, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{ this is not valid json }");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Invalid JSON body should not invoke next middleware")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_JsonInvalido_NaoDeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var options = new SanitizationOptions { ValidateJsonStructure = true };
        var middleware = CreateMiddleware(nextMock.Object, options);
        var context = CreateHttpContext(
            contentType: "application/json",
            body: "{ this is not valid json }");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact(DisplayName = "Exception thrown by next middleware should return 500")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ProximoMiddlewareLancaExcecao_DeveRetornar500()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Pipeline failure");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Exception thrown by next middleware should not propagate to caller")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ProximoMiddlewareLancaExcecao_NaoDevePropagarExcecao()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Pipeline failure");
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Null(exception);
    }

    [Fact(DisplayName = "Exception thrown by next middleware should log the error")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ProximoMiddlewareLancaExcecao_DeveRegistrarErroNoLog()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SanitizationMiddleware>>();
        var options = Options.Create(new SanitizationOptions());
        RequestDelegate next = _ => throw new InvalidOperationException("Pipeline failure");
        var middleware = new SanitizationMiddleware(next, loggerMock.Object, options);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "Request with text/html content type should return 415")]
    [Trait("Sanitization", "")]
    public async Task InvokeAsync_ContentTypeTextHtml_DeveRetornar415()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext(contentType: "text/html");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, context.Response.StatusCode);
    }

    #endregion
}
