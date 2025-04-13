using System.Text;
using System.Text.RegularExpressions;
using IPK25_chat.Enums;

namespace IPK25_chat.Validators;

public class TcpValidator : IValidator
{
    private List<KeyValuePair<Regex, MessageType>> _validRegexes;

    public TcpValidator()
    {
        var idRegex = @"[a-zA-Z0-9_-]{1,20}";
        var secretRegex = @"[a-zA-Z0-9_-]{1,128}";
        var contentRegex = @"[\x20-\x7E\n]{1,60000}";
        var displayNameRegex = @"[\x21-\x7E]{1,20}";

        _validRegexes = new()
        {
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"^AUTH\s{idRegex}\sAS\s{displayNameRegex}\sUSING\s{secretRegex}\r\n"), MessageType.AUTH),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"JOIN\s{idRegex}\sAS\s{displayNameRegex}\r\n"), MessageType.JOIN),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"MSG\sFROM\s{displayNameRegex}\sIS\s{contentRegex}\r\n"), MessageType.MSG),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"ERR\sFROM\s{displayNameRegex}\sIS\s{contentRegex}\r\n"), MessageType.ERR),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"BYE\sFROM\s{displayNameRegex}\r\n"), MessageType.BYE),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"REPLY\sOK\sIS\s{contentRegex}\r\n"), MessageType.REPLY),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"REPLY\sNOK\sIS\s{contentRegex}\r\n"), MessageType.NOTREPLY),
        };
    }
    
    public MessageType ValidateAndGetMsgType(byte[] message)
    {
        foreach (var regex in _validRegexes)
        {
            if (regex.Key.IsMatch(Encoding.ASCII.GetString(message)))
            {
                return regex.Value;
            }
        }

        return MessageType.INVALID;
    }
}