using EBL.FIG.Common.Middleware.Lib.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace EBL.FIG.Common.Middleware.Tests.Authentication;

public class BasicAuthenticationMiddlewareTests
{
    private const string ValidUsername = "testuser";
    private const string ValidPassword = "testpass";

    private static BasicAuthenticationMiddleware CreateMiddleware(RequestDelegate next)
        => new(next, ValidUsername, ValidPassword);

    private static DefaultHttpContext CreateHttpContext() => new();

    private static string EncodeCredentials(string username, string password)
    {
        var credentials = $"{username}:{password}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
    }

    #region Sucesso

    [Fact(DisplayName = "Valid credentials should invoke next middleware and return 200")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_CredenciaisValidas_DevePassarParaProximoMiddleware()
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
        var encoded = EncodeCredentials(ValidUsername, ValidPassword);
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Valid credentials should not set status 401")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_CredenciaisValidas_NaoDeveDefinirStatus401()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials(ValidUsername, ValidPassword);
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Valid credentials should not add WWW-Authenticate header")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_CredenciaisValidas_NaoDeveAdicionarHeaderWWWAuthenticate()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials(ValidUsername, ValidPassword);
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Response.Headers.ContainsKey("WWW-Authenticate"));
    }

    [Fact(DisplayName = "Valid credentials with colon in password should invoke next middleware")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_CredenciaisValidasComSenhaComDoisPontos_DevePassarParaProximoMiddleware()
    {
        // Arrange
        const string passwordWithColon = "pass:word:complex";
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new BasicAuthenticationMiddleware(next, ValidUsername, passwordWithColon);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials(ValidUsername, passwordWithColon);
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "Missing Authorization header should return 401")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_SemHeaderAuthorization_DeveRetornar401()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Missing Authorization header should not invoke next middleware")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_SemHeaderAuthorization_NaoDeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact(DisplayName = "Missing Authorization header should add WWW-Authenticate header")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_SemHeaderAuthorization_DeveAdicionarHeaderWWWAuthenticate()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("WWW-Authenticate"));
        Assert.Equal("Basic", context.Response.Headers["WWW-Authenticate"].ToString());
    }

    [Fact(DisplayName = "Invalid authentication scheme should return 401")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_EsquemaAutenticacaoInvalido_DeveRetornar401()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials(ValidUsername, ValidPassword);
        context.Request.Headers["Authorization"] = $"Bearer {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Invalid authentication scheme should not invoke next middleware")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_EsquemaAutenticacaoInvalido_NaoDeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials(ValidUsername, ValidPassword);
        context.Request.Headers["Authorization"] = $"Bearer {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact(DisplayName = "Wrong username should return 401")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_UsernameIncorreto_DeveRetornar401()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials("wronguser", ValidPassword);
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Wrong username should not invoke next middleware")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_UsernameIncorreto_NaoDeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials("wronguser", ValidPassword);
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact(DisplayName = "Wrong password should return 401")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_PasswordIncorreta_DeveRetornar401()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials(ValidUsername, "wrongpass");
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact(DisplayName = "Wrong password should not invoke next middleware")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_PasswordIncorreta_NaoDeveInvocarProximoMiddleware()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials(ValidUsername, "wrongpass");
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact(DisplayName = "Wrong password should add WWW-Authenticate header")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_PasswordIncorreta_DeveAdicionarHeaderWWWAuthenticate()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials(ValidUsername, "wrongpass");
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("WWW-Authenticate"));
        Assert.Equal("Basic", context.Response.Headers["WWW-Authenticate"].ToString());
    }

    [Fact(DisplayName = "Empty credentials should return 401")]
    [Trait("Authentication", "")]
    public async Task InvokeAsync_CredenciaisVazias_DeveRetornar401()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();
        var encoded = EncodeCredentials(string.Empty, string.Empty);
        context.Request.Headers["Authorization"] = $"Basic {encoded}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    #endregion
}
