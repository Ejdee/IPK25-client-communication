using IPK25_chat.Enums;
using IPK25_chat.InputParse;
using IPK25_chat.Models;

namespace IPK25_chat.Tests;

public class MessageParserTests
{
    private readonly MessageParser _messageParser;

    public MessageParserTests()
    {
        var inputValidator = new InputValidator();
        var user = new UserModel("testUser", "Test User");
        _messageParser = new MessageParser(inputValidator, user);
    }

    [Fact]
    public void ParseMessage_ShouldParseAuthMessage()
    {
        // Arrange
        string message = "/auth username secret displayName";

        // Act
        var result = _messageParser.ParseMessage(message, out var parsedMessage);

        // Assert
        Assert.True(result);
        Assert.NotNull(parsedMessage);
        Assert.Equal(MessageType.AUTH, parsedMessage.MessageType);
        Assert.NotNull(parsedMessage.Parameters);
        Assert.Equal("username", parsedMessage.Parameters["username"]);
        Assert.Equal("secret", parsedMessage.Parameters["secret"]);
        Assert.Equal("displayName", parsedMessage.Parameters["displayName"]);
    }

    [Fact]
    public void ParseMessage_ShouldParseJoinMessage()
    {
        // Arrange
        string message = "/join channelID";

        // Act
        var result = _messageParser.ParseMessage(message, out var parsedMessage);

        // Assert
        Assert.True(result);
        Assert.NotNull(parsedMessage);
        Assert.Equal(MessageType.JOIN, parsedMessage.MessageType);
        Assert.NotNull(parsedMessage.Parameters);
        Assert.Equal("channelID", parsedMessage.Parameters["channelID"]);
    }

    [Fact]
    public void ParseMessage_ShouldParseRenameMessage()
    {
        // Arrange
        string message = "/rename newDisplayName";

        // Act
        var result = _messageParser.ParseMessage(message, out var parsedMessage);

        // Assert
        Assert.True(result);
        Assert.NotNull(parsedMessage);
        Assert.Equal(MessageType.RENAME, parsedMessage.MessageType);
        Assert.NotNull(parsedMessage.Parameters);
        Assert.Equal("newDisplayName", parsedMessage.Parameters["displayName"]);
    }

    [Fact]
    public void ParseMessage_ShouldParseHelpMessage()
    {
        // Arrange
        string message = "/help";

        // Act
        var result = _messageParser.ParseMessage(message, out var parsedMessage);

        // Assert
        Assert.True(result);
        Assert.NotNull(parsedMessage);
        Assert.Equal(MessageType.HELP, parsedMessage.MessageType);
        Assert.Null(parsedMessage.Parameters);
    }

    [Fact]
    public void ParseMessage_ShouldParseRegularMessage()
    {
        // Arrange
        string message = "This is a regular message.";

        // Act
        var result = _messageParser.ParseMessage(message, out var parsedMessage);

        // Assert
        Assert.True(result);
        Assert.NotNull(parsedMessage);
        Assert.Equal(MessageType.MSG, parsedMessage.MessageType);
        Assert.Equal("This is a regular message.", parsedMessage.Content);
    }

    [Fact]
    public void ParseMessage_ShouldReturnFalseForInvalidAuthMessage()
    {
        // Arrange
        string message = "/auth username";

        // Act
        var result = _messageParser.ParseMessage(message, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ParseMessage_ShouldReturnFalseForInvalidJoinMessage()
    {
        // Arrange
        string message = "/join";

        // Act
        var result = _messageParser.ParseMessage(message, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ParseMessage_ShouldReturnFalseForInvalidRenameMessage()
    {
        // Arrange
        string message = "/rename";

        // Act
        var result = _messageParser.ParseMessage(message, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ParseMessage_ShouldReturnFalseForInvalidHelpMessage()
    {
        // Arrange
        string message = "/help extra";

        // Act
        var result = _messageParser.ParseMessage(message, out _);

        // Assert
        Assert.False(result);
    }
}