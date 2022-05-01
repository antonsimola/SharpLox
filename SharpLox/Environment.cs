namespace SharpLox;

public class Environment
{
    private readonly Environment? _enclosing;

    private IDictionary<string, object> _values { get; set; } = new Dictionary<string, object>();


    public Environment()
    {
    }

    public Environment(Environment enclosing)
    {
        _enclosing = enclosing;
    }

    public void Define(string name, object value)
    {
        _values[name] = value;
    }

    public object Get(string name)
    {
        if (_values.ContainsKey(name))
        {
            return _values[name];
        }

        if (_enclosing != null)
        {
            return _enclosing.Get(name);
        } 

        throw new RuntimeException(name, $"Undefined variable {name}.");
    }

    public void Assign(Token name, object value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }
        
        if (_enclosing != null)
        {
            _enclosing.Assign(name, value);
            return;
        } 

        throw new RuntimeException(name, $"Trying to set variable that does not exists {name.Lexeme}.");
    }
}