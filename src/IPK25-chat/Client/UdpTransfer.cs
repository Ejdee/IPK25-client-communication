using System.Net;
using IPK25_chat.Models;
using IPK25_chat.PayloadBuilders;

namespace IPK25_chat.Client;

public class UdpTransfer : IClient
{
    private readonly IProtocolPayloadBuilder _payloadBuilder;
    private readonly UdpClientConfig _udpConfig;
    
    public UdpTransfer(UdpClientConfig udpConfig, IProtocolPayloadBuilder payloadBuilder)
    {
        _udpConfig = udpConfig;
        _payloadBuilder = payloadBuilder;
    }
    
    public void SendMessage(byte[] payload)
    {
        var id = $"{payload[1]}{payload[2]}";
        _udpConfig.ConfirmationTracker.NewConfirmationWait(id);
        _udpConfig.UdpClient.Client.ReceiveTimeout = _udpConfig.RetryDelay;
        
        for (int attempt = 0; attempt < _udpConfig.RetryCount; attempt++)
        {
            _udpConfig.UdpClient.Send(payload, payload.Length, _udpConfig.ServerEndPoint);

            if (_udpConfig.ConfirmationTracker.WaitForConfirmation(id, _udpConfig.RetryDelay))
            {
                _udpConfig.ConfirmationTracker.RemoveConfirmation(id);
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
        _udpConfig.UdpClient.Send(payload, payload.Length, _udpConfig.ServerEndPoint);
    }

    public void SendErrorMessage(byte[]? contents = null)
    {
        var payload = _payloadBuilder.CreateErrPacket(contents); 
        SendMessage(payload);
    }
    
    public void SetRemoteEndPoint(IPEndPoint remoteEndPoint)
    {
        _udpConfig.ServerEndPoint = remoteEndPoint;
    }

    public void Dispose()
    {
        _udpConfig.UdpClient.Close();
        _udpConfig.UdpClient.Dispose();
    }
}