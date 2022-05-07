namespace SharpLox;

public interface ILoxCallable
{
    
    int Arity { get; }
    Object Call(Interpreter interpreter, List<object> arguments);
}