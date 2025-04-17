# Project 2 - IPK25 Chat Client

---
- **Author:** Adam Běhoun
- **Login:** xbehoua00
---

This project is implemented in C# using .NET 9.0 in an object-oriented manner.
	
## Table of Contents

// TODO: table of contents

### Usage

To compile this project, use `make` command in the terminal. This will create a binary file named **ipk25chat-client**.

#### Command-line arguments
`./ipk25chat-client -t <udp/tcp> -s <ip/hostname> [-p <port>] [-d <timeout>] [-r <retries>] [-h]`
- where:
  - `-t` - transport protocol (udp or tcp)
  - `-s` - server address (IP address or hostname)
  - `-p` - port number (4567 is default)
  - `-d` - timeout (in ms) on waiting for a **confirmation** message (UDP only, default is 250ms)
  - `-r` - number of message retransmissions (UDP only, default is 3) 
  - `-h` - print help message and exit
  

For command-line arguments parsing, the `CommandLineParser` NuGet package is used. 

## Project overview
The project implements a client that communicates with a specified server using `IPK25-CHAT` protocol. The communication can be performed over two different transport protocols - **TCP** and **UDP**. The validation of the communication is controlled by the **FSM** - finite state machine. 

### Allowed user commands
- `/help` - prints out the list of available commands
- `/auth <username> <display name> <secret>` - sends authentication request to the server and waits for a **reply** message signaling the success or the failure.
- `/join <channelID>` - sends a request to join the specified channel. Again the **reply** message is expected with the result of the request.
- `<msg>` - sends a message to the server.
- `/bye` - triggered by the user with `Ctrl+C` or `Ctrl+D` signalizing the end of the communication and **graceful** termination.

