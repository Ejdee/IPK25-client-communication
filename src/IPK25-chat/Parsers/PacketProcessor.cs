using System.Text;
using IPK25_chat.Client;
using IPK25_chat.Core;
using IPK25_chat.Enums;

namespace IPK25_chat.Parsers;

public class PacketProcessor
{
    private readonly ConfirmationTracker _confirmationTracker;
    private readonly UdpTransfer _udpTransfer;
    private readonly Dictionary<byte, Action<byte[]>> _packetHandlers;

    public PacketProcessor(ConfirmationTracker confirmationTracker, UdpTransfer udpTransfer)
    {
        _confirmationTracker = confirmationTracker;
        _udpTransfer = udpTransfer;
        
        _packetHandlers = new Dictionary<byte, Action<byte[]>>
        {
            {(byte)PayloadType.CONFIRM, ProcessConfirmationPacket},
            {(byte)PayloadType.REPLY, ProcessReplyPacket},
            {(byte)PayloadType.AUTH, ProcessAuthPacket},
            {(byte)PayloadType.JOIN, ProcessJoinPacket},
            {(byte)PayloadType.MSG, ProcessMsgPacket},
            {(byte)PayloadType.PING, ProcessPingPacket},
            {(byte)PayloadType.ERR, ProcessErrPacket},
            {(byte)PayloadType.BYE, ProcessByePacket}
        };
    }
    
    public void ProcessIncomingPacket(byte[] data)
    {
        if (_packetHandlers.TryGetValue(data[0], out var handler))
        {
            handler(data);
        }
        else
        {
            Console.WriteLine($"Unknown packet type: {data[0]}");
        }
    }

    private void ProcessConfirmationPacket(byte[] data)
    {
        var refId = $"{data[1]}{data[2]}";
        _confirmationTracker.SetConfirmation(refId);
        Console.WriteLine($"Confirmation received for ID: {refId}");
    }

    private void ProcessReplyPacket(byte[] data)
    {
        // Process reply packet
        Console.WriteLine($"Processing reply packet: {BitConverter.ToString(data)}");
        _udpTransfer.SendConfirm(data[1], data[2]);
        var result = data[3];
        var contentOffset = 6;
        var content = ExtractContent(data, contentOffset);
        Console.WriteLine($"Action {(result != 0 ? "Success" : "Failure")}: {Encoding.ASCII.GetString(content.ToArray())}");
    }

    private void ProcessAuthPacket(byte[] data)
    {
        throw new NotSupportedException("Server should not send AUTH packets.");
    }

    private void ProcessJoinPacket(byte[] data)
    {
        throw new NotSupportedException("Server should not send JOIN packets.");
    }

    private void ProcessMsgPacket(byte[] data)
    {
        // Process message packet
        Console.WriteLine($"Processing message packet: {BitConverter.ToString(data)}");

        _udpTransfer.SendConfirm(data[1], data[2]);

        var displaNameOffset = 3;
        var displayName = ExtractContent(data, 3);
        
        var messageOffset = displaNameOffset + displayName.Count + 1;
        var message = ExtractContent(data, messageOffset);

        Console.WriteLine(
            $"{Encoding.ASCII.GetString(displayName.ToArray())}: {Encoding.ASCII.GetString(message.ToArray())}");
    }
    
    private void ProcessPingPacket(byte[] data)
    {
        _udpTransfer.SendConfirm(data[1], data[2]);
    }
    
    private void ProcessErrPacket(byte[] data)
    {
        // Process error packet
        Console.WriteLine($"Processing error packet: {BitConverter.ToString(data)}");
            
        //TODO: graceful termination
    }
    
    private void ProcessByePacket(byte[] data)
    {
        // Process bye packet
        Console.WriteLine($"Processing bye packet: {BitConverter.ToString(data)}");
        _udpTransfer.SendConfirm(data[1], data[2]);
        //TODO: graceful termination
        
    }

    private List<byte> ExtractContent(byte[] data, int offset)
    {
        var content = new List<byte>();
        for (int i = offset; data[i] != 0; i++)
        {
            content.Add(data[i]);
        }

        return content;
    }
}