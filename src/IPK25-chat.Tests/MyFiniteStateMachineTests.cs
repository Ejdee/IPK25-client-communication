using IPK25_chat.Enums;
using IPK25_chat.FSM;

namespace IPK25_chat.Tests;

public class MyFiniteStateMachineTests
{
    [Fact]
    public void MsgValid_StartStateValidMessageClient_AUTH()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        var message = MessageType.AUTH;
        var isServerMessage = false;
        
        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);
        
        // Assert
        Assert.True(isValid);
    }
    
    [Fact]
    public void MsgValid_StartStateInvalidMessageClient_JOIN()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        var message = MessageType.JOIN;
        var isServerMessage = false;
        
        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void MsgValid_StartStateInvalidMessageServer_MSG()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        var message = MessageType.MSG;
        var isServerMessage = true;
        
        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void MsgValid_StartStateValidMessageClient_Bye()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        var message = MessageType.BYE;
        var isServerMessage = false;
        
        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);
        
        // Assert
        Assert.True(isValid);
    }
    
    [Fact]
    public void MsgValid_AuthStateValidMessageServer_NOTREPLY()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var message = MessageType.NOTREPLY;
        var isServerMessage = true;
        
        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);
        
        // Assert
        Assert.True(isValid);
    }
    
    [Fact]
    public void MsgValid_AuthStateValidMessageServer_MSG()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var message = MessageType.MSG;
        var isServerMessage = true;
        
        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);
        var shouldSendError = fsm.GetActionAvailable();
        
        // Assert
        Assert.False(isValid);
        Assert.Equal(FsmAction.SendErrorMessage, shouldSendError);
    }
    
    [Fact]
    public void MsgValid_AuthStateInvalidMessageClient_AUTH()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var message = MessageType.AUTH;
        
        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, false);
        
        // Assert
        Assert.True(isValid);
    }
    
    [Fact]
    public void MsgValid_AuthStateValidMessageClient_AferNotReply_AUTH()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var messageServer = MessageType.NOTREPLY;
        
        // Act
        
        var result = fsm.MessageValidInTheCurrentState(messageServer, true);
        if(fsm.TransitionAvailable())
        {
            fsm.PerformTransition();
        }
        
        // Assert
        Assert.True(result);
        Assert.Equal(States.AUTH, fsm.CurrentState);
    }
    
    [Fact]
    public void MsgValid_OpenStateValidMessageServer_MSG()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.OPEN;
        var message = MessageType.MSG;
        var isServerMessage = true;

        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void MsgValid_OpenStateValidMessageClient_MSG()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.OPEN;
        var message = MessageType.MSG;
        var isServerMessage = false;

        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void MsgValid_OpenStateValidMessageClient_JOIN()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.OPEN;
        var message = MessageType.JOIN;
        var isServerMessage = false;

        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void MsgValid_OpenStateInvalidMessageClient_ERR()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.OPEN;
        var message = MessageType.ERR;
        var isServerMessage = false;

        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void TransitionAvailable_ValidTransition()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        var clientMessage = MessageType.AUTH;
        
        // Act
        fsm.MessageValidInTheCurrentState(clientMessage, false);
        var result = fsm.TransitionAvailable();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void TransitionAvailable_InvalidTransition()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var serverMessage = MessageType.NOTREPLY;
        
        // Act
        fsm.MessageValidInTheCurrentState(serverMessage, true);
        var result = fsm.TransitionAvailable();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void TransitionAvailable_ValidTransition2()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var serverMessage = MessageType.REPLY;
        
        // Act
        fsm.MessageValidInTheCurrentState(serverMessage, true);
        var result = fsm.TransitionAvailable();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void PerformTransition_ValidTransition()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var serverMessage = MessageType.REPLY;
        
        // Act
        fsm.MessageValidInTheCurrentState(serverMessage, true);
        fsm.PerformTransition();
        
        // Assert
        Assert.Equal(States.OPEN, fsm.CurrentState);
    }
    
    [Fact]
    public void PerformTransition_InvalidTransition()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var serverMessage = MessageType.NOTREPLY;
        
        // Act
        fsm.MessageValidInTheCurrentState(serverMessage, true);
        fsm.PerformTransition();
        
        // Assert
        Assert.Equal(States.AUTH, fsm.CurrentState);
    }
    
    [Fact]
    public void PerformTransition_ValidTransition2()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.JOIN;
        var serverMessage = MessageType.MSG;
        
        // Act
        fsm.MessageValidInTheCurrentState(serverMessage, true);
        fsm.PerformTransition();
        
        // Assert
        Assert.Equal(States.JOIN, fsm.CurrentState);
    }

    [Fact]
    public void SendError_OpenToEnd()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.OPEN;
        var serverMessage = MessageType.REPLY;
        
        // Act
        fsm.MessageValidInTheCurrentState(serverMessage, true);
        var result = fsm.GetActionAvailable();
        
        // Assert
        Assert.Equal(FsmAction.SendErrorMessage, result);
    }

    [Fact]
    public void SendBye_FromOpenToEnd()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.OPEN;
        var clientMessage = MessageType.BYE;
        
        // Act
        fsm.MessageValidInTheCurrentState(clientMessage, false);
        var result = fsm.GetActionAvailable();
        fsm.PerformTransition();
        
        // Assert
        Assert.Equal(FsmAction.PerformTransition, result);
        Assert.Equal(States.END, fsm.CurrentState);
    }

    [Fact]
    public void SendErr_FromJoinServer()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.JOIN;
        var serverMessage = MessageType.ERR;
        
        // Act
        fsm.MessageValidInTheCurrentState(serverMessage, true);
        var result = fsm.GetActionAvailable();
        fsm.PerformTransition();
        
        // Assert
        Assert.Equal(FsmAction.PerformTransition, result);
        Assert.Equal(States.END, fsm.CurrentState);
    }
}