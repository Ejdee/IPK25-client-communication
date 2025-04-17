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
Before the message is sent, it needs to be converted into the **IPK25-CHAT** protocol format. This is done by the `PayloadBuilder` class. 

#### Listening
This is more complicated. As mentioned, the TCP protocol does not guarantee that the packets will be delivered in one piece. Fortunately, the protocol specifies that the packet message is terminated by the `'\r\n'` sequence.
Therefore, the incoming bytes are stored in the `List<byte>` and when the desired sequence is found, the whole message is processed.

Processing the message means:
- **validation** - the message format is validated using `regular expressions` that are specified based on the **ABNF** grammar.
- **suitability check** - the message is checked if it is available in the current state of the **FSM**. If not, the message is ignored.
- **parsing** - the message is then parsed into the specified format and printed out to the standard output.

















