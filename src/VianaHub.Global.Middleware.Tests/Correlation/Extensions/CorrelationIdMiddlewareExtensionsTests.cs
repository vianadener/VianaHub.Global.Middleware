using EBL.FIG.Common.Middleware.Lib.Correlation;
using EBL.FIG.Common.Middleware.Lib.Correlation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.Correlation.Extensions;

public class CorrelationIdMiddlewareExtensionsTests
{
    #region Sucesso

    [Fact(DisplayName = "UseCorrelationId should return IApplicationBuilder")]
    [Trait("Correlation", "")]
    public void UseCorrelationId_DeveRetornarIApplicationBuilder()
    {
        // Arrange
        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);

        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(new ServiceCollection().BuildServiceProvider());

        // Act
        var result = appMock.Object.UseCorrelationId();

        // Assert
        Assert.NotNull(result);
    }

    [Fact(DisplayName = "UseCorrelationId should register middleware in the pipeline")]
    [Trait("Correlation", "")]
    public void UseCorrelationId_DeveRegistrarMiddlewareNoPipeline()
    {
        // Arrange
        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);

        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(new ServiceCollection().BuildServiceProvider());

        // Act
        appMock.Object.UseCorrelationId();

        // Assert
        appMock.Verify(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    [Fact(DisplayName = "UseCorrelationId should return the same IApplicationBuilder instance")]
    [Trait("Correlation", "")]
    public void UseCorrelationId_DeveRetornarMesmaInstanciaIApplicationBuilder()
    {
        // Arrange
        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);

        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(new ServiceCollection().BuildServiceProvider());

        // Act
        var result = appMock.Object.UseCorrelationId();

        // Assert
        Assert.Same(appMock.Object, result);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "UseCorrelationId called multiple times should register middleware multiple times")]
    [Trait("Correlation", "")]
    public void UseCorrelationId_ChamadoMultiplasVezes_DeveRegistrarMiddlewareMultiplasVezes()
    {
        // Arrange
        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);

        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(new ServiceCollection().BuildServiceProvider());

        // Act
        appMock.Object.UseCorrelationId();
        appMock.Object.UseCorrelationId();

        // Assert
        appMock.Verify(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Exactly(2));
    }

    #endregion
}
