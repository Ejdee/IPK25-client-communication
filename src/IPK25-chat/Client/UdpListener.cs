using System.Net;
using System.Net.Sockets;

namespace IPK25_chat.Client;

public class UdpListener
{
   private UdpClient _udpClient;
   private List<string> _processedMessageIds;
   private CancellationTokenSource _cancellationTokenSource = new();

   public event Action<byte[]>? OnMessageArrival; 
   
   public UdpListener(UdpClient udpClient)
   {
      _udpClient = udpClient;
      _processedMessageIds = new List<string>();
   }

   public void StartListening()
   {
      Console.WriteLine("UDP Listener started...");
      Task.Run(async () =>
      {
         while (!_cancellationTokenSource.IsCancellationRequested)
         {
            try
            {
               var result = await _udpClient.ReceiveAsync();
               var data = result.Buffer;

               var id = GetMessageId(data);
               if (!_processedMessageIds.Contains(id))
               {
                  _processedMessageIds.Add(id);
                  OnMessageArrival?.Invoke(data);
               }
            }
            catch (Exception e)
            {
               Console.WriteLine(e);
            }
            
         }
      });
   }

   public void StopListening()
   {
      _cancellationTokenSource.Cancel();
      _udpClient.Close();
   } 
   
   private string GetMessageId(byte[] data) => $"{data[1]}{data[2]}";
}