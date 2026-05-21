using EBL.FIG.Common.Middleware.Lib.ExceptionHandling.Exceptions;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling.Exceptions;

public class NotFoundExceptionTests
{
    #region Sucesso

    [Fact(DisplayName = "NotFoundException - Constructor with message should set Message property correctly")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComMensagem_DeveDefinirPropriedadeMensagem()
    {
        // Arrange
        const string message = "Resource not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact(DisplayName = "NotFoundException - Should be assignable from Exception base class")]
    [Trait("ExceptionHandling", "")]
    public void NotFoundException_DeveSerAtribuivelDeException()
    {
        // Arrange & Act
        var exception = new NotFoundException("Not found");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact(DisplayName = "NotFoundException - Should be catchable as base Exception type")]
    [Trait("ExceptionHandling", "")]
    public void NotFoundException_DeveSerCapturadaComoTipoBaseException()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            throw new NotFoundException("Entity not found");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.IsType<NotFoundException>(caughtException);
    }

    [Fact(DisplayName = "NotFoundException - InnerException should be null when not provided")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_SemInnerException_InnerExceptionDeveSerNulo()
    {
        // Arrange & Act
        var exception = new NotFoundException("Not found");

        // Assert
        Assert.Null(exception.InnerException);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "NotFoundException - Constructor with empty message should store empty string")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComMensagemVazia_DeveArmazenarStringVazia()
    {
        // Arrange & Act
        var exception = new NotFoundException(string.Empty);

        // Assert
        Assert.Equal(string.Empty, exception.Message);
    }

    [Fact(DisplayName = "NotFoundException - Should not be of type ArgumentException")]
    [Trait("ExceptionHandling", "")]
    public void NotFoundException_NaoDeveSerDoTipoArgumentException()
    {
        // Arrange & Act
        var exception = new NotFoundException("Not found");

        // Assert
        Assert.IsNotType<ArgumentException>(exception);
    }

    #endregion
}
