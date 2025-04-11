namespace IPK25_chat.Models;

public class UserModel
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

    public UserModel(string username, string displayName)
    {
        _username = username;
        _displayName = displayName;
    }
}