using System.Net.Sockets;
using IPK25_chat.Clients.Interfaces;

namespace IPK25_chat.Clients;

public class TcpListener : IDisposable, IListener
{
    private readonly TcpClient _tcpClient;
    private CancellationTokenSource _cancelToken = new();
    
    public event Action<bool, byte[]>? OnMessageArrival;

    public TcpListener(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
    }

    public void StartListening()
    {
        Task.Run(async () =>
        {
            await using var stream = _tcpClient.GetStream();
            List<byte> messageBuffer = new();
            var buffer = new byte[1024];

            try
            {
                while (!_cancelToken.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, _cancelToken.Token);

                    if (bytesRead == 0)
                        break;

                    messageBuffer.AddRange(buffer.Take(bytesRead));

                    while (TryExtractMessage(messageBuffer, out var extractedMessage))
                    {
                        OnMessageArrival?.Invoke(true, extractedMessage);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
    }

    public bool TryExtractMessage(List<byte> messageBuffer, out byte[] extractedMessage)
    {
        extractedMessage = Array.Empty<byte>();

        for (int i = 0; i < messageBuffer.Count-1; i++)
        {
            if(messageBuffer[i] == '\r' && messageBuffer[i+1] == '\n')
            {
                var msgLengthWithTheClrf = i + 2;
                extractedMessage = messageBuffer.Take(msgLengthWithTheClrf).ToArray();
                messageBuffer.RemoveRange(0, msgLengthWithTheClrf);
                return true;
            }
        }

        return false;
    }

    public void StopListening()
    {
        _cancelToken.Cancel();
    }

    public void Dispose()
    {
        _tcpClient.Dispose();
        _cancelToken.Dispose();
    }
}