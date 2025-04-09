namespace IPK25_chat.Logger;

public class ResultLogger
{
    const string HelpMessage = @"
    Supported Commands:
    /auth {Username} {Secret} {DisplayName}
        Sends an AUTH message with the provided data to the server.
        Correctly handles the Reply message and locally sets the DisplayName value (same as the /rename command).

    /join {ChannelID}
        Sends a JOIN message with the specified channel name to the server.
        Correctly handles the Reply message.

    /rename {DisplayName}
        Locally changes the display name of the user.
        The new display name will be sent with subsequent messages or selected commands.

    /help
        Prints out the list of supported commands, their parameters, and descriptions.
    ";    
    
    public void PrintHelp()
    {
        Console.WriteLine(HelpMessage);
    }
}