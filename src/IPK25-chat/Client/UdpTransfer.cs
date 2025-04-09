using System.Net;
using System.Net.Sockets;

namespace IPK25_chat.Client;

public class UdpTransfer : IClient
{
    private UdpClient _udpClient;
    private IPEndPoint _remoteEndPoint;
    private ConfirmationTracker _confirmationTracker;
    private int _retryCount;
    private int _retryDelay;
    
    public UdpTransfer(int retryCount, int retryDelay, IPEndPoint serverEndPoint, ConfirmationTracker confirmationTracker)
    {
        _udpClient = new UdpClient(0);
        _remoteEndPoint = serverEndPoint;
        _confirmationTracker = confirmationTracker;
        _retryCount = retryCount;
        _retryDelay = retryDelay;
    }
    
    public UdpClient UdpClient => _udpClient;
    
    public void SendMessage(byte[] payload)
    {
        var id = $"{payload[1]}{payload[2]}";
        _confirmationTracker.NewConfirmationWait(id);
        
        for (int attempt = 0; attempt < _retryCount; attempt++)
        {
            _udpClient.Send(payload, payload.Length, _remoteEndPoint);
            _udpClient.Client.ReceiveTimeout = _retryDelay;

            if (_confirmationTracker.WaitForConfirmation(id, _retryDelay))
            {
                _confirmationTracker.RemoveConfirmation(id);
                Console.WriteLine("Message sent and acknowledged.");
                return;
            }
            else
            {
                Console.WriteLine("Message not acknowledged, retrying...");
            }
        }
    }

    public void SendConfirm(byte idPart1, byte idPart2)
    {
        var payload = new byte[]{ 0x00, idPart1, idPart2 };
    }

    public byte[] ReceiveMessage()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        var data = _udpClient.Receive(ref remoteEndPoint);
        _remoteEndPoint = remoteEndPoint;
        return data;
    }

    public void Dispose()
    {
        _udpClient.Close();
        _udpClient.Dispose();
    }
}