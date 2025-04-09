using System.Text;
using IPK25_chat.Protocol;

namespace IPK25_chat.Client;

public class PacketProcessor
{
    private readonly ConfirmationTracker _confirmationTracker;
    private readonly UdpTransfer _udpTransfer;

    public PacketProcessor(ConfirmationTracker confirmationTracker, UdpTransfer udpTransfer)
    {
        _confirmationTracker = confirmationTracker;
        _udpTransfer = udpTransfer;
    }

    public void ProcessIncomingPacket(byte[] data)
    {
        if (data[0] == (byte)PayloadTypeEnum.CONFIRM)
        {
            Console.WriteLine($"Received confirmation packet: {BitConverter.ToString(data)}");
            ProcessConfirmationPacket(data);
        }
        else if (data[0] == (byte)PayloadTypeEnum.REPLY)
        {
            ProcessReplyPacket(data);
        }
        else if (data[0] == (byte)PayloadTypeEnum.AUTH)
        {
            ProcessAuthPacket(data);
        }
        else if (data[0] == (byte)PayloadTypeEnum.JOIN)
        {
            ProcessJoinPacket(data);
        }
        else if (data[0] == (byte)PayloadTypeEnum.MSG)
        {
            ProcessMsgPacket(data);
        }
        else if (data[0] == (byte)PayloadTypeEnum.PING)
        {
            ProcessPingPacket(data);
            Console.WriteLine($"Received PING packet: {BitConverter.ToString(data)}");
        }
        else if (data[0] == (byte)PayloadTypeEnum.ERR)
        {
            ProcessErrPacket(data);
            Console.WriteLine($"Received ERR packet: {BitConverter.ToString(data)}");
        }
        else if (data[0] == (byte)PayloadTypeEnum.BYE)
        {
            ProcessByePacket(data);
            Console.WriteLine($"Received BYE packet: {BitConverter.ToString(data)}");
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
        List<byte> content = new List<byte>();
        for (int i = contentOffset; data[i] != 0; i++)
        {
            content.Add(data[i]);
        }
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
        List<byte> displayName = new List<byte>();
        for (int i = displaNameOffset; data[i] != 0; i++)
        {
            displayName.Add(data[i]);
        }

        var messageOffset = displaNameOffset + displayName.Count + 1;

        List<byte> message = new List<byte>();
        for (int i = messageOffset; data[i] != 0; i++)
        {
            message.Add(data[i]);
        }

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
}