// See https://aka.ms/new-console-template for more information

using System.Threading.Channels;
using SharpLox;

if (args.Length == 0)
{
    RunPrompt();
} else if (args.Length == 1)
{
    RunFile(args[1]);
}


void RunPrompt()
{

    
    while (true)
    {
        Console.Write("> ");
        var line = Console.ReadLine();
        
        if (string.IsNullOrEmpty(line)) break;
        Run(line);
    }
}


void RunFile(string path )
{
    var script = File.ReadAllText(path);
    
}

void Run(string line )
{
    var t = new Tokenizer(line);
    t.GetTokens().ForEach(Console.WriteLine);
}

