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
        var interpreter = new Interpreter();
        var resolver = new Resolver(interpreter);
        resolver.Resolve(statements);
        
        interpreter.Interpret(statements);
        var result = stringWriter.ToString().Trim();
        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
        standardOutput.AutoFlush = true;
        Console.SetOut(standardOutput);
        return result;
    }
    
}