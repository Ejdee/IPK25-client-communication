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

    private CancellationTokenSource? _authTimeout;
    private readonly int _authTimeoutDuration = 5000;

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
                if (type is MessageType.REPLY or MessageType.NOTREPLY)
                {
                    _authTimeout?.Cancel();
                    _authTimeout = null;
                }
                
                _packetProcessor.ProcessIncomingPacket(payload);
            }
            else
            {
                _udpTransfer.SendMessage(payload);
                
                if(type == MessageType.AUTH)
                    StartAuthTimeout();
            }

            switch (_fsm.GetActionAvailable())
            {
                case FsmAction.SendErrorMessage:
                    _udpTransfer.SendErrorMessage();
                    break;
                case FsmAction.PerformTransition:
                    _fsm.PerformTransition();
                    break;
                case FsmAction.NoAction:
                    break;
            }
        }
        else
        {
            if (_fsm.GetActionAvailable() == FsmAction.SendErrorMessage)
            {
                _udpTransfer.SendErrorMessage();
            }
            else
            {
                Console.WriteLine(
                    $"Message not valid in the current state {_fsm.CurrentState}: {BitConverter.ToString(payload)}");
            }
        }
        
        if (_fsm.CurrentState == States.END)
        {
            _authTimeout?.Cancel();
            TerminateCommunication();
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

    private void TerminateCommunication()
    {
        Console.WriteLine("Ending the session.");
        _udpListener.StopListening();
        _udpTransfer.Dispose();
        Environment.Exit(0);
    }

    private void StartAuthTimeout()
    {
        _authTimeout = new CancellationTokenSource();
        
        Task.Delay(_authTimeoutDuration, _authTimeout.Token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
            {
                _udpTransfer.SendErrorMessage();
                TerminateCommunication();
            }
        }, TaskScheduler.Default);
    }
}