using System.Net.Sockets;
using IPK25_chat.Protocol;

namespace IPK25_chat.Client;

public class TcpTransfer : IClient
{
    private readonly UdpProtocolPayloadBuilder _payloadBuilder;
    private readonly TcpClient _tcpClient;
    
    public TcpTransfer(UdpProtocolPayloadBuilder payloadBuilder)
    {
        _payloadBuilder = payloadBuilder;
        _tcpClient = new TcpClient();
    }
    
    public void SendMessage(byte[] payload)
    {
        if (_tcpClient.Connected)
        {
            NetworkStream stream = _tcpClient.GetStream();
            stream.Write(payload, 0, payload.Length);
            stream.Flush();
        }
        else
        {
            throw new InvalidOperationException("TCP client is not connected.");
        }
    }

    public void SendErrorMessage(byte[]? contents = null)
    {
        var payload = _payloadBuilder.CreateErrPacket(contents); 
        SendMessage(payload);
    }
    
    public void Dispose()
    {
        _tcpClient.Dispose();
    }
}