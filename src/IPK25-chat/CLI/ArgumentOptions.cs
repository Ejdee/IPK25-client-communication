using CommandLine;

namespace IPK25_chat.CLI
{
    public class ArgumentOptions
    {
        [Option('t', "transport", Required = false, HelpText = "Transport protocol to use (tcp/udp).")]
        public string Protocol { get; set; } = string.Empty;
        
        [Option('s', "ip-or-hostname", Required = false, HelpText = "Server IP or hostname")]
        public string  ServerAddress { get; set; } = string.Empty;
        
        [Option('p', "port", Default=4567, Required = false, HelpText = "Server port.")]
        public int Port { get; set; } = 4567;
        
        [Option('d', "udp-timeout", Default=250, Required = false, HelpText = "UDP confirmation timeout in milliseconds.")]
        public int UdpTimeout { get; set; } = 250;
        
        [Option('r', "udp-retry", Required = false, HelpText = "Maximum number of UDP retries.")]
        public int UdpRetries { get; set; } = 3;
        
        [Option('h', "help", Required = false, HelpText = "Display help message.")]
        public bool Help { get; set; }
    }
}

