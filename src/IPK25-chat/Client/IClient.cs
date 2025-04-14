
namespace IPK25_chat.Client;

public interface IClient : IDisposable
{
    void SendMessage(byte[] payload);
    void SendErrorMessage(byte[]? contents = null);
}