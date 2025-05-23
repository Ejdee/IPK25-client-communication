using System.Net.Sockets;
using IPK25_chat.Clients.Interfaces;
using IPK25_chat.Enums;

namespace IPK25_chat.Clients;

public class UdpListener : IDisposable, IListener
{
   private UdpClient _udpClient;
   private HashSet<string> _processedMessageIds;
   private CancellationTokenSource _cancellationTokenSource = new();
   private readonly UdpTransfer _udpTransfer;
   private const int MinPacketLength = 3;

   public event Action<bool, byte[]>? OnMessageArrival; 
   
   public UdpListener(UdpClient udpClient, UdpTransfer udpTransfer)
   {
      _udpClient = udpClient;
      _udpTransfer = udpTransfer;
      _processedMessageIds = new HashSet<string>();
   }

   public void StartListening()
   {
      //Console.WriteLine("UDP Listener started...");
      Task.Run(async () =>
      {
         while (!_cancellationTokenSource.IsCancellationRequested)
         {
            try
            {
               var result = await _udpClient.ReceiveAsync();
               var remote = result.RemoteEndPoint;
               
               var data = result.Buffer;
               
               if (data.Length < MinPacketLength)
               {
                  // Not an IPK25 packet
                  continue;
               }

               if (data[0] == (byte)PayloadType.REPLY)
               {
                  _udpTransfer.SetRemoteEndPoint(remote);
               }

               // if the message is confirmed, we must not put the id in the set
               // because it is id of the message this program sent
               if (data[0] == (byte)PayloadType.CONFIRM)
               {
                  OnMessageArrival?.Invoke(true, data);
                  continue;
               }
               
               // send confirmation no matter if the message is invalid or not
               _udpTransfer.SendConfirm(data[1], data[2]);
               
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
   } 
   
   private string GetMessageId(byte[] data) => $"{data[1]}{data[2]}";

   public void Dispose()
   {
      _udpClient.Dispose();
      _cancellationTokenSource.Dispose();
      _udpTransfer.Dispose();
   }
}