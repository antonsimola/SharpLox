using System.Globalization;

namespace SharpLox;

using static TokenType;

public class Tokenizer
{
    private readonly string _source;
    public List<Token> Tokens { get; set; } = new List<Token>();
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>()
    {
        { "and", AND },
        { "class", CLASS },
        { "else", ELSE },
        { "false", FALSE },
        { "for", FOR },
        { "fun", FUN },
        { "if", IF },
        { "nil", NIL },
        { "or", OR },
        { "print", PRINT },
        { "return", RETURN },
        { "super", SUPER },
        { "this", THIS },
        { "true", TRUE },
        { "var", VAR },
        { "while", WHILE }
    };

    public Tokenizer(string source)
    {
        _source = source;
    }

   public  List<Token> GetTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }


        Tokens.Add(new Token(EOF, "", null, _line));
        return Tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(':
                AddToken(LEFT_PAREN);
                break;
            case ')':
                AddToken(RIGHT_PAREN);
                break;
            case '{':
                AddToken(LEFT_BRACE);
                break;
            case '}':
                AddToken(RIGHT_BRACE);
                break;
            case ',':
                AddToken(COMMA);
                break;
            case '.':
                AddToken(DOT);
                break;
            case '-':
                AddToken(MINUS);
                break;
            case '+':
                AddToken(PLUS);
                break;
            case ';':
                AddToken(SEMICOLON);
                break;
            case '*':
                AddToken(STAR);
                break;
            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;
            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(SLASH);
                }

                break;
            case '"':
                HandleString();
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                break;

            default:
                if (IsDigit(c))
                {
                    HandleNumber();
                }

                else if (IsAlpha(c))
                {
                    HandleIdentifier();
                }

                else
                {
                    Diagnostics.Error(_line, $"Unexpected character {c}");
                }

                break;
        }
    }

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private void HandleIdentifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        
        String text = _source[_start .. _current];
        var type = IDENTIFIER;
        if (Keywords.TryGetValue(text, out var t))
        {
            type = t;
        }
        
        AddToken(type);
    }

    private void HandleNumber()
    {
        while (IsDigit(Peek())) Advance();

        // Look for a fractional part.
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume the "."
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        AddToken(NUMBER, Double.Parse(_source[_start .. _current], CultureInfo.InvariantCulture));
    }

    private char PeekNext()
    {
        if (_current + 1 >= _source.Length) return '\0';
        return _source[_current + 1];
    }

    private void HandleString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                _line++;
            }

            Advance();
        }

        if (IsAtEnd())
        {
            Diagnostics.Error(_line, "Unterminated string.");
            return;
        }

        // The closing ".
        Advance();

        // Trim the surrounding quotes.
        string value = _source[(_start + 1) .. (_current - 1)];
        AddToken(STRING, value);
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    private bool Match(char c)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != c) return false;

        _current++;
        return true;
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, Object literal)
    {
        string text = _source[_start ..  _current];
        Tokens.Add(new Token(type, text, literal, _line));
    }

    private char Advance()
    {
        return _source[_current++];
    }

    private bool IsAtEnd()
    {
        return _current >= _source.Length;
    }
}