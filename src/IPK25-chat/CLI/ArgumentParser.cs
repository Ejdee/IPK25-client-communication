using CommandLine;

namespace IPK25_chat.CLI
{
    public class ArgumentParser
    {
        public ArgumentOptions? ParsedOptions { get; private set; }
        
        public void Parse(string[] args)
        {
            Parser parser = new Parser(settings => settings.AllowMultiInstance = false);
            
            parser.ParseArguments<ArgumentOptions>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
        }

        private void RunOptions(ArgumentOptions? opts)
        {
            ParsedOptions = opts;
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine("Parsing arguments failed.");
            var enumerable = errs as Error[] ?? errs.ToArray();
            for (int i = 0; i < enumerable.Count(); i++)
            {
                Console.WriteLine(enumerable.ElementAt(i));
            }
        }
    }
}