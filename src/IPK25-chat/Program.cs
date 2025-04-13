
using System.Net;
using System.Net.Sockets;
using IPK25_chat.CLI;
using IPK25_chat.Client;
using IPK25_chat.Core;
using IPK25_chat.Logger;
using IPK25_chat.Models;
using IPK25_chat.PacketProcess;
using IPK25_chat.Parsers;
using IPK25_chat.PayloadBuilders;
using IPK25_chat.Validators;
using TcpListener = IPK25_chat.Client.TcpListener;

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

        IProtocolPayloadBuilder payloadBuilder;
        IClient client;
        IListener listener;
        IValidator validatorOfIncoming;
        IPacketProcessor packetProcessor;
        var serverEndPoint = CreateInitialEndpoint(parser.ParsedOptions.ServerAddress, parser.ParsedOptions.Port);
        if (parser.ParsedOptions.Protocol == "udp")
        {
            UdpClient udpClient = new UdpClient(0);
            UdpClientConfig udpConfig = new(retryCount, retryDelay, serverEndPoint, confirmationTracker, udpClient);
            
            payloadBuilder = new UdpProtocolPayloadBuilder(userModel);
            UdpTransfer udpTransfer = new UdpTransfer(udpConfig, payloadBuilder);
            client = udpTransfer;
            listener = new UdpListener(udpConfig.UdpClient, udpTransfer);
            validatorOfIncoming = new UdpValidator();
            packetProcessor = new UdpPacketProcessor(confirmationTracker, udpTransfer);
        }
        else if (parser.ParsedOptions.Protocol == "tcp")
        {
            payloadBuilder = new TcpProtocolPayloadBuilder(userModel);
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(serverEndPoint);
            client = new TcpTransfer(payloadBuilder, tcpClient);
            listener = new TcpListener(tcpClient);
            validatorOfIncoming = new TcpValidator();
            packetProcessor = new TcpPacketProcessor();
        }
        else
        {
            Console.WriteLine("Unknown protocol.");
            Environment.Exit(1);
            return;
        }

        MyFiniteStateMachine fsm = new();
        MessageHandler messageHandler = new(packetProcessor, fsm, listener, client, validatorOfIncoming);

        listener.OnMessageArrival += messageHandler.HandleMessage;
        listener.StartListening();
        
        bool isRunning = true;
        
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            var byeMessage = payloadBuilder.CreateByePacket();
            messageHandler.HandleMessage(false, byeMessage);
            listener.StopListening();
            client.Dispose();
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