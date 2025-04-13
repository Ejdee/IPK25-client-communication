using System.Text;
using IPK25_chat.Enums;
using IPK25_chat.Models;

namespace IPK25_chat.Protocol;

public class TcpProtocolPayloadBuilder : IProtocolPayloadBuilder
{
    private readonly UserModel _user;
    
    public TcpProtocolPayloadBuilder(UserModel user)
    {
        _user = user;
    }

    public byte[] GetPayloadFromMessage(MessageModel message)
    {
        // Current implementation for UDP
        switch (message.MessageType)
        {
            case MessageType.AUTH:
                return CreatePayload(message.MessageType, message.Parameters["username"], _user.DisplayName,
                    message.Parameters["secret"]);
            case MessageType.JOIN:
                return CreatePayload(message.MessageType, message.Parameters["channelID"], _user.DisplayName);
            case MessageType.MSG:
                return CreatePayload(message.MessageType, _user.DisplayName, message.Content);
            default:
                throw new NotSupportedException();
        }
    }

    public byte[] CreatePayload(MessageType type, params string[] parameters)
    {
        string payload;
        switch (type)
        {
            case MessageType.AUTH:
                payload = $"AUTH {parameters[0]} AS {parameters[1]} USING {parameters[2]}\r\n";
                break;
            case MessageType.JOIN:
                payload = $"JOIN {parameters[0]} AS {parameters[1]}\r\n";
                break;
            case MessageType.MSG:
                payload = $"MSG FROM {parameters[0]} IS {parameters[1]}\r\n";
                break;
            default:
                throw new NotSupportedException();
        }
        return Encoding.ASCII.GetBytes(payload);
    }

    public byte[] CreateByePacket()
    {
        return Encoding.ASCII.GetBytes("BYE FROM " + _user.DisplayName + "\r\n");
    }

    public byte[] CreateErrPacket(byte[]? content)
    {
        string errorMessage = content != null ? Encoding.ASCII.GetString(content) : "";
        return Encoding.ASCII.GetBytes("ERR FROM " + _user.DisplayName + " IS " + errorMessage + "\r\n");
    }
}