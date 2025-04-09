
using IPK25_chat.CLI;
using IPK25_chat.Logger;
using IPK25_chat.Models;
using IPK25_chat.Parsers;
using IPK25_chat.Protocol;

namespace IPK25_chat;

internal abstract class Program
{
    private string _displayName = "Frodo";
    
    public static void Main(string[] args)
    {
        ArgumentParser parser = new();
        parser.Parse(args);

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

        while (Console.ReadLine() is { } inputLine)
        {
            try
            {
                var model = msgParser.ParseMessage(inputLine);
                try
                {
                    var result = payloadBuilder.GetPayloadFromMessage(model);
                    Console.WriteLine(BitConverter.ToString(result));
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