using IPK25_chat.Client;
using IPK25_chat.Enums;
using IPK25_chat.Parsers;

namespace IPK25_chat.Core;

public class MessageHandler
{
    private readonly MyFiniteStateMachine _fsm;
    private readonly UdpTransfer _udpTransfer;
    private readonly UdpListener _udpListener;
    private readonly PacketProcessor _packetProcessor;

    public MessageHandler(PacketProcessor packetProcessor, UdpTransfer udpTransfer, MyFiniteStateMachine fsm, UdpListener udpListener)
    {
        _packetProcessor = packetProcessor;
        _udpTransfer = udpTransfer;
        _fsm = fsm;
        _udpListener = udpListener;
    }

    public void HandleMessage(bool isIncoming, byte[] payload)
    {
        var type = GetMessageType(payload);
        if (_fsm.MessageValidInTheCurrentState(type, isIncoming))
        {
            if (isIncoming)
            {
                _packetProcessor.ProcessIncomingPacket(payload);
            }
            else
            {
                _udpTransfer.SendMessage(payload);
            }

            if (_fsm.TransitionAvailable())
            {
                Console.WriteLine($"Transitioning from state {_fsm.CurrentState}.");
                _fsm.PerformTransition();
                if (_fsm.CurrentState == States.END)
                {
                    Console.WriteLine("Ending the session.");
                    _udpListener.StopListening();
                    _udpTransfer.Dispose();
                    Environment.Exit(0);
                }
            }
        }
        else
        {
            Console.WriteLine($"Message not valid in the current state {_fsm.CurrentState}: {BitConverter.ToString(payload)}");
        }
    }

    private MessageType GetMessageType(byte[] data)
    {
        return data[0] switch
        {
            (byte)PayloadType.AUTH => MessageType.AUTH,
            (byte)PayloadType.JOIN => MessageType.JOIN,
            (byte)PayloadType.MSG => MessageType.MSG,
            (byte)PayloadType.REPLY => data[3] != 0 ? MessageType.REPLY : MessageType.NOTREPLY,
            (byte)PayloadType.PING => MessageType.PING,
            (byte)PayloadType.ERR => MessageType.ERR,
            (byte)PayloadType.BYE => MessageType.BYE,
            (byte)PayloadType.CONFIRM => MessageType.CONFIRM,
            _ => throw new NotSupportedException("Unsupported message type")
        };
    }
}