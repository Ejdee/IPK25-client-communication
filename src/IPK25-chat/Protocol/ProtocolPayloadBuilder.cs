using System.Text;
using IPK25_chat.Models;

namespace IPK25_chat.Protocol;

public class ProtocolPayloadBuilder
{
    private int _id = 0;
    private string _displayName;

    public string DisplayName
    {
        get => _displayName;
        set => _displayName = value;
    }
    
    public ProtocolPayloadBuilder(string displayName)
    {
        _displayName = displayName;
    }

    public byte[] GetPayloadFromMessage(MessageModel message)
    {
        switch (message.MessageType)
        {
            case MessageType.AUTH:
                return CreatePayload(message.MessageType, message.Parameters["username"], message.Parameters["displayName"], message.Parameters["secret"]);
            case MessageType.JOIN:
                return CreatePayload(message.MessageType, message.Parameters["channelID"], _displayName);
            case MessageType.MSG:
                return CreatePayload(message.MessageType, message.Content);
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
            MessageType.AUTH => (byte)PayloadTypeEnum.AUTH,
            MessageType.JOIN => (byte)PayloadTypeEnum.JOIN,
            MessageType.MSG => (byte)PayloadTypeEnum.MSG,
            _ => throw new ArgumentOutOfRangeException(nameof(messageMessageType), messageMessageType, null)
        };
    }
}