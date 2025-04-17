using System.Net;
using System.Net.Sockets;
using IPK25_chat.Clients;

namespace IPK25_chat.Models;

public class UdpClientConfig
{
    public int RetryCount { get; set; }
    public int RetryDelay { get; set; }
    public IPEndPoint ServerEndPoint { get; set; }
    public ConfirmationTracker ConfirmationTracker { get; set; }
    public UdpClient UdpClient { get; set; }

    public UdpClientConfig(int retryCount, int retryDelay, IPEndPoint serverEndPoint,
        ConfirmationTracker confirmationTracker, UdpClient client)
    {
        RetryCount = retryCount;
        RetryDelay = retryDelay;
        ServerEndPoint = serverEndPoint;
        ConfirmationTracker = confirmationTracker;
        UdpClient = client;
    }
    
}