using IPK25_chat.Enums;

namespace IPK25_chat.Models;

public class MessageModel
{
    private MessageType _messageType;
    private Dictionary<string, string>? _parameters;
    private string? _content;

    public MessageType MessageType
    {
        get => _messageType;
        set => _messageType = value;
    }
    
    public Dictionary<string, string>? Parameters
    {
        get => _parameters;
        set => _parameters = value;
    }
    
    public string? Content
    {
        get => _content;
        set => _content = value;
    }
    
    public MessageModel(MessageType messageType, Dictionary<string, string>? parameters = null, string? content = null)
    {
        _parameters = parameters;
        _messageType = messageType;
        _content = content;
    }

}