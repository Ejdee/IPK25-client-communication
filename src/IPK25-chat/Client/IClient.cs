using System.Net;

namespace IPK25_chat.Client;

public interface IClient : IDisposable
{
    void SendMessage(byte[] payload);
    byte[] ReceiveMessage();
}