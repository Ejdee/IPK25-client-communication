using System.Text;
using IPK25_chat.Enums;
using IPK25_chat.Models;
using IPK25_chat.Protocol;

namespace IPK25_chat.Tests
{
    public class TcpProtocolPayloadBuilderTests
    {
        private readonly UserModel _testUser;
        private readonly TcpProtocolPayloadBuilder _builder;

        public TcpProtocolPayloadBuilderTests()
        {
            _testUser = new UserModel ("TestUser", "TestDisplayName");
            _builder = new TcpProtocolPayloadBuilder(_testUser);
        }

        [Fact]
        public void GetPayloadFromMessage_Auth_ReturnsCorrectPayload()
        {
            // Arrange
            var message = new MessageModel (
                MessageType.AUTH,
                new Dictionary<string, string>
                {
                    { "username", "testname" },
                    { "displayName", _testUser.DisplayName },
                    { "secret", "someSecret" }
                }
            );
            var expected = $"AUTH testname AS {_testUser.DisplayName} USING someSecret\r\n";
            
            // Act
            var result = _builder.GetPayloadFromMessage(message);
            var resultString = Encoding.ASCII.GetString(result);
            
            // Assert
            Assert.Equal(expected, resultString);
        }
        
        [Fact]
        public void GetPayloadFromMessage_Join_ReturnsCorrectPayload()
        {
            // Arrange
            var message = new MessageModel(
                MessageType.JOIN,
                new Dictionary<string, string>()
                {
                    { "channelID", "general" }
                }
            );
            var expected = $"JOIN general AS {_testUser.DisplayName}\r\n";
            
            // Act
            var result = _builder.GetPayloadFromMessage(message);
            var resultString = Encoding.ASCII.GetString(result);
            
            // Assert
            Assert.Equal(expected, resultString);
        }
        
        [Fact]
        public void GetPayloadFromMessage_Msg_ReturnsCorrectPayload()
        {
            // Arrange
            var message = new MessageModel (
                MessageType.MSG,
                new Dictionary<string, string>(),
                "Hello, world!"
            );
            var expected = $"MSG FROM {_testUser.DisplayName} IS Hello, world!\r\n";
            
            // Act
            var result = _builder.GetPayloadFromMessage(message);
            var resultString = Encoding.ASCII.GetString(result);
            
            // Assert
            Assert.Equal(expected, resultString);
        }

        [Fact]
        public void CreateByePacket_ReturnsCorrectFormat()
        {
            // Arrange
            var expected = $"BYE FROM {_testUser.DisplayName}\r\n";
            
            // Act
            var result = _builder.CreateByePacket();
            var resultString = Encoding.ASCII.GetString(result);
            
            // Assert
            Assert.Equal(expected, resultString);
        }

        [Fact]
        public void CreateErrPacket_WithContent_ReturnsCorrectFormat()
        {
            // Arrange
            var errorContent = "Connection failed";
            var contentBytes = Encoding.ASCII.GetBytes(errorContent);
            var expected = $"ERR FROM {_testUser.DisplayName} IS {errorContent}\r\n";
            
            // Act
            var result = _builder.CreateErrPacket(contentBytes);
            var resultString = Encoding.ASCII.GetString(result);
            
            // Assert
            Assert.Equal(expected, resultString);
        }
        
        [Fact]
        public void CreateErrPacket_WithoutContent_ReturnsCorrectFormat()
        {
            // Arrange
            var expected = $"ERR FROM {_testUser.DisplayName} IS \r\n";
            
            // Act
            var result = _builder.CreateErrPacket(null);
            var resultString = Encoding.ASCII.GetString(result);
            
            // Assert
            Assert.Equal(expected, resultString);
        }
        
        [Fact]
        public void GetPayloadFromMessage_UnsupportedType_ThrowsException()
        {
            // Arrange
            var message = new MessageModel((MessageType)99);
            
            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _builder.GetPayloadFromMessage(message));
        }
        
        [Fact]
        public void CreatePayload_UnsupportedType_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _builder.CreatePayload((MessageType)99));
        }
    }
}