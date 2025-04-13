
using System.Net;
using System.Net.Sockets;
using IPK25_chat.CLI;
using IPK25_chat.Client;
using IPK25_chat.Core;
using IPK25_chat.Logger;
using IPK25_chat.Models;
using IPK25_chat.Parsers;
using IPK25_chat.Protocol;

namespace IPK25_chat;

internal abstract class Program
{
    public static void Main(string[] args)
    {
        ArgumentParser parser = new();
        parser.Parse(args);

        UserModel userModel = new UserModel("DefaultUsername", "defaultDisplayName");
        ConfirmationTracker confirmationTracker = new();
        ResultLogger resultLogger = new();
        InputValidator validator = new();
        MessageParser msgParser = new(validator, resultLogger, userModel);
        UdpProtocolPayloadBuilder payloadBuilder = new UdpProtocolPayloadBuilder(userModel);

        int retryCount = 0;
        int retryDelay = 0;
        
        if (parser.ParsedOptions == null)
        {
            Console.WriteLine("Parsed options are null.");
            return;
        }
        else
        {
            Console.WriteLine($"Transport: {parser.ParsedOptions.Protocol}");
            Console.WriteLine($"Server: {parser.ParsedOptions.ServerAddress}");
            Console.WriteLine($"Port: {parser.ParsedOptions.Port}");
            Console.WriteLine($"Confirmation timeout: {parser.ParsedOptions.UdpTimeout}");
            Console.WriteLine($"Max Retries: {parser.ParsedOptions.UdpRetries}");
            retryCount = parser.ParsedOptions.UdpRetries;
            retryDelay = parser.ParsedOptions.UdpTimeout;
        }

        UdpClient udpClient = new UdpClient(0);
        var serverEndPoint = CreateInitialEndpoint(parser.ParsedOptions.ServerAddress, parser.ParsedOptions.Port);
        UdpClientConfig udpConfig = new(retryCount, retryDelay, serverEndPoint, confirmationTracker, udpClient);
        using UdpTransfer udpTransfer = new UdpTransfer(udpConfig, payloadBuilder);
        PacketProcessor packetProcessor = new(confirmationTracker, udpTransfer);
        UdpListener udpListener = new(udpClient, udpTransfer);
        MyFiniteStateMachine fsm = new();
        MessageHandler messageHandler = new(packetProcessor, udpTransfer, fsm, udpListener);

        udpListener.OnMessageArrival += messageHandler.HandleMessage;
        udpListener.StartListening();
        
        
        bool isRunning = true;
        
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            var byeMessage = payloadBuilder.CreateByePacket();
            messageHandler.HandleMessage(false, byeMessage);
            udpListener.StopListening();
            isRunning = false;
            Environment.Exit(0);
        };

        while (isRunning && Console.ReadLine() is { } inputLine)
        {
            if (!msgParser.ParseMessage(inputLine, out var model))
            {
                Console.WriteLine("Invalid message format.");
                continue;
            }
            
            try
            {
                var result = payloadBuilder.GetPayloadFromMessage(model);
                messageHandler.HandleMessage(false, result);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending message: {e.Message}");
            }
        }
    }

    public static IPEndPoint CreateInitialEndpoint(string serverAddress, int port)
    {
        if (!IPAddress.TryParse(serverAddress, out var ipAddress))
        {
            var addresses = Dns.GetHostAddresses(serverAddress);
            if (addresses.Length == 0)
            {
                throw new ArgumentException("Invalid server address");
            } 
            ipAddress = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? addresses.First();
        }
        return new IPEndPoint(ipAddress, port);
    }
}