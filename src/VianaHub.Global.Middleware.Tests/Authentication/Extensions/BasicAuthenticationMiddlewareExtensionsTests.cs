using EBL.FIG.Common.Middleware.Lib.Authentication;
using EBL.FIG.Common.Middleware.Lib.Authentication.Configuration;
using EBL.FIG.Common.Middleware.Lib.Authentication.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.Authentication.Extensions;

public class BasicAuthenticationMiddlewareExtensionsTests
{
    #region Sucesso

    [Fact(DisplayName = "AddBasicAuthentication should register options in service collection")]
    [Trait("Authentication", "")]
    public void AddBasicAuthentication_DeveRegistrarConfiguracoesNosServicos()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBasicAuthentication(options =>
        {
            options.Username = "user";
            options.Password = "pass";
        });

        var provider = services.BuildServiceProvider();
        var optionsInstance = provider.GetRequiredService<IOptions<BasicAuthenticationOptions>>();

        // Assert
        Assert.NotNull(optionsInstance);
        Assert.Equal("user", optionsInstance.Value.Username);
        Assert.Equal("pass", optionsInstance.Value.Password);
    }

    [Fact(DisplayName = "AddBasicAuthentication should return the same IServiceCollection")]
    [Trait("Authentication", "")]
    public void AddBasicAuthentication_DeveRetornarMesmoIServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBasicAuthentication(_ => { });

        // Assert
        Assert.Same(services, result);
    }

    [Fact(DisplayName = "AddBasicAuthentication with full configuration should register all options")]
    [Trait("Authentication", "")]
    public void AddBasicAuthentication_ComMultiplasConfiguracoes_DeveRegistrarTodasAsOpcoes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBasicAuthentication(options =>
        {
            options.Username = "admin";
            options.Password = "s3cr3t";
        });

        var provider = services.BuildServiceProvider();
        var optionsInstance = provider.GetRequiredService<IOptions<BasicAuthenticationOptions>>();

        // Assert
        Assert.Equal("admin", optionsInstance.Value.Username);
        Assert.Equal("s3cr3t", optionsInstance.Value.Password);
    }

    [Fact(DisplayName = "UseBasicAuthentication should return IApplicationBuilder")]
    [Trait("Authentication", "")]
    public void UseBasicAuthentication_DeveRetornarIApplicationBuilder()
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
        var result = appMock.Object.UseBasicAuthentication("user", "pass");

        // Assert
        Assert.NotNull(result);
    }

    [Fact(DisplayName = "UseBasicAuthentication should register middleware in the pipeline")]
    [Trait("Authentication", "")]
    public void UseBasicAuthentication_DeveRegistrarMiddlewareNoPipeline()
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
        appMock.Object.UseBasicAuthentication("user", "pass");

        // Assert
        appMock.Verify(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "AddBasicAuthentication with empty delegate should keep default values")]
    [Trait("Authentication", "")]
    public void AddBasicAuthentication_ComDadosVazios_DeveManterValoresPadrao()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBasicAuthentication(_ => { });

        var provider = services.BuildServiceProvider();
        var optionsInstance = provider.GetRequiredService<IOptions<BasicAuthenticationOptions>>();

        // Assert
        Assert.Equal(string.Empty, optionsInstance.Value.Username);
        Assert.Equal(string.Empty, optionsInstance.Value.Password);
    }

    [Fact(DisplayName = "AddBasicAuthentication without Username configured should be empty string")]
    [Trait("Authentication", "")]
    public void AddBasicAuthentication_UsernameNaoConfigurado_DeveSerStringVazia()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBasicAuthentication(options =>
        {
            options.Password = "apenas_senha";
        });

        var provider = services.BuildServiceProvider();
        var optionsInstance = provider.GetRequiredService<IOptions<BasicAuthenticationOptions>>();

        // Assert
        Assert.Equal(string.Empty, optionsInstance.Value.Username);
    }

    [Fact(DisplayName = "AddBasicAuthentication without Password configured should be empty string")]
    [Trait("Authentication", "")]
    public void AddBasicAuthentication_PasswordNaoConfigurada_DeveSerStringVazia()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBasicAuthentication(options =>
        {
            options.Username = "apenas_usuario";
        });

        var provider = services.BuildServiceProvider();
        var optionsInstance = provider.GetRequiredService<IOptions<BasicAuthenticationOptions>>();

        // Assert
        Assert.Equal(string.Empty, optionsInstance.Value.Password);
    }

    #endregion
}
