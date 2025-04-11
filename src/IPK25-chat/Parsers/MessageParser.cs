using IPK25_chat.Enums;
using IPK25_chat.Logger;
using IPK25_chat.Models;

namespace IPK25_chat.Parsers;

public class MessageParser
{
    private readonly InputValidator _inputValidator;
    private readonly ResultLogger _logger;
    private readonly UserModel _user;

    public MessageParser(InputValidator inputValidator, ResultLogger logger, UserModel user)
    {
        _inputValidator = inputValidator;
        _logger = logger;
        _user = user;
    }

    public bool ParseMessage(string message, out MessageModel messageModel)
    {
        messageModel = new MessageModel(MessageType.MSG);
        
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
                    
                    _user.Username = messageModel.Parameters["username"];
                    _user.DisplayName = messageModel.Parameters["displayName"];
                    return true;
                }
                return false;
            }

            if (message.StartsWith("/join"))
            {
                messageModel.MessageType = MessageType.JOIN;
                string[] parameters = message.Split(" ").Skip(1).ToArray();
                if (parameters.Length == 1)
                {
                    messageModel.Parameters = new Dictionary<string, string>
                    {
                        { "channelID", _inputValidator.ValidateInput(parameters[0], 20, "[a-zA-Z0-9_-]") }
                    };
                    return true;
                }

                return false;
            }

            if (message.StartsWith("/rename"))
            {
                messageModel.MessageType = MessageType.RENAME;
                string[] parameters = message.Split(" ").Skip(1).ToArray();
                if (parameters.Length == 1)
                {
                    messageModel.Parameters = new Dictionary<string, string>
                    {
                        { "displayName", _inputValidator.ValidateInput(parameters[0], 20, "[\\x21-\\x7E]") }
                    };
                     
                    _user.DisplayName = messageModel.Parameters["displayName"];
                    return true;
                }

                return false;
            }

            if (message.StartsWith("/help"))
            {
                messageModel.MessageType = MessageType.HELP;
                string[] parameters = message.Split(" ").Skip(1).ToArray();
                if (parameters.Length != 0)
                {
                    return false;
                }

                _logger.PrintHelp();
                return true;
            }

            return false;
        } 
        
        messageModel.MessageType = MessageType.MSG;
        messageModel.Content = _inputValidator.ValidateInput(message, 60000, "[\\x0A-\\x7E]");

        return true;
    }
}