using IPK25_chat.Enums;

namespace IPK25_chat.Validators;

public interface IValidator
{
    public MessageType ValidateAndGetMsgType(byte[] message);
}