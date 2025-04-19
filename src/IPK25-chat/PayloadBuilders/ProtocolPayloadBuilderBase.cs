using IPK25_chat.Enums;
using IPK25_chat.Models;

namespace IPK25_chat.PayloadBuilders;

public abstract class ProtocolPayloadBuilderBase: IProtocolPayloadBuilder
{
    private readonly UserModel _user;

    protected ProtocolPayloadBuilderBase(UserModel user)
    {
        _user = user;
    }

    public byte[] GetPayloadFromMessage(MessageModel message)
    {
        switch (message.MessageType)
        {
            case MessageType.AUTH:
                if (message.Parameters == null) 
                    throw new ArgumentNullException(nameof(message.Parameters));
                
                return CreatePayload(message.MessageType, message.Parameters["username"], _user.DisplayName,
                    message.Parameters["secret"]);
            case MessageType.JOIN:
                if (message.Parameters == null) 
                    throw new ArgumentNullException(nameof(message.Parameters));
                
                return CreatePayload(message.MessageType, message.Parameters["channelID"], _user.DisplayName);
            case MessageType.MSG:
                if (message.Content == null)
                    throw new ArgumentNullException(nameof(message.Parameters));
                
                return CreatePayload(message.MessageType, _user.DisplayName, message.Content);
            default:
                throw new NotSupportedException();
        }
    }

    public abstract byte[] CreatePayload(MessageType type, params string[] parameters);
    public abstract byte[] CreateByePacket();

    public abstract byte[] CreateErrPacket(byte[]? content);
}