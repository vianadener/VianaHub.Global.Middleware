using EBL.FIG.Common.Middleware.Lib.Correlation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.Correlation;

public class CorrelationIdMiddlewareTests
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    private static CorrelationIdMiddleware CreateMiddleware(RequestDelegate next)
        => new(next);

    private static DefaultHttpContext CreateHttpContext() => new();

    /// <summary>
    /// Creates an HttpContext whose IHttpResponseFeature manually fires all
    /// OnStarting callbacks when FireOnStartingAsync() is called, allowing
    /// tests to assert on response headers set inside OnStarting.
    /// </summary>
    private static (HttpContext context, Func<Task> fireOnStarting) CreateHttpContextWithResponseCallbacks()
    {
        var callbacks = new List<(Func<object, Task> callback, object state)>();

        var responseFeatureMock = new Mock<IHttpResponseFeature>();
        responseFeatureMock
            .Setup(f => f.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((cb, state) => callbacks.Add((cb, state)));

        var headers = new HeaderDictionary();
        responseFeatureMock.SetupGet(f => f.Headers).Returns(headers);
        responseFeatureMock.SetupGet(f => f.StatusCode).Returns(200);
        responseFeatureMock.SetupGet(f => f.HasStarted).Returns(false);

        var context = new DefaultHttpContext();
        context.Features.Set(responseFeatureMock.Object);

        // Expose a delegate that fires all registered OnStarting callbacks in order
        async Task FireOnStartingAsync()
        {
            foreach (var (cb, state) in callbacks)
                await cb(state);
        }

        return (context, FireOnStartingAsync);
    }

    #region Sucesso

    [Fact(DisplayName = "Request without correlation ID header should generate a new GUID")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_SemHeaderCorrelationId_DeveGerarNovoGuid()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Items.ContainsKey(CorrelationIdHeader));
        var correlationId = context.Items[CorrelationIdHeader]?.ToString();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact(DisplayName = "Request with existing correlation ID header should reuse it")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_ComHeaderCorrelationIdExistente_DeveReutilizarValor()
    {
        // Arrange
        var existingCorrelationId = Guid.NewGuid().ToString();
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContext();
        context.Request.Headers[CorrelationIdHeader] = existingCorrelationId;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var storedCorrelationId = context.Items[CorrelationIdHeader]?.ToString();
        Assert.Equal(existingCorrelationId, storedCorrelationId);
    }

    [Fact(DisplayName = "Correlation ID should be stored in HttpContext Items")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_DeveArmazenarCorrelationIdEmContextItems()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Items.ContainsKey(CorrelationIdHeader));
        Assert.NotNull(context.Items[CorrelationIdHeader]);
    }

    [Fact(DisplayName = "Should invoke next middleware in pipeline")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_DeveInvocarProximoMiddlewareNoPipeline()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        nextMock
            .Setup(n => n(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Once);
    }

    [Fact(DisplayName = "Correlation ID in Items should match the value from the request header")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_CorrelationIdEmItemsDeveCorresponderAoHeaderDaRequisicao()
    {
        // Arrange
        var expectedId = Guid.NewGuid().ToString();
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContext();
        context.Request.Headers[CorrelationIdHeader] = expectedId;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var itemsValue = context.Items[CorrelationIdHeader]?.ToString();
        Assert.Equal(expectedId, itemsValue);
    }

    [Fact(DisplayName = "Two requests without correlation ID header should generate distinct IDs")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_DuasRequisicoesSemHeaderCorrelationId_DevemGerarIdsDistintos()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context1 = CreateHttpContext();
        var context2 = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(context2);

        // Assert
        var id1 = context1.Items[CorrelationIdHeader]?.ToString();
        var id2 = context2.Items[CorrelationIdHeader]?.ToString();
        Assert.NotEqual(id1, id2);
    }

    [Fact(DisplayName = "Response header should contain correlation ID from request after response starts")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_OnStarting_DeveAdicionarCorrelationIdDoRequestNoHeaderDaResposta()
    {
        // Arrange
        var expectedId = Guid.NewGuid().ToString();
        var (context, fireOnStarting) = CreateHttpContextWithResponseCallbacks();
        context.Request.Headers[CorrelationIdHeader] = expectedId;

        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);
        await fireOnStarting(); // simulate ASP.NET Core triggering OnStarting before headers are flushed

        // Assert
        var responseHeaderValue = context.Response.Headers[CorrelationIdHeader].ToString();
        Assert.Equal(expectedId, responseHeaderValue);
    }

    [Fact(DisplayName = "Response header should contain generated GUID when request has no correlation ID header after response starts")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_OnStarting_SemHeaderNaRequisicao_DeveAdicionarGuidGeradoNoHeaderDaResposta()
    {
        // Arrange
        var (context, fireOnStarting) = CreateHttpContextWithResponseCallbacks();

        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);
        await fireOnStarting();

        // Assert
        var responseHeaderValue = context.Response.Headers[CorrelationIdHeader].ToString();
        Assert.False(string.IsNullOrWhiteSpace(responseHeaderValue));
        Assert.True(Guid.TryParse(responseHeaderValue, out _));
    }

    [Fact(DisplayName = "Response header correlation ID should match the value stored in HttpContext Items after response starts")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_OnStarting_HeaderDaRespostaDeveCorresponderAoItemDoContexto()
    {
        // Arrange
        var (context, fireOnStarting) = CreateHttpContextWithResponseCallbacks();

        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);
        await fireOnStarting();

        // Assert
        var itemsValue = context.Items[CorrelationIdHeader]?.ToString();
        var responseHeaderValue = context.Response.Headers[CorrelationIdHeader].ToString();
        Assert.Equal(itemsValue, responseHeaderValue);
    }

    [Fact(DisplayName = "Response header should not be set before OnStarting callback fires")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_OnStarting_HeaderDaRespostaNaoDeveEstarDefinidoAntesDoCallbackDisparar()
    {
        // Arrange
        var (context, _) = CreateHttpContextWithResponseCallbacks();

        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Act — intentionally do NOT call fireOnStarting
        await middleware.InvokeAsync(context);

        // Assert: header must be absent because OnStarting has not fired yet
        Assert.False(context.Response.Headers.ContainsKey(CorrelationIdHeader));
    }

    [Fact(DisplayName = "OnStarting callback should be registered exactly once per request")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_OnStarting_CallbackDeveSerRegistradoExatamenteUmaVez()
    {
        // Arrange
        var registrationCount = 0;

        var responseFeatureMock = new Mock<IHttpResponseFeature>();
        responseFeatureMock
            .Setup(f => f.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((_, _) => registrationCount++);

        responseFeatureMock.SetupGet(f => f.Headers).Returns(new HeaderDictionary());
        responseFeatureMock.SetupGet(f => f.StatusCode).Returns(200);
        responseFeatureMock.SetupGet(f => f.HasStarted).Returns(false);

        var context = new DefaultHttpContext();
        context.Features.Set(responseFeatureMock.Object);

        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(1, registrationCount);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "Null HttpContext should throw ArgumentNullException")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_HttpContextNulo_DeveLancarArgumentNullException()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => middleware.InvokeAsync(null!));
    }

    [Fact(DisplayName = "Exception thrown by next middleware should propagate")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_ProximoMiddlewareLancaExcecao_DevePropagarExcecao()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Pipeline error");
        var nextMock = new Mock<RequestDelegate>();
        nextMock
            .Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);

        var middleware = CreateMiddleware(nextMock.Object);
        var context = CreateHttpContext();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));

        Assert.Equal("Pipeline error", ex.Message);
    }

    [Fact(DisplayName = "Empty string correlation ID header should not be reused as a valid GUID")]
    [Trait("Correlation", "")]
    public async Task InvokeAsync_HeaderCorrelationIdVazio_NaoDeveSerReutilizadoComoGuidValido()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreateHttpContext();
        context.Request.Headers[CorrelationIdHeader] = string.Empty;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var storedId = context.Items[CorrelationIdHeader]?.ToString();

        // An empty string is not a valid GUID — middleware should have stored the empty value as-is
        // (the middleware propagates whatever came in the header; validation is the caller's responsibility)
        Assert.False(Guid.TryParse(storedId, out _));
    }

    #endregion
}
