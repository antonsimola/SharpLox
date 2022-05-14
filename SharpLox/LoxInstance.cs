namespace SharpLox;

public class LoxInstance
{
    private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
    private readonly LoxClass _klass;

    public LoxInstance(LoxClass klass)
    {
        _klass = klass;
    }

    public override string ToString()
    {
        return _klass._name + " instance";
    }

    public object Get(Token name)
    {
        if (fields.TryGetValue(name.Lexeme, out var res))
        {
            return res;
        }

        var method = _klass.FindMethod(name.Lexeme);
        if (method != null)
        {
            return method.Bind(this);
        }

        throw new RuntimeException(name, "Undefined property '" + name.Lexeme + "'.");
    }

    public void Set(Token token, object val)
    {
        fields[token.Lexeme] = val;
    }
}