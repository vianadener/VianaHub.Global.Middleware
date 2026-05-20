using EBL.FIG.Common.Middleware.Lib.Sanitization;

namespace EBL.FIG.Common.Middleware.Tests.Sanitization;

public class SanitizationOptionsTests
{
    #region Sucesso

    [Fact(DisplayName = "Default MaxRequestSize should be 10485760 bytes (10MB)")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_MaxRequestSizePadrao_DeveSer10MB()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.Equal(10485760, options.MaxRequestSize);
    }

    [Fact(DisplayName = "Default EnableHtmlEncoding should be true")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_EnableHtmlEncodingPadrao_DeveSerTrue()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.True(options.EnableHtmlEncoding);
    }

    [Fact(DisplayName = "Default EnableXssProtection should be true")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_EnableXssProtectionPadrao_DeveSerTrue()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.True(options.EnableXssProtection);
    }

    [Fact(DisplayName = "Default AllowedCharacters should contain expected values")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_AllowedCharactersPadrao_DeveConterValoresEsperados()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.NotNull(options.AllowedCharacters);
        Assert.Contains("@", options.AllowedCharacters);
        Assert.Contains("_", options.AllowedCharacters);
        Assert.Contains("-", options.AllowedCharacters);
        Assert.Contains(".", options.AllowedCharacters);
        Assert.Contains(",", options.AllowedCharacters);
        Assert.Contains(" ", options.AllowedCharacters);
    }

    [Fact(DisplayName = "Default AllowedCharacters should have 6 entries")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_AllowedCharactersPadrao_DeveTer6Entradas()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.Equal(6, options.AllowedCharacters.Length);
    }

    [Fact(DisplayName = "Default DeniedCharacters should contain expected values")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_DeniedCharactersPadrao_DeveConterValoresEsperados()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.NotNull(options.DeniedCharacters);
        Assert.Contains("<", options.DeniedCharacters);
        Assert.Contains(">", options.DeniedCharacters);
        Assert.Contains("'", options.DeniedCharacters);
        Assert.Contains("\"", options.DeniedCharacters);
        Assert.Contains(";", options.DeniedCharacters);
        Assert.Contains("=", options.DeniedCharacters);
        Assert.Contains("(", options.DeniedCharacters);
        Assert.Contains(")", options.DeniedCharacters);
        Assert.Contains("{", options.DeniedCharacters);
        Assert.Contains("}", options.DeniedCharacters);
    }

    [Fact(DisplayName = "Default DeniedCharacters should have 10 entries")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_DeniedCharactersPadrao_DeveTer10Entradas()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.Equal(10, options.DeniedCharacters.Length);
    }

    [Fact(DisplayName = "Default MaxStringLength should be 4000")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_MaxStringLengthPadrao_DeveSer4000()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.Equal(4000, options.MaxStringLength);
    }

    [Fact(DisplayName = "Default ValidateRuleExpressions should be true")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_ValidateRuleExpressionsPadrao_DeveSerTrue()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.True(options.ValidateRuleExpressions);
    }

    [Fact(DisplayName = "Default ValidateJsonStructure should be true")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_ValidateJsonStructurePadrao_DeveSerTrue()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.True(options.ValidateJsonStructure);
    }

    [Fact(DisplayName = "MaxRequestSize should be settable to a custom value")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_MaxRequestSize_DeveAceitarValorPersonalizado()
    {
        // Arrange
        var options = new SanitizationOptions();

        // Act
        options.MaxRequestSize = 5242880;

        // Assert
        Assert.Equal(5242880, options.MaxRequestSize);
    }

    [Fact(DisplayName = "EnableHtmlEncoding should be settable to false")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_EnableHtmlEncoding_DeveAceitarFalse()
    {
        // Arrange
        var options = new SanitizationOptions();

        // Act
        options.EnableHtmlEncoding = false;

        // Assert
        Assert.False(options.EnableHtmlEncoding);
    }

    [Fact(DisplayName = "EnableXssProtection should be settable to false")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_EnableXssProtection_DeveAceitarFalse()
    {
        // Arrange
        var options = new SanitizationOptions();

        // Act
        options.EnableXssProtection = false;

        // Assert
        Assert.False(options.EnableXssProtection);
    }

    [Fact(DisplayName = "AllowedCharacters should be replaceable with a custom array")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_AllowedCharacters_DeveAceitarArrayPersonalizado()
    {
        // Arrange
        var options = new SanitizationOptions();
        var custom = new[] { "#", "%" };

        // Act
        options.AllowedCharacters = custom;

        // Assert
        Assert.Equal(custom, options.AllowedCharacters);
    }

    [Fact(DisplayName = "DeniedCharacters should be replaceable with a custom array")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_DeniedCharacters_DeveAceitarArrayPersonalizado()
    {
        // Arrange
        var options = new SanitizationOptions();
        var custom = new[] { "!", "@" };

        // Act
        options.DeniedCharacters = custom;

        // Assert
        Assert.Equal(custom, options.DeniedCharacters);
    }

    [Fact(DisplayName = "MaxStringLength should be settable to a custom value")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_MaxStringLength_DeveAceitarValorPersonalizado()
    {
        // Arrange
        var options = new SanitizationOptions();

        // Act
        options.MaxStringLength = 500;

        // Assert
        Assert.Equal(500, options.MaxStringLength);
    }

    [Fact(DisplayName = "ValidateRuleExpressions should be settable to false")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_ValidateRuleExpressions_DeveAceitarFalse()
    {
        // Arrange
        var options = new SanitizationOptions();

        // Act
        options.ValidateRuleExpressions = false;

        // Assert
        Assert.False(options.ValidateRuleExpressions);
    }

    [Fact(DisplayName = "ValidateJsonStructure should be settable to false")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_ValidateJsonStructure_DeveAceitarFalse()
    {
        // Arrange
        var options = new SanitizationOptions();

        // Act
        options.ValidateJsonStructure = false;

        // Assert
        Assert.False(options.ValidateJsonStructure);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "AllowedCharacters default should not be empty")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_AllowedCharactersPadrao_NaoDeveSerVazio()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.NotEmpty(options.AllowedCharacters);
    }

    [Fact(DisplayName = "DeniedCharacters default should not be empty")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_DeniedCharactersPadrao_NaoDeveSerVazio()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.NotEmpty(options.DeniedCharacters);
    }

    [Fact(DisplayName = "MaxRequestSize default should not be zero")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_MaxRequestSizePadrao_NaoDeveSerZero()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.NotEqual(0, options.MaxRequestSize);
    }

    [Fact(DisplayName = "MaxStringLength default should not be zero")]
    [Trait("Sanitization", "")]
    public void SanitizationOptions_MaxStringLengthPadrao_NaoDeveSerZero()
    {
        // Arrange & Act
        var options = new SanitizationOptions();

        // Assert
        Assert.NotEqual(0, options.MaxStringLength);
    }

    #endregion
}
