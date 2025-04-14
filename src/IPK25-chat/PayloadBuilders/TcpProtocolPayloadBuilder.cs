using System.Text;
using IPK25_chat.Enums;
using IPK25_chat.Models;

namespace IPK25_chat.PayloadBuilders;

public class TcpProtocolPayloadBuilder : ProtocolPayloadBuilderBase, IProtocolPayloadBuilder
{
    private readonly UserModel _user;
    
    public TcpProtocolPayloadBuilder(UserModel user) : base(user)
    {
        _user = user;
    }

    public override byte[] CreatePayload(MessageType type, params string[] parameters)
    {
        string payload = type switch
        {
            MessageType.AUTH => $"AUTH {parameters[0]} AS {parameters[1]} USING {parameters[2]}\r\n",
            MessageType.JOIN => $"JOIN {parameters[0]} AS {parameters[1]}\r\n",
            MessageType.MSG => $"MSG FROM {parameters[0]} IS {parameters[1]}\r\n",
            _ => throw new NotSupportedException()
        };
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