Very simplified and abstract flow-chart diagram be shown as follows (doesn't show the communication with the server):

![Flow chart](images/IPK25-chat.drawio(1).png)

## Project structure
As mentioned, the project follows the object-oriented design.
- `CLI` - contains classes for command-line parsing and validation. Parsed arguments are further passed to the program.
- `Clients` - contains two interfaces for **listener** and **transfer** (sending the messages) and its implementations for each transport protocol. There is also `ConfirmationTracker.cs` class that is responsible for tracking **confirmation** messages from the server and informing the **transfer** (only used for UDP).
- `FSM` - contains the implementation of the **finite state machine**
- `InputParse` - classes responsible for validating the input messages (`InputValidator.cs`) from the user and transforming them into suitable format for further processing (`MessageParser.cs`).
- `Models` - contains abstract representations of core models used in the program. 
  - `MessageModel.cs` is the collection of data needed for the message <-> IPK25-CHAT protocol conversion. 
  - `TransitionModel.cs` - represents the transition in the **FSM**.
  - `UserModel.cs` - contains the user data - most importantly the **display name** that can be changed during the runtime.
  - `UdpClientConfig.cs` - just an abstraction for the collection of UDP protocol configuration data (eg. retry count, timeout,...).
- `PacketProcess` - responsible for processing the incoming messages. Consists of an interface that is implemented by the two transport protocols again.
- `PayloadBuilders` - transforms the `MessageModel` into the **IPK25-CHAT** protocol format based on the type of transport protocol.
- `Validators` - validates the incoming messages from the server. 

### Structure and usage between the components shown in the diagram
- blue lines represents the usage
- green dotted lines represents the inheritance

![Diagram](images/IPK25-chat-usage-diagram.png)

## Transportation protocols
They operate on the L4 of the OSI model. The end points of the communication are specified by the **IP** address and the **port** number. Even though transport protocols like **SCTP**, **QUIC** and others are still becoming more popular, the most widely used are **TCP** and **UDP**. [1]

## TCP protocol
TCP is a point-to-point protocol (1 sender, 1 receiver). It guarantees the delivery of the packets in the same order as they were sent. But this requires some preparation of the communication in some form of **handshake**. But the protocol does **not** guarantee that the packets will be delivered in one piece. This behaviour needs to be handled by the program. [1],[2]

### Implementation of TCP protocol
The core of the implementation is based on the built in `TcpClient` class. When the program is executed, the server end point is created and the connection is established. 
```csharp
tcpClient.Connect(serverEndPoint);
```
This client is used both in the **listener** and **transfer** classes. 

#### Sending
Before the message is sent, it needs to be converted into the **IPK25-CHAT** protocol format. This is done by the `PayloadBuilder` class. Then it is simply sent using these three lines:
```csharp
NetworkStream stream = _tcpClient.GetStream();
stream.Write(payload, 0, payload.Length);
stream.Flush();
```

#### Listening
This is more complicated. As mentioned, the TCP protocol does not guarantee that the packets will be delivered in one piece. Fortunately, the protocol specifies that the packet message is terminated by the `'\r\n'` sequence.
Therefore, the incoming bytes are stored in the `List<byte>` and when the desired sequence is found, the whole message is processed.

Processing the message means:
- **validation** - the message format is validated using `regular expressions` that are specified based on the **ABNF** grammar.
- **suitability check** - the message is checked if it is available in the current state of the **FSM**. If not, the message is ignored.
- **parsing** - the message is then parsed into the specified format and printed out to the standard output.

Here is the example of using regular expression for validation. The logic behind this is to store all the valid regexes into a list and on the server incoming message iterate through it to see if there is a check.
```csharp
const string fromRegex = @"[fF][rR][oO][mM]";
const string byeRegex = @"[bB][yY][eE]";
const string displayNameRegex = @"[\x21-\x7E]{1,20}";
...
new KeyValuePair<Regex, MessageType>(
    new Regex($@"{byeRegex}\s{fromRegex}\s{displayNameRegex}\r\n"), MessageType.BYE)
...
if (regex.Key.IsMatch(Encoding.ASCII.GetString(message)))
    return regex.Value; // return the corresponding message type (BYE in this case)
```

#### Message processing diagram

![Message processing](images/tcp-message-processing.drawio.png)

## UDP protocol
Simple transport protocol that does not guarantee the delivery of the packets nor the same order of the packets as they were sent.
There is no need for some form of **handshake** since the protocol is **connection-less**. It is assumed, that the **IP** protocol is used as underlying protocol. [1],[2],[3]

### Implementation of UDP protocol
Because of the unreliable delivery, the program needs some kind of assurance of the successful delivery of the packets. This is done by sending the **confirmation** message back to the sender. The confirmation packet consists of its type (`0x00`) and the **message ID** of the original message. 

#### Confirmation tracker
For this purpose, the `ConcurrentDictionary<string, ManualResetEventSlim>` is used. Where `string` is the **message ID** and `ManualResetEventSlim` behaves like a semaphore. When the message is sent, the program waits for the listener to trigger the semaphore (when the **confirmation** message comes). If the semaphore is not triggered within the specified timeout, the message is sent again. [4][5]

#### Sending
The sending works with the `UdpClient` class. Another obstacle in the UDP is that the first message is sent to `4567 or specified port` and the following communication is switched to dynamic port. This is resolved by the `UdpListener` that switches the server end-point when **REPLY** message is received.

#### Listening
Another obstacle may be **duplicate** message processing. Since it cannot be guaranteed that our confirmation message will be delivered, the server may send the same message again. This is resolved by keeping the processed message **IDs** in the `HashSet<string>`. So the message validation can be described with a diagram like this:

![udp message processing](images/UDP-message-processing.drawio.png)

## FSM (finite state machine)
As mentioned, the communication is controlled by the **FSM**. But in this project it doesn't mean that the main flow of the program is executed there. It means that the **FSM** oversees and guides the `MessageHandler.cs` class that is responsible for sending and receiving messages. 
### Implementation of FSM
The **FSM** is implemented in the `MyFiniteStateMachine.cs` class. It uses the `TransitionModel.cs` to build the available transitions. So on **initial** and every other state, the available transitions for this state are stored in the `List<TransitionModel> _currentAvailableTransitions` and as the program flows, they invalid states are removed. This is an example of the specified transition:
```csharp
new(States.START, States.AUTH, MessageType.BLANK, MessageType.AUTH, true, false),
```
where:
- `States.START` - initial state
- `States.AUTH` - the next state
- `MessageType.BLANK` - the message that is expected from the server
- `MessageType.AUTH` - the message that is expected from the user
- `true` - the message is received from the server (since it is blank (no message needed) here, it is received on initialization)
- `false` - the message from the user is not yet received

So when the user sends the **AUTH** message, the transition has the two last values set to `true`, meaning that transition needs to be performed. When the transition is performed, the state variable is changes and the available transitions are updated.


## Testing






