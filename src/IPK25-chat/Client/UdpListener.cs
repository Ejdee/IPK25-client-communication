using System.Net;
using System.Net.Sockets;
using IPK25_chat.Core;
using IPK25_chat.Enums;

namespace IPK25_chat.Client;

public class UdpListener
{
   private UdpClient _udpClient;
   private HashSet<string> _processedMessageIds;
   private CancellationTokenSource _cancellationTokenSource = new();
   private readonly UdpTransfer _udpTransfer;

   public event Action<bool, byte[]>? OnMessageArrival; 
   
   public UdpListener(UdpClient udpClient, UdpTransfer udpTransfer)
   {
      _udpClient = udpClient;
      _udpTransfer = udpTransfer;
      _processedMessageIds = new HashSet<string>();
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
               var remote = result.RemoteEndPoint;
               
               var data = result.Buffer;

               if (data[0] == (byte)PayloadType.REPLY)
               {
                  _udpTransfer.SetRemoteEndPoint(remote);
               }

               var id = GetMessageId(data);
               if (_processedMessageIds.Add(id))
               {
                  OnMessageArrival?.Invoke(true, data);
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
      _udpClient.Dispose();
   } 
   
   private string GetMessageId(byte[] data) => $"{data[1]}{data[2]}";
}