using System.Text;
using System.Text.RegularExpressions;
using IPK25_chat.Enums;

namespace IPK25_chat.Validators;

public class TcpValidator : IValidator
{
    private List<KeyValuePair<Regex, MessageType>> _validRegexes;

    public TcpValidator()
    {
        const string idRegex = @"[a-zA-Z0-9_-]{1,20}";
        const string secretRegex = @"[a-zA-Z0-9_-]{1,128}";
        const string contentRegex = @"[\x20-\x7E\n]{1,60000}";
        const string displayNameRegex = @"[\x21-\x7E]{1,20}";
        const string authRegex = @"[aA][uU][tT][hH]";
        const string joinRegex = @"[jJ][oO][iI][nN]";
        const string msgRegex = @"[mM][sS][gG]";
        const string errRegex = @"[eE][rR][rR]";
        const string byeRegex = @"[bB][yY][eE]";
        const string replyRegex = @"[rR][eE][pP][lL][yY]";
        const string okRegex = @"[oO][kK]";
        const string nokRegex = @"[nN][oO][kK]";
        const string asRegex = @"[aA][sS]";
        const string usingRegex = @"[uU][sS][iI][nN][gG]";
        const string isRegex = @"[iI][sS]";
        const string fromRegex = @"[fF][rR][oO][mM]";

        _validRegexes = new()
        {
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"^{authRegex}\s{idRegex}\s{asRegex}\s{displayNameRegex}\s{usingRegex}\s{secretRegex}\r\n"), MessageType.AUTH),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"{joinRegex}\s{idRegex}\s{asRegex}\s{displayNameRegex}\r\n"), MessageType.JOIN),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"{msgRegex}\s{fromRegex}\s{displayNameRegex}\s{isRegex}\s{contentRegex}\r\n"), MessageType.MSG),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"{errRegex}\s{fromRegex}\s{displayNameRegex}\s{isRegex}\s{contentRegex}\r\n"), MessageType.ERR),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"{byeRegex}\s{fromRegex}\s{displayNameRegex}\r\n"), MessageType.BYE),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"{replyRegex}\s{okRegex}\s{isRegex}\s{contentRegex}\r\n"), MessageType.REPLY),
            new KeyValuePair<Regex, MessageType>(
                new Regex($@"{replyRegex}\s{nokRegex}\s{isRegex}\s{contentRegex}\r\n"), MessageType.NOTREPLY),
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