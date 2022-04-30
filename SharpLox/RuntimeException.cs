namespace SharpLox;

public class RuntimeException: Exception
{
    private readonly string _message;
    public Token Token { get; set; }

    public RuntimeException(Token token, string message)
    {
        Token = token;
        _message = message;
    }
    
    public string GetMessage()
    {
        return _message;
    }
    
}