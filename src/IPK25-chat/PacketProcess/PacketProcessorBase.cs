using IPK25_chat.Enums;

namespace IPK25_chat.PacketProcess;

public abstract class PacketProcessorBase : IPacketProcessor
{
    protected readonly Dictionary<MessageType, Action<byte[]>> _packetHandlers = new();
    
    protected PacketProcessorBase()
    {
        _packetHandlers[MessageType.CONFIRM] = ProcessConfirmationPacket;
        _packetHandlers[MessageType.ERR] = ProcessErrorPacket;
        _packetHandlers[MessageType.BYE] = ProcessByePacket;
        _packetHandlers[MessageType.PING] = ProcessPingPacket;
        _packetHandlers[MessageType.MSG] = ProcessMessagePacket;
        _packetHandlers[MessageType.REPLY] = ProcessReplyPacket;
        _packetHandlers[MessageType.NOTREPLY] = ProcessReplyPacket;
    }
    
    public void ProcessIncomingPacket(MessageType type, byte[] data)
    {
        if(_packetHandlers.TryGetValue(type, out var handler))
        {
            handler(data);
        }
        else
        {
            Console.WriteLine($"Unknown packet type: {data[0]}");
        }
    }
    
    protected abstract void ProcessConfirmationPacket(byte[] data);
    protected abstract void ProcessErrorPacket(byte[] data);
    protected abstract void ProcessByePacket(byte[] data);
    protected abstract void ProcessPingPacket(byte[] data);
    protected abstract void ProcessMessagePacket(byte[] data);
    protected abstract void ProcessReplyPacket(byte[] data);
}