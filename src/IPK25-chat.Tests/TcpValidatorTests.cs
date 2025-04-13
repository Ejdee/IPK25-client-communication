using System.Text;
using IPK25_chat.Enums;
using IPK25_chat.Validators;

namespace IPK25_chat.Tests;

public class TcpValidatorTests
{
    [Fact]
    public void ValidateAndGetMsgType_ValidAuthMessage()
    {
        // Arrange
        var validator = new TcpValidator();
        var message = Encoding.ASCII.GetBytes("AUTH user123 AS DisplayName USING secret123\r\n");

        // Act
        var result = validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(MessageType.AUTH, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_ValidJoinMessage()
    {
        // Arrange
        var validator = new TcpValidator();
        var message = Encoding.ASCII.GetBytes("JOIN user123 AS DisplayName\r\n");

        // Act
        var result = validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(MessageType.JOIN, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_UnknownMessage()
    {
        // Arrange
        var validator = new TcpValidator();
        var message = Encoding.ASCII.GetBytes("UNKNOWN MESSAGE FORMAT\r\n");

        // Act
        var result = validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(MessageType.INVALID, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_ValidMsgMessage()
    {
        // Arrange
        var validator = new TcpValidator();
        var message = Encoding.ASCII.GetBytes("MSG FROM DisplayName IS Hello, world!\r\n");

        // Act
        var result = validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(MessageType.MSG, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_ValidErrMessage()
    {
        // Arrange
        var validator = new TcpValidator();
        var message = Encoding.ASCII.GetBytes("ERR FROM DisplayName IS Something went wrong\r\n");

        // Act
        var result = validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(MessageType.ERR, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_ValidByeMessage()
    {
        // Arrange
        var validator = new TcpValidator();
        var message = Encoding.ASCII.GetBytes("BYE FROM DisplayName\r\n");

        // Act
        var result = validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(MessageType.BYE, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_ValidReplyOkMessage()
    {
        // Arrange
        var validator = new TcpValidator();
        var message = Encoding.ASCII.GetBytes("REPLY OK IS Success yeha boi\r\n");

        // Act
        var result = validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(MessageType.REPLY, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_ValidReplyNokMessage()
    {
        // Arrange
        var validator = new TcpValidator();
        var message = Encoding.ASCII.GetBytes("REPLY NOK IS Failure\r\n");

        // Act
        var result = validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(MessageType.NOTREPLY, result);
    }
}