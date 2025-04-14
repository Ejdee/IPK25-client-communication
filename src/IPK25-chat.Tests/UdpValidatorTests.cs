using IPK25_chat.Enums;
using IPK25_chat.Validators;

namespace IPK25_chat.Tests;

public class UdpValidatorTests
{
    private readonly UdpValidator _validator = new UdpValidator();
    
    [Fact]
    public void ValidateAndGetMsgType_ValidConfirm()
    {
        // Arrange
        var message = new byte[] { 0x00, 0x02, 0x03 };
        var expectedMessageType = MessageType.CONFIRM;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidConfirm()
    {
        // Arrange
        // TODO: VALID? 
        var message = new byte[] { 0x00, 0x02, 0x03, 0x04 };
        var expectedMessageType = MessageType.CONFIRM;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_ValidReplySuccess()
    {
        // Arrange
        var message = new byte[] { 0x01, 0x02, 0x03, 0x01, 0x00, 0x04, (byte)'a', (byte)'h', (byte)'o', (byte)'j', 0x00 };
        var expectedMessageType = MessageType.REPLY;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_ValidReplyFailure()
    {
        // Arrange
        var message = new byte[] { 0x01, 0x02, 0x03, 0x00, 0x00, 0x04, (byte)'a', (byte)'h', (byte)'o', (byte)'j', 0x00 };
        var expectedMessageType = MessageType.NOTREPLY;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidReply_MissingNullTerminator()
    {
        // Arrange
        var message = new byte[] { 0x01, 0x02, 0x03, 0x00, 0x00, 0x04, (byte)'a', (byte)'h', (byte)'o', (byte)'j'};
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidReply_MissingPayload()
    {
        // Arrange
        var message = new byte[] { 0x01, 0x02, 0x03};
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_ValidAuth()
    {
        // Arrange
        var message = new byte[]
        {
            0x02, 0x02, 0x03,
            (byte)'u', (byte)'s', (byte)'e', (byte)'r', 0x00,
            (byte)'d', (byte)'i', (byte)'s', (byte)'p', (byte)'l', (byte)'a', (byte)'y', (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00,
            (byte)'s', (byte)'e', (byte)'c', (byte)'r', (byte)'e', (byte)'t', 0x00
        };
        var expectedMessageType = MessageType.AUTH;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidAuth_MissingNullTerminatorSecret()
    {
        // Arrange
        var message = new byte[]
        {
            0x02, 0x02, 0x03,
            (byte)'u', (byte)'s', (byte)'e', (byte)'r', 0x00,
            (byte)'d', (byte)'i', (byte)'s', (byte)'p', (byte)'l', (byte)'a', (byte)'y', (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00,
            (byte)'s', (byte)'e', (byte)'c', (byte)'r', (byte)'e', (byte)'t'
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidAuth_MissingNullTerminatorUser()
    {
        // Arrange
        var message = new byte[]
        {
            0x02, 0x02, 0x03,
            (byte)'u', (byte)'s', (byte)'e', (byte)'r',
            (byte)'d', (byte)'i', (byte)'s', (byte)'p', (byte)'l', (byte)'a', (byte)'y', (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00,
            (byte)'s', (byte)'e', (byte)'c', (byte)'r', (byte)'e', (byte)'t', 0x00
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidAuth_TooLongUsername()
    {
        // Arrange
        var message = new byte[]
        {
            0x02, 0x02, 0x03,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00,
            (byte)'d', (byte)'i', (byte)'s', (byte)'p', (byte)'l', (byte)'a', (byte)'y', (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00,
            (byte)'s', (byte)'e', (byte)'c', (byte)'r', (byte)'e', (byte)'t', 0x00
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_ValidJoin()
    {
        // Arrange
        var message = new byte[]
        {
            0x03, 0x02, 0x03,
            (byte)'c', (byte)'h', (byte)'a', (byte)'n', (byte)'n', (byte)'e', (byte)'l', 0x00,
            (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00
        };
        var expectedMessageType = MessageType.JOIN;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_InvalidJoin_MissingDisplayName()
    {
        // Arrange
        var message = new byte[]
        {
            0x03, 0x02, 0x03,
            (byte)'c', (byte)'h', (byte)'a', (byte)'n', (byte)'n', (byte)'e', (byte)'l', 0x00,
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_InvalidJoin_LongDisplayName()
    {
        // Arrange
        var message = new byte[]
        {
            0x03, 0x02, 0x03,
            (byte)'c', (byte)'h', (byte)'a', (byte)'n', (byte)'n', (byte)'e', (byte)'l', 0x00,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00,
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_ValidMessage()
    {
        // Arrange
        var message = new byte[]
        {
            0x04, 0x02, 0x03,
            (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00,
            (byte)'m', (byte)'e', (byte)'s', (byte)'s', (byte)'a', (byte)'g', (byte)'e', 0x00
        };
        var expectedMessageType = MessageType.MSG;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidMessage_MissingNullTerminator()
    {
        // Arrange
        var message = new byte[]
        {
            0x04, 0x02, 0x03,
            (byte)'m', (byte)'e', (byte)'s', (byte)'s', (byte)'a', (byte)'g', (byte)'e'
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_ValidPing()
    {
        // Arrange
        var message = new byte[]
        {
            0xFD, 0x02, 0x03,
        };
        var expectedMessageType = MessageType.PING;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidPing()
    {
        // Arrange
        var message = new byte[]
        {
            0xFD, 0x02, 0x03, 0x00
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_ValidErr()
    {
        // Arrange
        var message = new byte[]
        {
            0xFE, 0x02, 0x03,
            (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00,
            (byte)'e', (byte)'r', (byte)'r', (byte)'o', (byte)'r', 0x00
        };
        var expectedMessageType = MessageType.ERR;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidErr_MissingNullTerminatorErrContent()
    {
        // Arrange
        var message = new byte[]
        {
            0xFE, 0x02, 0x03,
            (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00,
            (byte)'e', (byte)'r', (byte)'r', (byte)'o', (byte)'r'
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidErr_MissingName()
    {
        // Arrange
        var message = new byte[]
        {
            0xFE, 0x02, 0x03,
            (byte)'e', (byte)'r', (byte)'r', (byte)'o', (byte)'r', 0x00
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_ValidBye()
    {
        // Arrange
        var message = new byte[]
        {
            0xFF, 0x02, 0x03,
            (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00
        };
        var expectedMessageType = MessageType.BYE;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidBye_MissingNullTerminator()
    {
        // Arrange
        var message = new byte[]
        {
            0xFF, 0x02, 0x03,
            (byte)'n', (byte)'a', (byte)'m', (byte)'e'
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }
    
    [Fact]
    public void ValidateAndGetMsgType_InvalidBye_MissingName()
    {
        // Arrange
        var message = new byte[]
        {
            0xFF, 0x02, 0x03,
        };
        var expectedMessageType = MessageType.INVALID;

        // Act
        var result = _validator.ValidateAndGetMsgType(message);

        // Assert
        Assert.Equal(expectedMessageType, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_InvalidHeader()
    {
        // Arrange
        var message = new byte[]
        {
            0x00
        };
        var expectedMessageType = MessageType.INVALID;
        
        // Act
        var result = _validator.ValidateAndGetMsgType(message);
        
        // Assert
        Assert.Equal(expectedMessageType, result);
    }

    [Fact]
    public void ValidateAndGetMsgType_InvalidType()
    {
        // Arrange
        var message = new byte[]
        {
            0x10, 0x02, 0x03,
            (byte)'n', (byte)'a', (byte)'m', (byte)'e', 0x00
        };
        
        var expectedMessageType = MessageType.INVALID;
        
        // Act
        var result = _validator.ValidateAndGetMsgType(message);
        
        // Assert
        Assert.Equal(expectedMessageType, result);
    }
}