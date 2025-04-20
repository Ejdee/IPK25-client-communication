using System.Text;
using System.Text.RegularExpressions;

namespace IPK25_chat.PacketProcess;

public class TcpPacketProcessor : PacketProcessorBase
{
    private const int ErrorMsgMinWords = 5;
    private const int ReplyMsgWords = 4;
    private const int MessageMsgMinWords = 5;

    protected override void ProcessErrorPacket(byte[] data)
    {
        var msgParts = Encoding.ASCII.GetString(data).Split(' ');
        if (msgParts.Length < ErrorMsgMinWords)
        {
            Console.WriteLine($"ERROR: Invalid error packet: {BitConverter.ToString(data)}");
            return;
        }

        // since the processing is performed on valid messages, we can
        // safely access the parts of the message hardcoded
        var errorContent = string.Join(' ', msgParts[4..]);
        Console.Write($"ERROR FROM {msgParts[2]}: {errorContent}");
    }

    protected override void ProcessByePacket(byte[] data)
    {
        // No action required, program will terminate in FSM
    }

    protected override void ProcessMessagePacket(byte[] data)
    {
        var msgParts = Encoding.ASCII.GetString(data).Split(' ');
        if (msgParts.Length < MessageMsgMinWords)
        {
            Console.WriteLine($"ERROR: Invalid message packet: {BitConverter.ToString(data)}");
            return;
        }

        var displayName = msgParts[2];
        var messageContent = string.Join(' ', msgParts[4..]);
        Console.Write($"{displayName}: {messageContent}");
    }

    protected override void ProcessReplyPacket(byte[] data)
    {
        var msgParts = Encoding.ASCII.GetString(data).Split(' ');
        if (msgParts.Length < ReplyMsgWords)
        {
            Console.WriteLine($"Invalid reply packet: {BitConverter.ToString(data)}");
            return;
        }

        var result = msgParts[1];
        var content = string.Join(' ', msgParts[3..]);
        var okRegex = new Regex(@"^[oO][kK]$");
        Console.Write($"Action {(okRegex.IsMatch(result) ? "Success" : "Failure")}: {content}");
    }

    protected override void ProcessPingPacket(byte[] data)
    {
        // No action required for TCP
    }

    protected override void ProcessConfirmationPacket(byte[] data)
    {
        // No action required for TCP
    }
}