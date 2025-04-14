using System.Net;

namespace IPK25_chat.CLI;

public class ArgumentChecker
{
    private const string HelpMessage =
        "Usage: ./ipk25chat -t <udp/tcp> -s <ip/hostname> [-p <port>] [-d <timeout>] [-r <retries>] [-h]\n" +
        "Options:\n" +
        "   -t          Transport protocol used for connection (tcp/udp)\n" +
        "   -s          Server IP or hostname\n" +
        "   -p          Server port - DEFAULT: 4567\n" +
        "   -d          UDP confirmation timeout (in ms) - DEFAULT: 250\n" +
        "   -r          Maximum number of UDP retries - DEFAULT: 3\n" +
        "   -h          Prints program help output and exits\n";
    
    
    public ArgumentOptions CheckArguments(ArgumentOptions? args) 
    {
        if (args == null)
        {
            Console.WriteLine(HelpMessage);
            Environment.Exit(1);
        } 
        
        if (args.Help)
        {
            Console.WriteLine(HelpMessage);     
            Environment.Exit(0);
        }

        if (args.Protocol is not ("tcp" or "udp"))
        {
            Console.WriteLine("Invalid transport protocol. Use /help for more information."); 
            Environment.Exit(1);
        }

        if (!IsValidIpOrHostname(args.ServerAddress))
        {
            Console.WriteLine("Invalid server address. Use /help for more information."); 
            Environment.Exit(1);
        }

        return args;
    }

    private static bool IsValidIpOrHostname(string serverAddress)
    {
        if (IPAddress.TryParse(serverAddress, out _))
        {
            return true;
        }

        var hostEntry = Dns.GetHostAddresses(serverAddress);
        return hostEntry.Length != 0;
    }
}