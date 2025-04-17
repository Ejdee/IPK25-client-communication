
namespace IPK25_chat.Clients.Interfaces;

public interface IClient : IDisposable
{
    void SendMessage(byte[] payload);
    void SendErrorMessage(byte[]? contents = null);
}