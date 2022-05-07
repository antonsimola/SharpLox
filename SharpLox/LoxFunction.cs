using SharpLox.AbstractSyntaxTree;

namespace SharpLox;

public class LoxFunction : ILoxCallable
{
    private readonly FunctionStatement _functionDeclaration;
    private readonly Environment _closure;
    public int Arity { get; }

    public LoxFunction(FunctionStatement functionDeclaration, Environment closure)
    {
        Arity = functionDeclaration.Params.Count;
        _functionDeclaration = functionDeclaration;
        _closure = closure;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        Environment environment = new Environment(_closure);
        for (int i = 0; i < _functionDeclaration.Params.Count; i++)
        {
            environment.Define(_functionDeclaration.Params[i].Lexeme,
                arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_functionDeclaration.Body, environment);
        }
        catch (ReturnException re)
        {
            return re.Value;
        }
        
        return null;
    }

    public override string ToString()
    {
        return $"<fn {_functionDeclaration.Name.Lexeme}>";
    }
}