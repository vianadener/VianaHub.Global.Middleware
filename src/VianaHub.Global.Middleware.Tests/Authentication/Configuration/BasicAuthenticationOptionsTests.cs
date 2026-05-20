using EBL.FIG.Common.Middleware.Lib.Authentication.Configuration;
using FizzWare.NBuilder;

namespace EBL.FIG.Common.Middleware.Tests.Authentication.Configuration;

public class BasicAuthenticationOptionsTests
{
    #region Sucesso

    [Fact(DisplayName = "Default instance Username should be empty string")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_Instanciapadrao_UsernameDeveSerStringVazia()
    {
        // Arrange & Act
        var options = new BasicAuthenticationOptions();

        // Assert
        Assert.Equal(string.Empty, options.Username);
    }

    [Fact(DisplayName = "Default instance Password should be empty string")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_InstanciaPadrao_PasswordDeveSerStringVazia()
    {
        // Arrange & Act
        var options = new BasicAuthenticationOptions();

        // Assert
        Assert.Equal(string.Empty, options.Password);
    }

    [Fact(DisplayName = "Setting Username should store the correct value")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_DefinirUsername_DeveArmazenarValorCorreto()
    {
        // Arrange
        const string expectedUsername = "meu_usuario";
        var options = new BasicAuthenticationOptions
        {
            Username = expectedUsername
        };

        // Act & Assert
        Assert.Equal(expectedUsername, options.Username);
    }

    [Fact(DisplayName = "Setting Password should store the correct value")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_DefinirPassword_DeveArmazenarValorCorreto()
    {
        // Arrange
        const string expectedPassword = "minha_senha";
        var options = new BasicAuthenticationOptions
        {
            Password = expectedPassword
        };

        // Act & Assert
        Assert.Equal(expectedPassword, options.Password);
    }

    [Fact(DisplayName = "Instance created via NBuilder should contain filled properties")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_CriadoViaNBuilder_DeveConterPropriedadesPreenchidas()
    {
        // Arrange & Act
        var options = Builder<BasicAuthenticationOptions>
            .CreateNew()
            .With(o => o.Username = "user_nb")
            .With(o => o.Password = "pass_nb")
            .Build();

        // Assert
        Assert.Equal("user_nb", options.Username);
        Assert.Equal("pass_nb", options.Password);
    }

    [Fact(DisplayName = "List created via NBuilder should contain multiple items")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_ListaCriadaViaNBuilder_DeveConterMultiplosItens()
    {
        // Arrange & Act
        var optionsList = Builder<BasicAuthenticationOptions>
            .CreateListOfSize(5)
            .Build();

        // Assert
        Assert.Equal(5, optionsList.Count);
        Assert.All(optionsList, o =>
        {
            Assert.NotNull(o.Username);
            Assert.NotNull(o.Password);
        });
    }

    [Fact(DisplayName = "Updating Username after instantiation should reflect new value")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_AlterarUsernameAposInstancia_DeveRefletirNovoValor()
    {
        // Arrange
        var options = new BasicAuthenticationOptions { Username = "inicial" };

        // Act
        options.Username = "atualizado";

        // Assert
        Assert.Equal("atualizado", options.Username);
    }

    [Fact(DisplayName = "Updating Password after instantiation should reflect new value")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_AlterarPasswordAposInstancia_DeveRefletirNovoValor()
    {
        // Arrange
        var options = new BasicAuthenticationOptions { Password = "inicial" };

        // Act
        options.Password = "atualizada";

        // Assert
        Assert.Equal("atualizada", options.Password);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "Null Username should not equal empty string")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_UsernameNulo_NaoDeveIgualarStringVazia()
    {
        // Arrange
        var options = new BasicAuthenticationOptions();
#pragma warning disable CS8625
        options.Username = null;
#pragma warning restore CS8625

        // Act & Assert
        Assert.Null(options.Username);
    }

    [Fact(DisplayName = "Null Password should not equal empty string")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_PasswordNula_NaoDeveIgualarStringVazia()
    {
        // Arrange
        var options = new BasicAuthenticationOptions();
#pragma warning disable CS8625
        options.Password = null;
#pragma warning restore CS8625

        // Act & Assert
        Assert.Null(options.Password);
    }

    [Fact(DisplayName = "Distinct usernames should not be equal")]
    [Trait("Authentication", "")]
    public void BasicAuthenticationOptions_UsernamesDistintos_NaoDevemSerIguais()
    {
        // Arrange
        var options1 = Builder<BasicAuthenticationOptions>
            .CreateNew()
            .With(o => o.Username = "userA")
            .Build();

        var options2 = Builder<BasicAuthenticationOptions>
            .CreateNew()
            .With(o => o.Username = "userB")
            .Build();

        // Act & Assert
        Assert.NotEqual(options1.Username, options2.Username);
    }

    #endregion
}
