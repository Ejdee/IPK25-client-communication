using IPK25_chat.Client;
using IPK25_chat.Core;
using IPK25_chat.Enums;
using IPK25_chat.Models;

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
        
        // Assert
        Assert.True(isValid);
    }
    
    [Fact]
    public void MsgValid_AuthStateInvalidMessageClient_AUTH()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var message = MessageType.AUTH;
        var isServerMessage = false;
        
        // Act
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void MsgValid_AuthStateValidMessageClient_AferNotReply_AUTH()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.AUTH;
        var messageServer = MessageType.NOTREPLY;
        var message = MessageType.AUTH;
        var isServerMessage = false;
        
        // Act
        
        var preSend = fsm.MessageValidInTheCurrentState(messageServer, true);
        var isValid = fsm.MessageValidInTheCurrentState(message, isServerMessage);
        
        // Assert
        Assert.True(preSend);
        Assert.True(isValid);
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
    public void MsgValid_OpenStateValidMessageClient_ERRAfterServer_ANYREPLY()
    {
        // Arrange
        var fsm = new MyFiniteStateMachine();
        fsm.CurrentState = States.OPEN;
        var serverMessage = MessageType.REPLY;
        var clientMessage = MessageType.ERR;

        // Act
        var preSend = fsm.MessageValidInTheCurrentState(serverMessage, true);
        var isValid = fsm.MessageValidInTheCurrentState(clientMessage, false);

        // Assert
        Assert.True(preSend);
        Assert.True(isValid);
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
        Assert.False(result);
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
}