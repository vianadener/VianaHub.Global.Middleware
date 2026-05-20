// <copyright file="RequestResponseLoggingOptionsTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using EBL.FIG.Common.Middleware.Lib.Logging;
using System.Text.Json;

namespace EBL.FIG.Common.Middleware.Tests.Logging;

public class RequestResponseLoggingOptionsTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Success
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Default SensitiveHeaders should contain Authorization")]
    [Trait("Logging", "")]
    public void SensitiveHeaders_Padrao_DeveConterAuthorization()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.Contains("Authorization", options.SensitiveHeaders);
    }

    [Fact(DisplayName = "Default SensitiveHeaders should contain Cookie")]
    [Trait("Logging", "")]
    public void SensitiveHeaders_Padrao_DeveConterCookie()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.Contains("Cookie", options.SensitiveHeaders);
    }

    [Fact(DisplayName = "Default SensitiveHeaders should contain Set-Cookie")]
    [Trait("Logging", "")]
    public void SensitiveHeaders_Padrao_DeveConterSetCookie()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.Contains("Set-Cookie", options.SensitiveHeaders);
    }

    [Fact(DisplayName = "Default SensitiveHeaders should contain X-API-Key")]
    [Trait("Logging", "")]
    public void SensitiveHeaders_Padrao_DeveConterXApiKey()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.Contains("X-API-Key", options.SensitiveHeaders);
    }

    [Fact(DisplayName = "Default SensitiveHeaders should contain ApiKey")]
    [Trait("Logging", "")]
    public void SensitiveHeaders_Padrao_DeveConterApiKey()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.Contains("ApiKey", options.SensitiveHeaders);
    }

    [Fact(DisplayName = "Default SensitiveHeaders should contain Password")]
    [Trait("Logging", "")]
    public void SensitiveHeaders_Padrao_DeveConterPassword()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.Contains("Password", options.SensitiveHeaders);
    }

    [Fact(DisplayName = "SensitiveHeaders lookup should be case-insensitive")]
    [Trait("Logging", "")]
    public void SensitiveHeaders_BuscaDeveSerCaseInsensitive()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.Contains("authorization", options.SensitiveHeaders);
        Assert.Contains("COOKIE", options.SensitiveHeaders);
    }

    [Fact(DisplayName = "Default PathFilters should be an empty list")]
    [Trait("Logging", "")]
    public void PathFilters_Padrao_DeveSerListaVazia()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.NotNull(options.PathFilters);
        Assert.Empty(options.PathFilters);
    }

    [Fact(DisplayName = "Default LogErrorsOnly should be false")]
    [Trait("Logging", "")]
    public void LogErrorsOnly_Padrao_DeveSerFalso()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.False(options.LogErrorsOnly);
    }

    [Fact(DisplayName = "Default EnableBuffering should be true")]
    [Trait("Logging", "")]
    public void EnableBuffering_Padrao_DeveSerVerdadeiro()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.True(options.EnableBuffering);
    }

    [Fact(DisplayName = "Default BufferSize should be 1000")]
    [Trait("Logging", "")]
    public void BufferSize_Padrao_DeveSer1000()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.Equal(1000, options.BufferSize);
    }

    [Fact(DisplayName = "Default JsonSerializerOptions should not write indented")]
    [Trait("Logging", "")]
    public void JsonSerializerOptions_Padrao_NaoDeveEscreverIdentado()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.NotNull(options.JsonSerializerOptions);
        Assert.False(options.JsonSerializerOptions.WriteIndented);
    }

    [Fact(DisplayName = "PathFilters should accept added entries")]
    [Trait("Logging", "")]
    public void PathFilters_DeveAceitarEntradas()
    {
        var options = new RequestResponseLoggingOptions();
        options.PathFilters.Add("/api");
        Assert.Single(options.PathFilters);
        Assert.Equal("/api", options.PathFilters[0]);
    }

    [Fact(DisplayName = "LogErrorsOnly set to true should retain configured value")]
    [Trait("Logging", "")]
    public void LogErrorsOnly_DefinidoComoVerdadeiro_DeveManterValor()
    {
        var options = new RequestResponseLoggingOptions { LogErrorsOnly = true };
        Assert.True(options.LogErrorsOnly);
    }

    [Fact(DisplayName = "BufferSize set to custom value should retain configured value")]
    [Trait("Logging", "")]
    public void BufferSize_DefinidoComValorCustom_DeveManterValor()
    {
        var options = new RequestResponseLoggingOptions { BufferSize = 500 };
        Assert.Equal(500, options.BufferSize);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Failure
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "SensitiveHeaders should not contain non-sensitive header")]
    [Trait("Logging", "")]
    public void SensitiveHeaders_NaoDeveConterHeaderNaoSensivel()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.DoesNotContain("Content-Type", options.SensitiveHeaders);
    }

    [Fact(DisplayName = "PathFilters should not be null after instantiation")]
    [Trait("Logging", "")]
    public void PathFilters_NaoDeveSerNuloAposInstanciacao()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.NotNull(options.PathFilters);
    }

    [Fact(DisplayName = "SensitiveHeaders should not be null after instantiation")]
    [Trait("Logging", "")]
    public void SensitiveHeaders_NaoDeveSerNuloAposInstanciacao()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.NotNull(options.SensitiveHeaders);
    }

    [Fact(DisplayName = "JsonSerializerOptions should not be null after instantiation")]
    [Trait("Logging", "")]
    public void JsonSerializerOptions_NaoDeveSerNuloAposInstanciacao()
    {
        var options = new RequestResponseLoggingOptions();
        Assert.NotNull(options.JsonSerializerOptions);
    }
}
