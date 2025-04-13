using IPK25_chat.Enums;

namespace IPK25_chat.PacketProcess;

public interface IPacketProcessor
{
    public void ProcessIncomingPacket(MessageType type, byte[] data);
}