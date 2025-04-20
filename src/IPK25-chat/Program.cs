
using IPK25_chat.CLI;
using IPK25_chat.Clients.Interfaces;
using IPK25_chat.Enums;
using IPK25_chat.InputParse;
using IPK25_chat.PayloadBuilders;

namespace IPK25_chat;

internal abstract class Program
{
    private static Queue<string> _userCommands = new(); 
    
    public static void Main(string[] args)
    {
        ArgumentParser parser = new();
        parser.Parse(args);
        
        var appFactory = new AppFactory(new ArgumentChecker());
        var (client, listener, msgHandler, payloadBuilder, msgParser) = appFactory.CreateApp(parser.ParsedOptions);

        listener.OnMessageArrival += msgHandler.HandleMessage;
        listener.StartListening();
        
        StartListeningForUserInput(payloadBuilder, msgHandler, msgParser, listener, client);
    }

    private static void HandleCancelKeyPress(IProtocolPayloadBuilder payloadBuilder, MessageHandler msgHandler,
        IListener listener, IClient client)
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            HandleGracefulTermination(listener, client, payloadBuilder, msgHandler);
        };
    }

    private static void StartListeningForUserInput(IProtocolPayloadBuilder payloadBuilder, MessageHandler msgHandler,
        MessageParser msgParser, IListener listener, IClient client)
    {
        // set up CTRL + C handler
        HandleCancelKeyPress(payloadBuilder, msgHandler, listener, client);

        // start a new thread for user command processing
        StartProcessingUserCommands(msgHandler, payloadBuilder, msgParser);
        
        while (Console.ReadLine() is { } inputLine)
        {
            // needs to be locked because of race condition
            lock (_userCommands)
            {
                _userCommands.Enqueue(inputLine);
            }
        }

        // CTRL + D - handle EOF
        HandleGracefulTermination(listener, client, payloadBuilder, msgHandler);
    }
    
    private static void HandleGracefulTermination(IListener listener, IClient client,
        IProtocolPayloadBuilder payloadBuilder, MessageHandler msgHandler)
    {
        var byeMessage = payloadBuilder.CreateByePacket();
        msgHandler.HandleMessage(false, byeMessage);
        listener.StopListening();
        client.Dispose();
        Environment.Exit(0);
    }

    private static void StartProcessingUserCommands(MessageHandler msgHandler, IProtocolPayloadBuilder payloadBuilder,
        MessageParser msgParser)
    {
        Task.Run(() =>
        {
            while (true)
            {
                if (!msgHandler.IsWaitingForReply() && !msgHandler.IsProcessingUserCommand())
                {
                    string inputLine;
                    // needs to be locked because of race condition
                    lock (_userCommands)
                    {
                        if (_userCommands.Count == 0)
                            continue;
                        
                        inputLine = _userCommands.Dequeue();
                    }
                    
                    msgHandler.ProcessUserCommand(inputLine, payloadBuilder, msgHandler, msgParser);
                }
            }
        }); 
    }
}