using System.Text;
using IPK25_chat.Enums;
using IPK25_chat.Models;

namespace IPK25_chat.PayloadBuilders;

public class UdpProtocolPayloadBuilder : ProtocolPayloadBuilderBase
{
    private int _id = 0;
    private readonly UserModel _user;

    public UdpProtocolPayloadBuilder(UserModel user) : base(user)
    {
        _user = user;
    }

    public override byte[] CreatePayload(MessageType type, params string[] parameters)
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

    public override byte[] CreateByePacket()
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

    public override byte[] CreateErrPacket(byte[]? content)
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