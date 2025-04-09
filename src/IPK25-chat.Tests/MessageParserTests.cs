using IPK25_chat.Models;
using IPK25_chat.Parsers;

namespace IPK25_chat.Tests;

public class MessageParserTests
{
    private readonly MessageParser _messageParser;

    public MessageParserTests()
    {
        var inputValidator = new InputValidator();
        _messageParser = new MessageParser(inputValidator);
    }

    [Fact]
    public void ParseMessage_ShouldParseAuthMessage()
    {
        // Arrange
        string message = "/auth username secret displayName";

        // Act
        var result = _messageParser.ParseMessage(message);

        // Assert
        Assert.Equal(MessageType.AUTH, result.MessageType);
        Assert.NotNull(result.Parameters);
        Assert.Equal("username", result.Parameters["username"]);
        Assert.Equal("secret", result.Parameters["secret"]);
        Assert.Equal("displayName", result.Parameters["displayName"]);
    }

    [Fact]
    public void ParseMessage_ShouldParseJoinMessage()
    {
        // Arrange
        string message = "/join channelID";

        // Act
        var result = _messageParser.ParseMessage(message);

        // Assert
        Assert.Equal(MessageType.JOIN, result.MessageType);
        Assert.NotNull(result.Parameters);
        Assert.Equal("channelID", result.Parameters["channelID"]);
    }

    [Fact]
    public void ParseMessage_ShouldParseRenameMessage()
    {
        // Arrange
        string message = "/rename newDisplayName";

        // Act
        var result = _messageParser.ParseMessage(message);

        // Assert
        Assert.Equal(MessageType.RENAME, result.MessageType);
        Assert.NotNull(result.Parameters);
        Assert.Equal("newDisplayName", result.Parameters["displayName"]);
    }

    [Fact]
    public void ParseMessage_ShouldParseHelpMessage()
    {
        // Arrange
        string message = "/help";

        // Act
        var result = _messageParser.ParseMessage(message);

        // Assert
        Assert.Equal(MessageType.HELP, result.MessageType);
        Assert.Null(result.Parameters);
    }

    [Fact]
    public void ParseMessage_ShouldParseRegularMessage()
    {
        // Arrange
        string message = "This is a regular message.";

        // Act
        var result = _messageParser.ParseMessage(message);

        // Assert
        Assert.Equal(MessageType.MSG, result.MessageType);
        Assert.Equal("This is a regular message.", result.Content);
    }

    [Fact]
    public void ParseMessage_ShouldThrowExceptionForInvalidAuthMessage()
    {
        // Arrange
        string message = "/auth username";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _messageParser.ParseMessage(message));
        Assert.Equal("Invalid AUTH command. Expected format: /auth <username> <secret> <displayName>", exception.Message);
    }

    [Fact]
    public void ParseMessage_ShouldThrowExceptionForInvalidJoinMessage()
    {
        // Arrange
        string message = "/join";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _messageParser.ParseMessage(message));
        Assert.Equal("Invalid JOIN command. Expected format: /join <channelID>", exception.Message);
    }

    [Fact]
    public void ParseMessage_ShouldThrowExceptionForInvalidRenameMessage()
    {
        // Arrange
        string message = "/rename";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _messageParser.ParseMessage(message));
        Assert.Equal("Invalid RENAME command. Expected format: /rename <newDisplayName>", exception.Message);
    }

    [Fact]
    public void ParseMessage_ShouldThrowExceptionForInvalidHelpMessage()
    {
        // Arrange
        string message = "/help extra";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _messageParser.ParseMessage(message));
        Assert.Equal("Invalid HELP command. Expected format: /help", exception.Message);
    }
}