namespace IPK25_chat.Clients.Interfaces;

public interface IListener
{
    public void StartListening();
    public void Dispose();
    public void StopListening();
    public event Action<bool, byte[]>? OnMessageArrival;
}