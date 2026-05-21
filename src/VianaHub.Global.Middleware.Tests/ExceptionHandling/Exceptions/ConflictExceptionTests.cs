using EBL.FIG.Common.Middleware.Lib.ExceptionHandling.Exceptions;

namespace EBL.FIG.Common.Middleware.Tests.ExceptionHandling.Exceptions;

public class ConflictExceptionTests
{
    #region Sucesso

    [Fact(DisplayName = "ConflictException - Default constructor should create instance with empty message")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_Padrao_DeveCriarInstanciaComMensagemPadrao()
    {
        // Arrange & Act
        var exception = new ConflictException();

        // Assert
        Assert.NotNull(exception);
    }

    [Fact(DisplayName = "ConflictException - Constructor with message should set Message property correctly")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComMensagem_DeveDefinirPropriedadeMensagem()
    {
        // Arrange
        const string message = "Resource already exists";

        // Act
        var exception = new ConflictException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact(DisplayName = "ConflictException - Constructor with message and inner exception should set both properties")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComMensagemEInnerException_DeveDefinirAmbasAsPropriedades()
    {
        // Arrange
        const string message = "Conflict detected";
        var innerException = new InvalidOperationException("Unique constraint violated");

        // Act
        var exception = new ConflictException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact(DisplayName = "ConflictException - Should be assignable from Exception base class")]
    [Trait("ExceptionHandling", "")]
    public void ConflictException_DeveSerAtribuivelDeException()
    {
        // Arrange & Act
        var exception = new ConflictException("Conflict");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact(DisplayName = "ConflictException - Should be catchable as base Exception type")]
    [Trait("ExceptionHandling", "")]
    public void ConflictException_DeveSerCapturadaComoTipoBaseException()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            throw new ConflictException("Duplicate resource");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.IsType<ConflictException>(caughtException);
    }

    [Fact(DisplayName = "ConflictException - InnerException should be null when not provided in single-message constructor")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComMensagemSemInnerException_InnerExceptionDeveSerNulo()
    {
        // Arrange & Act
        var exception = new ConflictException("Conflict");

        // Assert
        Assert.Null(exception.InnerException);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "ConflictException - Constructor with empty message should store empty string")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComMensagemVazia_DeveArmazenarStringVazia()
    {
        // Arrange & Act
        var exception = new ConflictException(string.Empty);

        // Assert
        Assert.Equal(string.Empty, exception.Message);
    }

    [Fact(DisplayName = "ConflictException - Should not be of type NotFoundException")]
    [Trait("ExceptionHandling", "")]
    public void ConflictException_NaoDeveSerDoTipoNotFoundException()
    {
        // Arrange & Act
        var exception = new ConflictException("Conflict");

        // Assert
        Assert.IsNotType<NotFoundException>(exception);
    }

    [Fact(DisplayName = "ConflictException - Constructor with message and inner exception should not lose inner exception reference")]
    [Trait("ExceptionHandling", "")]
    public void Constructor_ComInnerException_NaoDevePerderReferenciaInnerException()
    {
        // Arrange
        var inner = new Exception("inner");
        var exception = new ConflictException("outer", inner);

        // Assert
        Assert.Same(inner, exception.InnerException);
    }

    #endregion
}
