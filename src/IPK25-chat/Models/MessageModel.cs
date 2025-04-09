using IPK25_chat.Protocol;

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

     
    
    public void Print()
    {
        Console.WriteLine($"MessageType: {MessageType}");
    
        if (Parameters != null && Parameters.Count > 0)
        {
            Console.WriteLine("Parameters:");
            foreach (var param in Parameters)
            {
                Console.WriteLine($"  {param.Key}: {param.Value}");
            }
        }
        else
        {
            Console.WriteLine("Parameters: None");
        }

        if (!string.IsNullOrEmpty(Content))
        {
            Console.WriteLine($"Content: {Content}");
        }
        else
        {
            Console.WriteLine("Content: None");
        }    
    }
}