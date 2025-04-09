namespace IPK25_chat.Models;

public class User
{
    private string _username;
    private string _displayName;
    
    public string Username
    {
        get => _username;
        set => _username = value;
    }
    
    public string DisplayName
    {
        get => _displayName;
        set => _displayName = value;
    }

    public User(string username, string displayName)
    {
        _username = username;
        _displayName = displayName;
    }
}