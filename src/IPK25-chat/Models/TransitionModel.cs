using IPK25_chat.Enums;

namespace IPK25_chat.Models;

public class TransitionModel
{
    public TransitionModel(States currentState, States nextState, MessageType serverMessage, MessageType clientMessage, bool serverObtained, bool clientObtained)
    {
        CurrentState = currentState;
        ServerMessage = serverMessage;
        ClientMessage = clientMessage;
        ServerObtained = serverObtained;
        ClientObtained = clientObtained;
        NextState = nextState;
    }

    public States CurrentState { get; }
    public States NextState { get; }
    public MessageType ServerMessage { get; }
    public MessageType ClientMessage { get; }
    public bool ServerObtained { get; set;  }
    public bool ClientObtained { get; set; }
}