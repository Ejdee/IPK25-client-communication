using System.Text;
using IPK25_chat.Client;

namespace IPK25_chat.PacketProcess;

public class UdpPacketProcessor : PacketProcessorBase
{
    private readonly ConfirmationTracker _confirmationTracker;
    
    public UdpPacketProcessor(ConfirmationTracker confirmationTracker)
    {
        _confirmationTracker = confirmationTracker;
    }
    
    protected override void ProcessConfirmationPacket(byte[] data)
    {
        var refId = $"{data[1]}{data[2]}";
        _confirmationTracker.SetConfirmation(refId);
    }

    protected override void ProcessErrorPacket(byte[] data)
    {
        var displayNameOffset = 3;
        var displayName = ExtractContent(data, displayNameOffset);
        
        var errorOffset = displayNameOffset + displayName.Count + 1;
        var errorMessage = ExtractContent(data, errorOffset);
        Console.WriteLine(
            $"ERROR FROM {Encoding.ASCII.GetString(displayName.ToArray())}: {Encoding.ASCII.GetString(errorMessage.ToArray())}");
    }

    protected override void ProcessByePacket(byte[] data)
    {
    }

    protected override void ProcessPingPacket(byte[] data)
    {
    }
    
    protected override void ProcessMessagePacket(byte[] data)
    {
        const int displayNameOffset = 3;
        var displayName = ExtractContent(data, 3);
        
        var messageOffset = displayNameOffset + displayName.Count + 1;
        var message = ExtractContent(data, messageOffset);

        Console.WriteLine(
            $"{Encoding.ASCII.GetString(displayName.ToArray())}: {Encoding.ASCII.GetString(message.ToArray())}");
    }

    protected override void ProcessReplyPacket(byte[] data)
    {
        var result = data[3];
        const int contentOffset = 6;
        var content = ExtractContent(data, contentOffset);
        Console.WriteLine($"Action {(result != 0 ? "Success" : "Failure")}: {Encoding.ASCII.GetString(content.ToArray())}");
    }
    
    private static List<byte> ExtractContent(byte[] data, int offset)
    {
        var content = new List<byte>();
        for (int i = offset; data[i] != 0; i++)
        {
            content.Add(data[i]);
        }

        return content;
    }
}