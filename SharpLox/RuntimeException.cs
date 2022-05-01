namespace SharpLox;

public class RuntimeException: Exception
{
    private readonly string _message;
    public Token Token { get; set; }
    public string Name { get; set; }

    public RuntimeException(Token token, string message)
    {
        Token = token;
        _message = message;
    }
    
    public RuntimeException(string name, string message)
    {
        Name = name;
        _message = message;
    }
    
    public string GetMessage()
    {
        return Name + " " + _message;
    }
    
}