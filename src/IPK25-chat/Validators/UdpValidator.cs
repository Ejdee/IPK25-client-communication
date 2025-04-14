using System.Reflection;
using IPK25_chat.Enums;

namespace IPK25_chat.Validators;

public class UdpValidator : IValidator
{
    private const int UsernameLength = 20;
    private const int ChannelIdLength = 20;
    private const int MessageLength = 60000;
    private const int SecretLength = 128; 
    private const int DisplayNameLength = 20;
    private const int ReplyLength = 1;
    private const int RefMessageIdLength = 2;

    private const int CommonHeaderLength = 3;
    private const int ReplyHeaderLength = 5;
    
    public MessageType ValidateAndGetMsgType(byte[] data)
    {
        return data[0] switch
        {
            (byte)PayloadType.AUTH => ValidatePayloadAndReturnType(CommonHeaderLength, data, MessageType.AUTH, UsernameLength, DisplayNameLength, SecretLength),
            (byte)PayloadType.JOIN => ValidatePayloadAndReturnType(CommonHeaderLength, data, MessageType.JOIN, ChannelIdLength, DisplayNameLength),
            (byte)PayloadType.MSG => ValidatePayloadAndReturnType(CommonHeaderLength, data, MessageType.MSG, DisplayNameLength, MessageLength),
            (byte)PayloadType.REPLY => ValidatePayloadAndReturnType(ReplyHeaderLength, data, MessageType.REPLY, MessageLength),
            (byte)PayloadType.PING => ValidatePayloadAndReturnType(CommonHeaderLength, data, MessageType.PING),
            (byte)PayloadType.ERR => ValidatePayloadAndReturnType(CommonHeaderLength, data, MessageType.ERR, DisplayNameLength, MessageLength),
            (byte)PayloadType.BYE => ValidatePayloadAndReturnType(CommonHeaderLength, data, MessageType.BYE, DisplayNameLength),
            (byte)PayloadType.CONFIRM => ValidatePayloadAndReturnType(CommonHeaderLength, data, MessageType.CONFIRM),
            _ => MessageType.INVALID
        };
    }

    private MessageType ValidatePayloadAndReturnType(int headerLength, byte[] data, MessageType type, params int[] partsLength)
    {
        if (!IsValidHeader(data, headerLength)) 
            return MessageType.INVALID;

        if (type == MessageType.REPLY)
            type = GetReplyMessageType(data);
        
        return IsValidPayload(data[headerLength..], partsLength) ? type : MessageType.INVALID;
    }

    private MessageType GetReplyMessageType(byte[] data)
    {
        return data[3] == 0x00 ? MessageType.NOTREPLY : MessageType.REPLY;
    }

    private bool IsValidHeader(byte[] data, int headerLength)
    {
        return data.Length >= headerLength;
    } 
    
    private bool IsValidPayload(byte[] data, params int[] partLengths)
    {
        var partsOfMsg = SplitByteWithNullTerminator(data);
        
        if (partsOfMsg.Count != partLengths.Length)
            return false;

        for (int i = 0; i < partLengths.Length; i++)
        {
            if (partsOfMsg[i].Length > partLengths[i])
                return false;
        }

        return true;
    }

    private static List<byte[]> SplitByteWithNullTerminator(byte[] data)
    {
        var result = new List<byte[]>();

        var tmp = new List<byte>();
        foreach (var t in data)
        {
            if (t == 0)
            {
                result.Add(tmp.ToArray());
                tmp.Clear();
            }
            else
            {
                tmp.Add(t);
            }
        }

        return result;
    }
 
}