
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using IPK25_chat.CLI;
using IPK25_chat.Client;
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

        ConfirmationTracker confirmationTracker = new();
        InputValidator validator = new();
        MessageParser msgParser = new(validator);
        ProtocolPayloadBuilder payloadBuilder = new ProtocolPayloadBuilder("Frodo");
        ResultLogger resultLogger = new();
        User user = new User("DefaultUsername", "defaultDisplayName");
        
        
        if (parser.ParsedOptions == null)
        {
            Console.WriteLine("Parsed options are null.");     
        }
        else
        {
            Console.WriteLine($"Transport: {parser.ParsedOptions.Protocol}");
            Console.WriteLine($"Server: {parser.ParsedOptions.ServerAddress}");
            Console.WriteLine($"Port: {parser.ParsedOptions.Port}");
            Console.WriteLine($"Confirmation timeout: {parser.ParsedOptions.UdpTimeout}");
            Console.WriteLine($"Max Retries: {parser.ParsedOptions.UdpRetries}");
        }

        var serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4567);
        using UdpTransfer udpTransfer = new UdpTransfer(3, 250, serverEndPoint, confirmationTracker);
        PacketProcessor packetProcessor = new(confirmationTracker, udpTransfer);

        UdpListener udpListener = new(udpTransfer.UdpClient);
        udpListener.OnMessageArrival += packetProcessor.ProcessIncomingPacket;
        udpListener.StartListening();

        while (Console.ReadLine() is { } inputLine)
        {
            try
            {
                var model = msgParser.ParseMessage(inputLine);
                try
                {
                    var result = payloadBuilder.GetPayloadFromMessage(model);
                    Console.WriteLine(BitConverter.ToString(result));

                    try
                    {
                        udpTransfer.SendMessage(result);
                    } catch (Exception e)
                    {
                        Console.WriteLine($"Error sending message: {e.Message}");
                    }
                }
                catch (NotSupportedException)
                {
                    if (model.MessageType == MessageType.HELP)
                    {
                        resultLogger.PrintHelp();
                    } else if (model.MessageType == MessageType.RENAME)
                    {
                        user.DisplayName = model.Parameters["displayName"];
                        Console.WriteLine($"New display name: {user.DisplayName}");
                    }
                }

            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

}