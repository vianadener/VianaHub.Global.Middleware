// <copyright file="RequestResponseLoggingMiddlewareExtensionsTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using EBL.FIG.Common.Middleware.Lib.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace EBL.FIG.Common.Middleware.Tests.Logging.Extensions;

public class RequestResponseLoggingMiddlewareExtensionsTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Success
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "AddRequestResponseLogging should return the same IServiceCollection")]
    [Trait("Logging", "")]
    public void AddRequestResponseLogging_DeveRetornarMesmoIServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        var result = services.AddRequestResponseLogging(configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact(DisplayName = "AddRequestResponseLogging should register RequestResponseLoggingOptions in DI container")]
    [Trait("Logging", "")]
    public void AddRequestResponseLogging_DeveRegistrarOptionsNoContainerDI()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddRequestResponseLogging(configuration);
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<RequestResponseLoggingOptions>>();

        // Assert
        Assert.NotNull(options);
    }

    [Fact(DisplayName = "AddRequestResponseLogging should bind configuration section RequestResponseLogging")]
    [Trait("Logging", "")]
    public void AddRequestResponseLogging_DeveVincularSecaoDeConfiguracao()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "RequestResponseLogging:LogErrorsOnly", "true" },
            { "RequestResponseLogging:BufferSize", "250" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        services.AddRequestResponseLogging(configuration);
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestResponseLoggingOptions>>().Value;

        // Assert
        Assert.True(options.LogErrorsOnly);
        Assert.Equal(250, options.BufferSize);
    }

    [Fact(DisplayName = "UseRequestResponseLogging should return IApplicationBuilder")]
    [Trait("Logging", "")]
    public void UseRequestResponseLogging_DeveRetornarIApplicationBuilder()
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
        var result = appMock.Object.UseRequestResponseLogging();

        // Assert
        Assert.NotNull(result);
    }

    [Fact(DisplayName = "UseRequestResponseLogging should register middleware in the pipeline")]
    [Trait("Logging", "")]
    public void UseRequestResponseLogging_DeveRegistrarMiddlewareNoPipeline()
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
        appMock.Object.UseRequestResponseLogging();

        // Assert
        appMock.Verify(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Failure
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "AddRequestResponseLogging with empty configuration should use default option values")]
    [Trait("Logging", "")]
    public void AddRequestResponseLogging_ConfiguracaoVazia_DeveUsarValoresPadrao()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddRequestResponseLogging(configuration);
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestResponseLoggingOptions>>().Value;

        // Assert
        Assert.False(options.LogErrorsOnly);
        Assert.Equal(1000, options.BufferSize);
        Assert.True(options.EnableBuffering);
    }
}
