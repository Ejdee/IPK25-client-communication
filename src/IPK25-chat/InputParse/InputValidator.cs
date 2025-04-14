using System.Text.RegularExpressions;

namespace IPK25_chat.InputParse;

public class InputValidator
{
    public string ValidateInput(string input, int maxLength, string regex)
    {
        if (!Regex.IsMatch(input, regex))
        {
            Console.WriteLine("ERROR: Invalid input format.");   
        } 
        
        if (input.Length > maxLength)
        {
            Console.WriteLine($"Input exceeds maximum length of {maxLength} characters. Truncating to {maxLength} characters.");
            return input.Substring(0, maxLength);
        }

        return input;
    }
}