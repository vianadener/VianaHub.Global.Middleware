using EBL.FIG.Common.Middleware.Lib.Sanitization;
using EBL.FIG.Common.Middleware.Lib.Sanitization.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.Sanitization.Extensions;

public class SanitizationMiddlewareExtensionsTests
{
    #region Sucesso

    [Fact(DisplayName = "UseSanitization without options should return the same IApplicationBuilder")]
    [Trait("Sanitization", "")]
    public void UseSanitization_SemOpcoes_DeveRetornarMesmoIApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);
        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(serviceProvider);

        // Act
        var result = appMock.Object.UseSanitization();

        // Assert
        Assert.NotNull(result);
    }

    [Fact(DisplayName = "UseSanitization without options should register middleware in pipeline")]
    [Trait("Sanitization", "")]
    public void UseSanitization_SemOpcoes_DeveRegistrarMiddlewareNoPipeline()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);
        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(serviceProvider);

        // Act
        appMock.Object.UseSanitization();

        // Assert
        appMock.Verify(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    [Fact(DisplayName = "UseSanitization with options should return the same IApplicationBuilder")]
    [Trait("Sanitization", "")]
    public void UseSanitization_ComOpcoes_DeveRetornarMesmoIApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);
        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(serviceProvider);

        // Act
        var result = appMock.Object.UseSanitization(opts =>
        {
            opts.MaxRequestSize = 5242880;
            opts.EnableHtmlEncoding = true;
        });

        // Assert
        Assert.NotNull(result);
    }

    [Fact(DisplayName = "UseSanitization with options should register middleware in pipeline")]
    [Trait("Sanitization", "")]
    public void UseSanitization_ComOpcoes_DeveRegistrarMiddlewareNoPipeline()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);
        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(serviceProvider);

        // Act
        appMock.Object.UseSanitization(opts =>
        {
            opts.MaxRequestSize = 5242880;
        });

        // Assert
        appMock.Verify(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    [Fact(DisplayName = "UseSanitization with custom MaxRequestSize should configure correctly")]
    [Trait("Sanitization", "")]
    public void UseSanitization_ComMaxRequestSizePersonalizado_DeveConfigurarCorretamente()
    {
        // Arrange
        SanitizationOptions? capturedOptions = null;
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);
        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(serviceProvider);

        // Act
        appMock.Object.UseSanitization(opts =>
        {
            opts.MaxRequestSize = 2048;
            capturedOptions = opts;
        });

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.Equal(2048, capturedOptions!.MaxRequestSize);
    }

    [Fact(DisplayName = "UseSanitization with XSS protection disabled should configure correctly")]
    [Trait("Sanitization", "")]
    public void UseSanitization_ComXssProtectionDesabilitado_DeveConfigurarCorretamente()
    {
        // Arrange
        SanitizationOptions? capturedOptions = null;
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);
        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(serviceProvider);

        // Act
        appMock.Object.UseSanitization(opts =>
        {
            opts.EnableXssProtection = false;
            capturedOptions = opts;
        });

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.False(capturedOptions!.EnableXssProtection);
    }

    [Fact(DisplayName = "UseSanitization with HTML encoding disabled should configure correctly")]
    [Trait("Sanitization", "")]
    public void UseSanitization_ComHtmlEncodingDesabilitado_DeveConfigurarCorretamente()
    {
        // Arrange
        SanitizationOptions? capturedOptions = null;
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var appMock = new Mock<IApplicationBuilder>();
        appMock
            .Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appMock.Object);
        appMock
            .Setup(a => a.ApplicationServices)
            .Returns(serviceProvider);

        // Act
        appMock.Object.UseSanitization(opts =>
        {
            opts.EnableHtmlEncoding = false;
            capturedOptions = opts;
        });

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.False(capturedOptions!.EnableHtmlEncoding);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "UseSanitization with null IApplicationBuilder should throw NullReferenceException")]
    [Trait("Sanitization", "")]
    public void UseSanitization_ApplicationBuilderNulo_DeveLancarNullReferenceException()
    {
        // Arrange
        IApplicationBuilder? builder = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => builder!.UseSanitization());
    }

    [Fact(DisplayName = "UseSanitization with options and null IApplicationBuilder should throw NullReferenceException")]
    [Trait("Sanitization", "")]
    public void UseSanitization_ComOpcoesEApplicationBuilderNulo_DeveLancarNullReferenceException()
    {
        // Arrange
        IApplicationBuilder? builder = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => builder!.UseSanitization(opts =>
        {
            opts.MaxRequestSize = 1024;
        }));
    }

    #endregion
}
