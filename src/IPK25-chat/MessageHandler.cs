using System.Text;
using IPK25_chat.Clients.Interfaces;
using IPK25_chat.Enums;
using IPK25_chat.FSM;
using IPK25_chat.PacketProcess;
using IPK25_chat.Validators;

namespace IPK25_chat;

public class MessageHandler
{
    private readonly MyFiniteStateMachine _fsm;
    private readonly IListener _listener;
    private readonly IClient _client;
    private readonly IValidator _validator;
    private readonly IPacketProcessor _packetProcessor;

    private CancellationTokenSource? _authTimeout;
    private const int AuthTimeoutDuration = 5000;
    
    private int _terminationCode = 0;

    public MessageHandler(IPacketProcessor packetProcessor, MyFiniteStateMachine fsm, IListener listener, IClient client, IValidator validator)
    {
        _packetProcessor = packetProcessor;
        _fsm = fsm;
        _listener = listener;
        _client = client;
        _validator = validator;
    }

    public void HandleMessage(bool isIncoming, byte[] payload)
    {
        var type = _validator.ValidateAndGetMsgType(payload);
        if (type == MessageType.INVALID)
        {
            _terminationCode = 1;
            Console.WriteLine($"ERROR: Invalid message: {BitConverter.ToString(payload)}");
            _client.SendErrorMessage(Encoding.ASCII.GetBytes("Invalid message format"));
            TerminateCommunication(_terminationCode);
        } 
        
        if (_fsm.MessageValidInTheCurrentState(type, isIncoming))
        {
            if (isIncoming)
            {
                if (type is MessageType.REPLY or MessageType.NOTREPLY)
                {
                    _authTimeout?.Cancel();
                    _authTimeout = null;
                }
                
                _packetProcessor.ProcessIncomingPacket(type, payload);
            }
            else
            {
                try
                {
                    _client.SendMessage(payload);
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("ERROR: Timeout while waiting for a confirmation.");
                    TerminateCommunication(1);
                }

                if(type is MessageType.AUTH or MessageType.JOIN)
                    StartAuthTimeout();
            }

            switch (_fsm.GetActionAvailable())
            {
                case FsmAction.SendErrorMessage:
                    _client.SendErrorMessage(Encoding.ASCII.GetBytes("Unwanted action"));
                    _terminationCode = 1;
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
                _client.SendErrorMessage(Encoding.ASCII.GetBytes("Unwanted action"));
                _terminationCode = 1;
            }
            else
            {
                Console.WriteLine(
                    $"ERROR: Message not valid in the current state {_fsm.CurrentState}: {BitConverter.ToString(payload)}");
            }
        }
        
        if (_fsm.CurrentState == States.END)
        {
            _authTimeout?.Cancel();
            TerminateCommunication(_terminationCode);
        }
    }

    private void TerminateCommunication(int exitCode)
    {
        _listener.StopListening();
        _listener.Dispose();
        _client.Dispose();
        Environment.Exit(exitCode);
    }

    private void StartAuthTimeout()
    {
        _authTimeout = new CancellationTokenSource();
        
        Task.Delay(AuthTimeoutDuration, _authTimeout.Token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
            {
                _client.SendErrorMessage(Encoding.ASCII.GetBytes("Authentication timeout"));
                Console.WriteLine("ERROR: timeout for REPLY message");
                TerminateCommunication(1);
            }
        }, TaskScheduler.Default);
    }
}