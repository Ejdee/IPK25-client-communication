using IPK25_chat.Enums;
using IPK25_chat.Models;

namespace IPK25_chat.Protocol;

public interface IProtocolPayloadBuilder
{
    byte[] GetPayloadFromMessage(MessageModel message);
    byte[] CreatePayload(MessageType type, params string[] parameters);
    byte[] CreateByePacket();
    byte[] CreateErrPacket(byte[]? content);
}