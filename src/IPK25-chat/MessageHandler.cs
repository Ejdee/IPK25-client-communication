using IPK25_chat.Client;
using IPK25_chat.Core;
using IPK25_chat.Enums;
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
                _client.SendMessage(payload);
                
                if(type == MessageType.AUTH)
                    StartAuthTimeout();
            }

            switch (_fsm.GetActionAvailable())
            {
                case FsmAction.SendErrorMessage:
                    _client.SendErrorMessage();
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
                _client.SendErrorMessage();
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

    private void TerminateCommunication()
    {
        Console.WriteLine("Ending the session.");
        _listener.StopListening();
        _listener.Dispose();
        _client.Dispose();
        Environment.Exit(0);
    }

    private void StartAuthTimeout()
    {
        _authTimeout = new CancellationTokenSource();
        
        Task.Delay(AuthTimeoutDuration, _authTimeout.Token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
            {
                _client.SendErrorMessage();
                TerminateCommunication();
            }
        }, TaskScheduler.Default);
    }
}