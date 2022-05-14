using SharpLox.AbstractSyntaxTree;

namespace SharpLox;

public class LoxFunction : ILoxCallable
{
    private readonly FunctionStatement _functionDeclaration;
    private readonly Environment _closure;
    private readonly bool _isInitializer;
    public int Arity { get; }

    public LoxFunction(FunctionStatement functionDeclaration, Environment closure, bool isInitializer)
    {
        Arity = functionDeclaration.Params.Count;
        _functionDeclaration = functionDeclaration;
        _closure = closure;
        _isInitializer = isInitializer;
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
            if (_isInitializer) return _closure.GetAt(0, "this");
            
            return re.Value;
        }
        
        if (_isInitializer) return _closure.GetAt(0, "this");

        
        return null;
    }

    public override string ToString()
    {
        return $"<fn {_functionDeclaration.Name.Lexeme}>";
    }

    public LoxFunction Bind(LoxInstance loxInstance)
    {
        Environment environment = new Environment(_closure);
        environment.Define("this", loxInstance);
        return new LoxFunction(_functionDeclaration, environment, _isInitializer);
    }
}