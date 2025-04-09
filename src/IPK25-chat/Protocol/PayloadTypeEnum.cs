namespace IPK25_chat.Protocol;

public enum PayloadTypeEnum : byte
{
    CONFIRM = 0x00,
    REPLY = 0x01,
    AUTH = 0x02,
    JOIN = 0x03,
    MSG = 0x04,
    PING = 0xFD,
    ERR = 0xFE,
    BYE = 0xFF,
}