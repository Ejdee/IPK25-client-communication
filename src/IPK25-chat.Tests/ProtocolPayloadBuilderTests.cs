using System.Text;
using IPK25_chat.Enums;
using IPK25_chat.Models;
using IPK25_chat.Protocol;
using Xunit;

namespace IPK25_chat.Tests;

public class ProtocolPayloadBuilderTests
{
    [Fact]
    public void GetPayloadFromMessage_ShouldCreateAuthPayload()
    {
        // Arrange
        var builder = new ProtocolPayloadBuilder(new UserModel("TestUser", "TestDisplayName"));
        var message = new MessageModel (
            MessageType.AUTH,
            new Dictionary<string, string>
            {
                { "username", "TestUser" },
                { "displayName", "TestDisplayName" },
                { "secret", "TestSecret" }
            });

        // Act
        var payload = builder.GetPayloadFromMessage(message);

        // Assert
        var expectedPayload = new List<byte>
        {
            (byte)PayloadType.AUTH, // Message type
            0, 0, // ID (initial value)
        };
        expectedPayload.AddRange(Encoding.ASCII.GetBytes("TestUser"));
        expectedPayload.Add(0);
        expectedPayload.AddRange(Encoding.ASCII.GetBytes("TestDisplayName"));
        expectedPayload.Add(0);
        expectedPayload.AddRange(Encoding.ASCII.GetBytes("TestSecret"));
        expectedPayload.Add(0);

        Assert.Equal(expectedPayload.ToArray(), payload);
    }

    [Fact]
    public void GetPayloadFromMessage_ShouldCreateJoinPayload()
    {
        // Arrange
        var builder = new ProtocolPayloadBuilder(new UserModel("TestUser", "TestDisplayName"));
        var message = new MessageModel(
            MessageType.JOIN,
            new Dictionary<string, string>
            {
                { "channelID", "TestChannel" }
            });
        
        // Act
        var payload = builder.GetPayloadFromMessage(message);

        // Assert
        var expectedPayload = new List<byte>
        {
            (byte)PayloadType.JOIN, // Message type
            0, 0, // ID (initial value)
        };
        expectedPayload.AddRange(Encoding.ASCII.GetBytes("TestChannel"));
        expectedPayload.Add(0);
        expectedPayload.AddRange(Encoding.ASCII.GetBytes("TestDisplayName"));
        expectedPayload.Add(0);

        Assert.Equal(expectedPayload.ToArray(), payload);
    }

    [Fact]
    public void GetPayloadFromMessage_ShouldCreateMsgPayload()
    {
        // Arrange
        var builder = new ProtocolPayloadBuilder(new UserModel("TestUser", "TestDisplayName"));
        var message = new MessageModel(
            MessageType.MSG,
            content: "Hello, World!");

        // Act
        var payload = builder.GetPayloadFromMessage(message);

        // Assert
        var expectedPayload = new List<byte>
        {
            (byte)PayloadType.MSG, // Message type
            0, 0, // ID (initial value)
        };
        expectedPayload.AddRange(Encoding.ASCII.GetBytes("TestDisplayName"));
        expectedPayload.Add(0);
        expectedPayload.AddRange(Encoding.ASCII.GetBytes("Hello, World!"));
        expectedPayload.Add(0);

        Assert.Equal(expectedPayload.ToArray(), payload);
    }
    
    [Fact]
    public void GetPayloadFromMessage_ShouldThrowNotSupportedExceptionRENAME()
    {
        // Arrange
        var builder = new ProtocolPayloadBuilder(new UserModel("TestUser", "TestDisplayName"));
        var message = new MessageModel(
            MessageType.RENAME,
            new Dictionary<string, string>
            {
                { "displayName", "NewDisplayName" }
            });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => builder.GetPayloadFromMessage(message));
    }

    [Fact]
    public void GetPayloadFromMessage_ShouldThrowNotSupportedExceptionHELP()
    {
        // Arrange
        var builder = new ProtocolPayloadBuilder(new UserModel("TestUser", "TestDisplayName"));
        var message = new MessageModel(
            MessageType.HELP);
        
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => builder.GetPayloadFromMessage(message));
    }
}