namespace IPK25_chat.Enums;

public enum MessageType
{
    INVALID = -1,
    BLANK = 0,
    RENAME = 1,
    AUTH = 2,
    JOIN = 3,
    MSG = 4,
    HELP = 5,
    PING = 6,
    BYE = 7,
    ERR = 8,
    REPLY = 9,
    NOTREPLY = 10,
    CONFIRM = 11,
}