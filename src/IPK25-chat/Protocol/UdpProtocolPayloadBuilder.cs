using System.Text;
using IPK25_chat.Enums;
using IPK25_chat.Models;

namespace IPK25_chat.Protocol;

public class UdpProtocolPayloadBuilder
{
    private int _id = 0;
    private readonly UserModel _user;
    
    public UdpProtocolPayloadBuilder(UserModel user)
    {
        _user = user;
    }

    public byte[] GetPayloadFromMessage(MessageModel message)
    {
        switch (message.MessageType)
        {
            case MessageType.AUTH:
                return CreatePayload(message.MessageType, message.Parameters["username"], _user.DisplayName, message.Parameters["secret"]);
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
        var payload = new List<byte>();
        
        payload.Add(CreateType(type));
        
        ushort id = (ushort) _id++;
        var idBytes = BitConverter.GetBytes(id);
        if (BitConverter.IsLittleEndian) { Array.Reverse(idBytes); }
        
        payload.Add(idBytes[0]);
        payload.Add(idBytes[1]);
        
        foreach (var parameter in parameters)
        {
            //TODO: idk about encoding
            payload.AddRange(Encoding.ASCII.GetBytes(parameter));
            payload.Add(0);
        }

        return payload.ToArray();
    }

    private byte CreateType(MessageType messageMessageType)
    {
        return messageMessageType switch
        {
            MessageType.AUTH => (byte)PayloadType.AUTH,
            MessageType.JOIN => (byte)PayloadType.JOIN,
            MessageType.MSG => (byte)PayloadType.MSG,
            _ => throw new ArgumentOutOfRangeException(nameof(messageMessageType), messageMessageType, null)
        };
    }

    public byte[] CreateByePacket()
    {
        var payload = new List<byte>();
        payload.Add((byte)PayloadType.BYE);
        
        ushort id = (ushort) _id++;
        var idBytes = BitConverter.GetBytes(id);
        if (BitConverter.IsLittleEndian) { Array.Reverse(idBytes); }
        
        payload.Add(idBytes[0]);
        payload.Add(idBytes[1]);
        
        payload.AddRange(Encoding.ASCII.GetBytes(_user.DisplayName));
        payload.Add(0);
        
        return payload.ToArray();
    }

    public byte[] CreateErrPacket(byte[]? content)
    {
        var payload = new List<byte>();
        payload.Add((byte)PayloadType.ERR);
        
        ushort id = (ushort) _id++;
        var idBytes = BitConverter.GetBytes(id);
        if (BitConverter.IsLittleEndian) { Array.Reverse(idBytes); }
        
        payload.Add(idBytes[0]);
        payload.Add(idBytes[1]);
        
        payload.AddRange(Encoding.ASCII.GetBytes(_user.DisplayName));
        payload.Add(0);

        if (content != null)
        {
            payload.AddRange(content);
            payload.Add(0);
        }

        return payload.ToArray();
    }
}