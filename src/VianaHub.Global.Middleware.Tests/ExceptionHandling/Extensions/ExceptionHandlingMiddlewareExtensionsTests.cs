using EBL.FIG.Common.Middleware.Lib.ExceptionHandling.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling.Extensions;

public class ExceptionHandlingMiddlewareExtensionsTests
{
    private static Mock<IApplicationBuilder> CreateAppBuilderMock()
    {
        var mock = new Mock<IApplicationBuilder>();
        mock.Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(mock.Object);
        mock.Setup(a => a.ApplicationServices)
            .Returns(new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider());
        mock.Setup(a => a.New()).Returns(mock.Object);
        return mock;
    }

    #region Sucesso

    [Fact(DisplayName = "UseExceptionHandling - Should call Use on IApplicationBuilder to register middleware")]
    [Trait("ExceptionHandling", "")]
    public void UseExceptionHandling_DeveChamarUseNoIApplicationBuilder()
    {
        // Arrange
        var appMock = CreateAppBuilderMock();

        // Act
        appMock.Object.UseExceptionHandling();

        // Assert
        appMock.Verify(
            a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()),
            Times.Once);
    }

    [Fact(DisplayName = "UseExceptionHandling - Should return IApplicationBuilder instance")]
    [Trait("ExceptionHandling", "")]
    public void UseExceptionHandling_DeveRetornarIApplicationBuilder()
    {
        // Arrange
        var appMock = CreateAppBuilderMock();

        // Act
        var result = appMock.Object.UseExceptionHandling();

        // Assert
        Assert.NotNull(result);
    }

    [Fact(DisplayName = "UseExceptionHandling - Return value should be same IApplicationBuilder instance")]
    [Trait("ExceptionHandling", "")]
    public void UseExceptionHandling_RetornoDeveSerMesmaInstanciaDeIApplicationBuilder()
    {
        // Arrange
        var appMock = CreateAppBuilderMock();

        // Act
        var result = appMock.Object.UseExceptionHandling();

        // Assert
        Assert.Same(appMock.Object, result);
    }

    [Fact(DisplayName = "UseExceptionHandling - Chaining two calls should call Use twice")]
    [Trait("ExceptionHandling", "")]
    public void UseExceptionHandling_EncadeamentoDuasChamadas_DeveChamarUseDuasVezes()
    {
        // Arrange
        var appMock = CreateAppBuilderMock();

        // Act
        appMock.Object.UseExceptionHandling().UseExceptionHandling();

        // Assert
        appMock.Verify(
            a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()),
            Times.Exactly(2));
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "UseExceptionHandling - Null IApplicationBuilder should throw NullReferenceException")]
    [Trait("ExceptionHandling", "")]
    public void UseExceptionHandling_IApplicationBuilderNulo_DeveLancarNullReferenceException()
    {
        // Arrange
        IApplicationBuilder? builder = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => builder!.UseExceptionHandling());
    }

    #endregion
}
