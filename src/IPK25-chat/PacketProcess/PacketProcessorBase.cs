using IPK25_chat.Enums;

namespace IPK25_chat.PacketProcess;

public abstract class PacketProcessorBase : IPacketProcessor
{
    protected readonly Dictionary<MessageType, Action<byte[]>> PacketHandlers = new();
    
    protected PacketProcessorBase()
    {
        PacketHandlers[MessageType.CONFIRM] = ProcessConfirmationPacket;
        PacketHandlers[MessageType.ERR] = ProcessErrorPacket;
        PacketHandlers[MessageType.BYE] = ProcessByePacket;
        PacketHandlers[MessageType.PING] = ProcessPingPacket;
        PacketHandlers[MessageType.MSG] = ProcessMessagePacket;
        PacketHandlers[MessageType.REPLY] = ProcessReplyPacket;
        PacketHandlers[MessageType.NOTREPLY] = ProcessReplyPacket;
    }
    
    public void ProcessIncomingPacket(MessageType type, byte[] data)
    {
        if(PacketHandlers.TryGetValue(type, out var handler))
        {
            handler(data);
        }
        else
        {
            Console.WriteLine($"ERROR: Unknown packet type: {data[0]}");
        }
    }
    
    protected abstract void ProcessConfirmationPacket(byte[] data);
    protected abstract void ProcessErrorPacket(byte[] data);
    protected abstract void ProcessByePacket(byte[] data);
    protected abstract void ProcessPingPacket(byte[] data);
    protected abstract void ProcessMessagePacket(byte[] data);
    protected abstract void ProcessReplyPacket(byte[] data);
}