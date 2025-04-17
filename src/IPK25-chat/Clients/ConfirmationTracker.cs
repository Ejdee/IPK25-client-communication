using System.Collections.Concurrent;

namespace IPK25_chat.Clients;

public class ConfirmationTracker
{
    private ConcurrentDictionary<string, ManualResetEventSlim> _confirmations = new();

    public void NewConfirmationWait(string messageId)
        => _confirmations[messageId] = new ManualResetEventSlim(false);

    public bool WaitForConfirmation(string messageId, int timeout)
    {
        if (_confirmations.TryGetValue(messageId, out var res))
        {
            return res.Wait(timeout);
        }

        return false;
    }
    
    public void SetConfirmation(string messageId)
    {
        if (_confirmations.TryGetValue(messageId, out var res))
        {
            res.Set();
        }
    }
    
    public void RemoveConfirmation(string messageId)
    {
        if (_confirmations.TryRemove(messageId, out var res))
        {
            res.Dispose();
        }
    }
}