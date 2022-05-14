namespace SharpLox;

public class LoxClass : ILoxCallable
{
    public string _name;
    private readonly Dictionary<string, LoxFunction> _methods;

    public LoxClass(string name, Dictionary<string, LoxFunction> methods)
    {
        _name = name;
        _methods = methods;
    }

    public override string ToString()
    {
        return _name;
    }

    public int Arity
    {
        get
        {
            LoxFunction initializer = FindMethod("init");
            if (initializer == null) return 0;
            return initializer.Arity;
        }
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new LoxInstance(this);
        LoxFunction initializer = FindMethod("init");
        if (initializer != null)
        {
            initializer.Bind(instance).Call(interpreter, arguments);
        }

        return instance;
    }

    public LoxFunction FindMethod(string nameLexeme)
    {
        if (_methods.TryGetValue(nameLexeme, out var res))
        {
            return res;
        }

        return null;
    }
}