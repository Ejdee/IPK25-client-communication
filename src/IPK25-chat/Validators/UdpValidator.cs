using IPK25_chat.Enums;

namespace IPK25_chat.Validators;

public class UdpValidator : IValidator
{
    public MessageType ValidateAndGetMsgType(byte[] data)
    {
        return data[0] switch
        {
            (byte)PayloadType.AUTH => MessageType.AUTH,
            (byte)PayloadType.JOIN => MessageType.JOIN,
            (byte)PayloadType.MSG => MessageType.MSG,
            (byte)PayloadType.REPLY => data[3] != 0 ? MessageType.REPLY : MessageType.NOTREPLY,
            (byte)PayloadType.PING => MessageType.PING,
            (byte)PayloadType.ERR => MessageType.ERR,
            (byte)PayloadType.BYE => MessageType.BYE,
            (byte)PayloadType.CONFIRM => MessageType.CONFIRM,
            _ => MessageType.INVALID
        };
    }
 
}