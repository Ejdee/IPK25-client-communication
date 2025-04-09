using IPK25_chat.Models;

namespace IPK25_chat.Parsers;

public class MessageParser
{
    private readonly InputValidator _inputValidator;

    public MessageParser(InputValidator inputValidator)
    {
        _inputValidator = inputValidator;
    }

    public MessageModel ParseMessage(string message)
    {
        MessageModel messageModel = new MessageModel(MessageType.MSG);
        
        if (message.StartsWith("/"))
        {
            if (message.StartsWith("/auth"))
            {
                messageModel.MessageType = MessageType.AUTH;

                string[] parameters = message.Split(" ").Skip(1).ToArray();
                if (parameters.Length == 3)
                {
                    messageModel.Parameters = new Dictionary<string, string>
                    {
                        { "username", _inputValidator.ValidateInput(parameters[0], 20, "[a-zA-Z0-9_-]") },
                        { "secret", _inputValidator.ValidateInput(parameters[1], 128, "[a-zA-Z0-9_-]") },
                        { "displayName", _inputValidator.ValidateInput(parameters[2], 20, "[\\x21-\\x7E]") }
                    };
                }
                else
                {
                    throw new ArgumentException("Invalid AUTH command. Expected format: /auth <username> <secret> <displayName>");
                }
            } else if (message.StartsWith("/join"))
            {
                messageModel.MessageType = MessageType.JOIN;
                string[] parameters = message.Split(" ").Skip(1).ToArray();
                if (parameters.Length == 1)
                {
                    messageModel.Parameters = new Dictionary<string, string>
                    {
                        { "channelID", _inputValidator.ValidateInput(parameters[0], 20, "[a-zA-Z0-9_-]") }
                    };
                }
                else
                {
                    throw new ArgumentException("Invalid JOIN command. Expected format: /join <channelID>");
                }
            } else if (message.StartsWith("/rename"))
            {
                messageModel.MessageType = MessageType.RENAME;    
                string[] parameters = message.Split(" ").Skip(1).ToArray();
                if (parameters.Length == 1)
                {
                    messageModel.Parameters = new Dictionary<string, string>
                    {
                        { "displayName", _inputValidator.ValidateInput(parameters[0], 20, "[\\x21-\\x7E]") }
                    };
                }
                else
                {
                    throw new ArgumentException("Invalid RENAME command. Expected format: /rename <newDisplayName>");
                }
            } else if (message.StartsWith("/help"))
            {
                messageModel.MessageType = MessageType.HELP;
                string[] parameters = message.Split(" ").Skip(1).ToArray();
                if (parameters.Length != 0)
                {
                    throw new ArgumentException("Invalid HELP command. Expected format: /help");
                }
            }
        } 
        else
        {
            messageModel.MessageType = MessageType.MSG;
            messageModel.Content = _inputValidator.ValidateInput(message, 60000, "[\\x0A-\\x7E]");
        }
        
        // Print the parsed message for debugging
        messageModel.Print();
        return messageModel;
    }
}