using System.Text;
using TcpListener = IPK25_chat.Client.TcpListener;

namespace IPK25_chat.Tests;

public class TcpListenerTests
{
    [Fact]
    public void TryExtractMessage_ShouldExtractWholeMessage()
    {
        // Arrange
        var tcpListener = new TcpListener(null!);
        var message = "AUTH TestUser AS TestDisplayName USING TestSecret \r\n";
        var messageBuffer = new List<byte>(System.Text.Encoding.ASCII.GetBytes(message));
        
        // Act
        tcpListener.TryExtractMessage(messageBuffer, out var extractedMessage);
        
        // Assert
        Assert.Equal(Encoding.ASCII.GetBytes(message), extractedMessage);
        Assert.Empty(messageBuffer);
    }
    
    [Fact]
    public void TryExtractMessage_ShouldExtractPartialMessage()
    {
        // Arrange
        var tcpListener = new TcpListener(null!);
        var messageToEqual = "AUTH TestUser AS TestDisplayName USING TestSecret \r\n";
        var message = messageToEqual + "remaining";
        var messageBuffer = new List<byte>(System.Text.Encoding.ASCII.GetBytes(message));
        
        // Act
        tcpListener.TryExtractMessage(messageBuffer, out var extractedMessage);
        
        // Assert
        Assert.Equal(Encoding.ASCII.GetBytes(messageToEqual), extractedMessage);
        Assert.Equal(messageBuffer, Encoding.ASCII.GetBytes("remaining"));
    }
    
    [Fact]
    public void TryExtractMessage_ShouldNotExtractWhenNoCRLF()
    {
        // Arrange
        var tcpListener = new TcpListener(null!);
        var message = "AUTH TestUser AS TestDisplayName USING TestSecret";
        var messageBuffer = new List<byte>(System.Text.Encoding.ASCII.GetBytes(message));
        
        // Act
        tcpListener.TryExtractMessage(messageBuffer, out var extractedMessage);
        
        // Assert
        Assert.Empty(extractedMessage);
        Assert.Equal(messageBuffer, Encoding.ASCII.GetBytes(message));
    }

    [Fact]
    public void TryExtractMessage_ShouldExtractTwoMessages()
    {
        // Arrange
        var tcpListener = new TcpListener(null!);
        var message1 = "message1\r\nmessage2\r\n";
        var message1ToEqual = "message1\r\n";
        var message2ToEqual = "message2\r\n";
        var messageBuffer = new List<byte>(System.Text.Encoding.ASCII.GetBytes(message1));
        
        // Act
        var i = 0;
        while (tcpListener.TryExtractMessage(messageBuffer, out var extractedMessage))
        {
            // Assert
            Assert.NotEmpty(extractedMessage);
            Assert.True(extractedMessage.SequenceEqual(Encoding.ASCII.GetBytes(message1ToEqual)) ||
                          extractedMessage.SequenceEqual(Encoding.ASCII.GetBytes(message2ToEqual)));
            i++;
        }
        Assert.Equal(2, i);
        Assert.Empty(messageBuffer);
    }
}