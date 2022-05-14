namespace SharpLox;

public class LoxScriptEngine
{

    public string Evaluate(string code)
    {
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var tokenizer = new Tokenizer(code);
        var tokens = tokenizer.GetTokens();
        var parser = new Parser(tokens);
        var statements = parser.Parse();
        
        if (Diagnostics.HadError)
        {
            Diagnostics.HadError = false;
            return "Had error";
        }
        var interpreter = new Interpreter();
        var resolver = new Resolver(interpreter);
        resolver.Resolve(statements);

        if (Diagnostics.HadError)
        {
            Diagnostics.HadError = false;
            return "Had error";
        }
        
        interpreter.Interpret(statements);
        var result = stringWriter.ToString().Trim();
        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
        standardOutput.AutoFlush = true;
        Console.SetOut(standardOutput);
        return result;
    }
    
}