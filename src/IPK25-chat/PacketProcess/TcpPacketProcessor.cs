using System.Text;

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
            Console.WriteLine($"Invalid error packet: {BitConverter.ToString(data)}");
            return;
        }

        var errorContent = string.Join(' ', msgParts[4..]);
        Console.WriteLine($"ERROR: {errorContent}");
    }

    protected override void ProcessByePacket(byte[] data)
    {
        // No action required, program will terminate in FSM
        return;
    }

    protected override void ProcessMessagePacket(byte[] data)
    {
        var msgParts = Encoding.ASCII.GetString(data).Split(' ');
        if (msgParts.Length < MessageMsgMinWords)
        {
            Console.WriteLine($"Invalid message packet: {BitConverter.ToString(data)}");
            return;
        }

        var displayName = msgParts[2];
        var messageContent = string.Join(' ', msgParts[4..]);
        Console.WriteLine($"{displayName}: {messageContent}");
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
        Console.WriteLine($"Action {(result == "OK" ? "Success" : "Failure")}: {content}");
    }

    protected override void ProcessPingPacket(byte[] data)
    {
        // No action required for TCP
        return;
    }

    protected override void ProcessConfirmationPacket(byte[] data)
    {
        // No action required for TCP
        return;
    }
}