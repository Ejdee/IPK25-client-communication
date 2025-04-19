
using IPK25_chat.CLI;
using IPK25_chat.Clients.Interfaces;
using IPK25_chat.Enums;
using IPK25_chat.InputParse;
using IPK25_chat.PayloadBuilders;

namespace IPK25_chat;

internal abstract class Program
{
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
        
        while (Console.ReadLine() is { } inputLine)
        {
            if (!msgParser.ParseMessage(inputLine, out var model))
            {
                Console.WriteLine("ERROR: Invalid message format.");
                continue;
            }

            if (model.MessageType is MessageType.RENAME or MessageType.HELP)
                continue;

            try
            {
                var result = payloadBuilder.GetPayloadFromMessage(model);
                msgHandler.HandleMessage(false, result);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: sending message: {e.Message}");
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
}