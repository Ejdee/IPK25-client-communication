using System.Net;
using System.Net.Sockets;
using IPK25_chat.CLI;
using IPK25_chat.Client;
using IPK25_chat.Core;
using IPK25_chat.Models;
using IPK25_chat.PacketProcess;
using IPK25_chat.Parsers;
using IPK25_chat.PayloadBuilders;
using IPK25_chat.Validators;
using TcpListener = IPK25_chat.Client.TcpListener;

namespace IPK25_chat;

public class AppFactory
{
    private readonly ArgumentChecker _argumentChecker;

    public AppFactory(ArgumentChecker argumentChecker)
    {
        _argumentChecker = argumentChecker;
    }

    public (IClient client, IListener listener, MessageHandler msgHandler, IProtocolPayloadBuilder payloadBuilder,
        MessageParser msgParser) CreateApp(ArgumentOptions? args)
    {
        args = _argumentChecker.CheckArguments(args);

        var serverEndPoint = CreateInitialEndpoint(args.ServerAddress, args.Port);
        var userModel = new UserModel("Default", "default");

        return args.Protocol switch
        {
            "udp" => CreateUdpComponents(args, serverEndPoint, userModel),
            "tcp" => CreateTcpComponents(serverEndPoint, userModel),
            _ => throw new ArgumentException("Unknown protocol")
        };
    }

    private static (IClient client, IListener listener, MessageHandler msgHandler, IProtocolPayloadBuilder
        payloadBuilder, MessageParser msgParser) CreateUdpComponents(
            ArgumentOptions args, IPEndPoint serverEndPoint, UserModel userModel)
    {
        var confirmationTracker = new ConfirmationTracker();
        var udpClient = new UdpClient(0);
        var udpConfig = new UdpClientConfig(args.UdpRetries, args.UdpTimeout, serverEndPoint, confirmationTracker,
            udpClient);
        var payloadBuilder = new UdpProtocolPayloadBuilder(userModel);
        var udpTransfer = new UdpTransfer(udpConfig, payloadBuilder);
        var listener = new UdpListener(udpClient, udpTransfer);
        var packetProcessor = new UdpPacketProcessor(confirmationTracker, udpTransfer);
        var validator = new UdpValidator();
        var fsm = new MyFiniteStateMachine();
        var msgHandler = new MessageHandler(packetProcessor, fsm, listener, udpTransfer, validator);
        var msgParser = new MessageParser(new InputValidator(), userModel);

        return (udpTransfer, listener, msgHandler, payloadBuilder, msgParser);
    }

    private static (IClient client, IListener listener, MessageHandler msgHandler, IProtocolPayloadBuilder
        payloadBuilder, MessageParser msgParser) CreateTcpComponents(
            IPEndPoint serverEndPoint, UserModel userModel)
    {
        var payloadBuilder = new TcpProtocolPayloadBuilder(userModel);
        var tcpClient = new TcpClient();
        tcpClient.Connect(serverEndPoint);
        var tcpTransfer = new TcpTransfer(payloadBuilder, tcpClient);
        var listener = new TcpListener(tcpClient);
        var validator = new TcpValidator();
        var packetProcessor = new TcpPacketProcessor();
        var fsm = new MyFiniteStateMachine();
        var msgHandler = new MessageHandler(packetProcessor, fsm, listener, tcpTransfer, validator);
        var msgParser = new MessageParser(new InputValidator(), userModel);

        return (tcpTransfer, listener, msgHandler, payloadBuilder, msgParser);
    }

    private static IPEndPoint CreateInitialEndpoint(string serverAddress, int port)
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