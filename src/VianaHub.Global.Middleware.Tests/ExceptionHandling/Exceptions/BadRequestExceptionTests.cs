using EBL.FIG.Common.Middleware.Lib.ExceptionHandling.Exceptions;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling.Exceptions;

public class BadRequestExceptionTests
{
    #region Sucesso

    [Fact(DisplayName = "BadRequestException - Constructor with message should set Message property correctly")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComMensagem_DeveDefinirPropriedadeMensagem()
    {
        // Arrange
        const string message = "Invalid request payload";

        // Act
        var exception = new BadRequestException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact(DisplayName = "BadRequestException - Should be assignable from Exception base class")]
    [Trait("ExceptionHandling", "")]
    public void BadRequestException_DeveSerAtribuivelDeException()
    {
        // Arrange & Act
        var exception = new BadRequestException("Bad request");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact(DisplayName = "BadRequestException - Should be catchable as base Exception type")]
    [Trait("ExceptionHandling", "")]
    public void BadRequestException_DeveSerCapturadaComoTipoBaseException()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            throw new BadRequestException("Malformed request");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.IsType<BadRequestException>(caughtException);
    }

    [Fact(DisplayName = "BadRequestException - InnerException should be null when not provided")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_SemInnerException_InnerExceptionDeveSerNulo()
    {
        // Arrange & Act
        var exception = new BadRequestException("Bad request");

        // Assert
        Assert.Null(exception.InnerException);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "BadRequestException - Constructor with empty message should store empty string")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComMensagemVazia_DeveArmazenarStringVazia()
    {
        // Arrange & Act
        var exception = new BadRequestException(string.Empty);

        // Assert
        Assert.Equal(string.Empty, exception.Message);
    }

    [Fact(DisplayName = "BadRequestException - Should not be of type NotFoundException")]
    [Trait("ExceptionHandling", "")]
    public void BadRequestException_NaoDeveSerDoTipoNotFoundException()
    {
        // Arrange & Act
        var exception = new BadRequestException("Bad request");

        // Assert
        Assert.IsNotType<NotFoundException>(exception);
    }

    #endregion
}
