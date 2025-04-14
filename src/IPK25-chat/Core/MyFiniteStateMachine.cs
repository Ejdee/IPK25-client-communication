using IPK25_chat.Enums;
using IPK25_chat.Models;

namespace IPK25_chat.Core;

public class MyFiniteStateMachine
{
    private States _currentState;
    private List<TransitionModel> _allTransitions;
    private List<TransitionModel> _currentAvailableTransitions;
    
    public MyFiniteStateMachine()
    {
        _currentState = States.START;
        _allTransitions = new List<TransitionModel>
        {
            new(States.START, States.AUTH, MessageType.BLANK, MessageType.AUTH, true, false),
            new(States.START, States.END, MessageType.BYE, MessageType.BLANK, false, true),
            new(States.START, States.END, MessageType.ERR, MessageType.BLANK, false, true),
            new(States.START, States.END, MessageType.BLANK, MessageType.BYE, true, false),
            
            //new(States.AUTH, States.AUTH, MessageType.NOTREPLY, MessageType.AUTH, false, false),
            new(States.AUTH, States.AUTH, MessageType.NOTREPLY, MessageType.BLANK, false, true),
            new(States.AUTH, States.AUTH, MessageType.BLANK, MessageType.AUTH, true, false),
            new(States.AUTH, States.OPEN, MessageType.REPLY, MessageType.BLANK, false, true),
            new(States.AUTH, States.END, MessageType.ERR, MessageType.BLANK, false, true),
            new(States.AUTH, States.END, MessageType.BYE, MessageType.BLANK, false, true),
            new(States.AUTH, States.END, MessageType.MSG, MessageType.ERR, false, false),
            new(States.AUTH, States.END, MessageType.BLANK, MessageType.BYE, true, false),
            
            new(States.OPEN, States.OPEN, MessageType.MSG, MessageType.BLANK, false, true),
            new(States.OPEN, States.OPEN, MessageType.BLANK, MessageType.MSG, true, false),
            new(States.OPEN, States.JOIN, MessageType.BLANK, MessageType.JOIN, true, false),
            new(States.OPEN, States.END, MessageType.ERR, MessageType.BLANK, false, true),
            new(States.OPEN, States.END, MessageType.BYE, MessageType.BLANK, false, true),
            new(States.OPEN, States.END, MessageType.REPLY, MessageType.ERR, false, false),
            new(States.OPEN, States.END, MessageType.NOTREPLY, MessageType.ERR, false, false),
            new(States.OPEN, States.END, MessageType.BLANK, MessageType.BYE, true, false),
            
            new(States.JOIN, States.JOIN, MessageType.MSG, MessageType.BLANK, false, true),
            new(States.JOIN, States.END, MessageType.ERR, MessageType.BLANK, false, true),
            new(States.JOIN, States.END, MessageType.BYE, MessageType.BLANK, false, true),
            new(States.JOIN, States.OPEN, MessageType.REPLY, MessageType.BLANK, false, true),
            new(States.JOIN, States.OPEN, MessageType.NOTREPLY, MessageType.BLANK, false, true),
        };

        _currentAvailableTransitions = GetAvailableTransitions(_currentState);
    }

    public States CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            // load the new available transitions
            _currentAvailableTransitions = _allTransitions
                .Where(t => t.CurrentState == _currentState).ToList();
        } 
    }

    public FsmAction GetActionAvailable()
    {
        if (TransitionAvailable()) 
            return FsmAction.PerformTransition;

        var errAction = _currentAvailableTransitions
            .Any(s => s.ServerObtained && s.ClientMessage == MessageType.ERR);

        if (errAction)
        {
            _currentState = States.END;
            return FsmAction.SendErrorMessage;
        }

        return FsmAction.NoAction;
    }
    
    public bool TransitionAvailable()
        => _currentAvailableTransitions.Any(t => t is { ServerObtained: true, ClientObtained: true });

    public bool MessageValidInTheCurrentState(MessageType message, bool isServerMessage)
    {
        if (message == MessageType.PING || message == MessageType.CONFIRM)
        {
            return true;
        } 
        
        var anyTransition = isServerMessage
            ? _currentAvailableTransitions.Any(t => !t.ServerObtained && t.ServerMessage == message)
            : _currentAvailableTransitions.Any(t => t is { ServerObtained: true, ClientObtained: false } && t.ClientMessage == message);
        
        if (!anyTransition)
            return false;
        
        if (isServerMessage)
        {
            _currentAvailableTransitions.RemoveAll(t =>
            {
                if (!t.ServerObtained && t.ServerMessage == message)
                {
                    t.ServerObtained = true;
                    return false;
                }

                return true;
            });
        }
        else
        {
            _currentAvailableTransitions.RemoveAll(t =>
            {
                if (t is { ServerObtained: true, ClientObtained: false } && t.ClientMessage == message)
                {
                    t.ClientObtained = true;
                    return false;
                }

                return true;
            });
        }

        // if we have a state where server sent a message and client should send an error,
        // flag the validation as false 
        if (_currentAvailableTransitions.Any(t => t is { ServerObtained: true, ClientMessage: MessageType.ERR }))
            return false;
         
        
        return true;
    }

    public void PerformTransition()
    {
        var transition = _currentAvailableTransitions
            .Where(t => t.ServerObtained && t.ClientObtained)
            .ToList();

        if (transition.Count != 1)
            return;

        _currentState = transition[0].NextState;
        _currentAvailableTransitions = GetAvailableTransitions(_currentState); 
        
        //Console.WriteLine($"Performing transition to state: {_currentState}");
    }

    private List<TransitionModel> GetAvailableTransitions(States state)
    {
        return _allTransitions
            .Where(t => t.CurrentState == state)
            .Select(t => new TransitionModel(
                t.CurrentState,
                t.NextState,
                t.ServerMessage,
                t.ClientMessage,
                t.ServerObtained,
                t.ClientObtained))
            .ToList();
    }
}