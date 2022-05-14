using SharpLox.AbstractSyntaxTree;

namespace SharpLox;

using static TokenType;

public class Parser
{
    private readonly IList<Token> _tokens;
    private int current = 0;

    public Parser(IList<Token> tokens)
    {
        _tokens = tokens;
    }

    public List<Statement> Parse()
    {
        List<Statement> statements = new List<Statement>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }


    private Statement Declaration()
    {
        try
        {
            if (Match(CLASS)) return ClassDeclaration();
            if (Match(FUN)) return FunctionDeclaration("function");
            if (Match(VAR)) return VariableDeclaration();

            return Statement();
        }
        catch (ParseException error)
        {
            Synchronize();
            return null;
        }
    }

    private Statement ClassDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expect class name.");
        Consume(LEFT_BRACE, "Expect '{' before class body.");

        List<FunctionStatement> methods = new List<FunctionStatement>();
        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(FunctionDeclaration("method"));
        }

        Consume(RIGHT_BRACE, "Expect '}' after class body.");

        return new ClassStatement(name, methods);
    }

    private FunctionStatement FunctionDeclaration(string kind)
    {
        Token name = Consume(IDENTIFIER, $"Expect {kind} name.");

        Consume(LEFT_PAREN, "Expect '(' after " + kind + " name.");

        List<Token> parameters = new List<Token>();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(
                    Consume(IDENTIFIER, "Expect parameter name."));
            } while (Match(COMMA));
        }

        Consume(RIGHT_PAREN, "Expect ')' after parameters.");

        Consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");
        List<Statement> body = Block();
        return new FunctionStatement(name, parameters, body);
    }

    private Statement VariableDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expect variable name.");

        Expression initializer = null;
        if (Match(EQUAL))
        {
            initializer = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after variable declaration.");
        return new VariableStatement(name, initializer);
    }

    private Statement Statement()
    {
        if (Match(IF)) return IfStatement();
        if (Match(WHILE)) return WhileStatement();
        if (Match(FOR)) return ForStatement();
        if (Match(RETURN)) return ReturnStatement();
        if (Match(PRINT)) return PrintStatement();
        if (Match(BREAK)) return BreakStatement();
        if (Match(LEFT_BRACE)) return new BlockStatement(Block());

        return ExpressionStatement();
    }

    private Statement ReturnStatement()
    {
        var keyword = Previous();
        Expression value = null;
        if (!Check(SEMICOLON))
        {
            value = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after return value.");
        return new ReturnStatement(keyword, value);
    }


    private BreakStatement BreakStatement()
    {
        Consume(SEMICOLON, "Expect ; after break ");
        return new BreakStatement();
    }

    private Statement ForStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'for'.");
        Statement initializer;
        if (Match(SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(VAR))
        {
            initializer = VariableDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expression condition = null;
        if (!Check(SEMICOLON))
        {
            condition = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after loop condition.");

        Expression increment = null;
        if (!Check(RIGHT_PAREN))
        {
            increment = Expression();
        }

        Consume(RIGHT_PAREN, "Expect ')' after for clauses.");
        Statement body = Statement();

        if (increment != null)
        {
            body = new BlockStatement(
                new List<Statement>()
                {
                    body,
                    new ExpressionStatement(increment)
                });
        }

        if (condition == null) condition = new LiteralExpression(true);
        body = new WhileStatement(condition, body);

        if (initializer != null)
        {
            body = new BlockStatement(new List<Statement>() { initializer, body });
        }

        return body;
    }

    private WhileStatement WhileStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'while'.");
        Expression condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after while condition.");
        Statement statement = Statement();

        return new WhileStatement(condition, statement);
    }

    private IfStatement IfStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'if'.");
        Expression condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after if condition.");

        Statement thenBranch = Statement();
        Statement elseBranch = null;
        if (Match(ELSE))
        {
            elseBranch = Statement();
        }

        return new IfStatement(condition, thenBranch, elseBranch);
    }

    private List<Statement> Block()
    {
        List<Statement> statements = new List<Statement>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private PrintStatement PrintStatement()
    {
        Expression value = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new PrintStatement(value);
    }

    private ExpressionStatement ExpressionStatement()
    {
        Expression expr = Expression();
        Consume(SEMICOLON, "Expect ';' after expression.");
        return new ExpressionStatement(expr);
    }

    private Expression Expression()
    {
        return Assignment();
    }

    private Expression Assignment()
    {
        Expression expr = Or();

        if (Match(EQUAL))
        {
            Token equals = Previous();
            Expression value = Assignment();

            if (expr is VariableExpression ve)
            {
                Token name = ve.Name;
                return new AssignExpression(name, value);
            }
            
            if (expr is GetExpression ge)
            {
                return new SetExpression(ge.Object, ge.Name, value);
            }

            error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expression Or()
    {
        Expression expr = And();
        while (Match(OR))
        {
            Token oper = Previous();
            Expression right = And();
            expr = new LogicalExpression(expr, oper, right);
        }

        return expr;
    }

    private Expression And()
    {
        Expression expr = Equality();

        while (Match(AND))
        {
            Token oper = Previous();
            Expression right = Equality();
            expr = new LogicalExpression(expr, oper, right);
        }

        return expr;
    }

    private Expression Equality()
    {
        var expr = Comparison();
        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            Token oper = Previous();
            Expression right = Comparison();
            expr = new BinaryExpression(expr, oper, right);
        }

        return expr;
    }

    private Expression Comparison()
    {
        Expression expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            Token oper = Previous();
            Expression right = Term();
            expr = new BinaryExpression(expr, oper, right);
        }

        return expr;
    }

    private Expression Term()
    {
        Expression expr = Factor();

        while (Match(MINUS, PLUS))
        {
            Token oper = Previous();
            Expression right = Factor();
            expr = new BinaryExpression(expr, oper, right);
        }

        return expr;
    }

    private Expression Factor()
    {
        Expression expr = Unary();

        while (Match(SLASH, STAR))
        {
            Token oper = Previous();
            Expression right = Unary();
            expr = new BinaryExpression(expr, oper, right);
        }

        return expr;
    }

    private Expression Unary()
    {
        if (Match(BANG, MINUS))
        {
            Token oper = Previous();
            AbstractSyntaxTree.Expression right = Unary();
            return new UnaryExpression(oper, right);
        }

        return Call();
    }

    private Expression Call()
    {
        Expression expression = Primary();
        while (true)
        {
            if (Match(LEFT_PAREN))
            {
                expression = FinishCall(expression);
            }
            else if (Match(DOT))
            {
                Token name = Consume(IDENTIFIER, "Expect property name after '.' .");
                expression = new GetExpression(expression, name);
            }
            else
            {
                break;
            }
        }

        return expression;
    }

    private Expression FinishCall(Expression callee)
    {
        List<Expression> arguments = new List<Expression>();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    error(Peek(), "Can't have more than 255 arguments.");
                }

                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        Token paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

        return new CallExpression(callee, paren, arguments);
    }

    private Expression Primary()
    {
        if (Match(FALSE))
        {
            return new LiteralExpression(false);
        }

        if (Match(TRUE))
        {
            return new LiteralExpression(true);
        }

        if (Match(THIS)) return new ThisExpression(Previous());

        if (Match(NUMBER, STRING))
        {
            return new LiteralExpression(Previous().Literal);
        }

        if (Match(IDENTIFIER))
        {
            return new VariableExpression(Previous());
        }

        if (Match(LEFT_PAREN))
        {
            var expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new GroupingExpression(expr);
        }

        throw error(Peek(), "Expect expression");
    }

    private Token Consume(TokenType type, String message)
    {
        if (Check(type)) return Advance();

        throw error(Peek(), message);
    }

    private Exception error(Token token, String message)
    {
        Diagnostics.Error(token, message);
        return new ParseException(token, message);
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private Token Previous()
    {
        return _tokens[current - 1];
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Peek()
    {
        return _tokens[current];
    }

    private bool IsAtEnd()
    {
        return Peek().Type == EOF;
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == SEMICOLON) return;

            switch (Peek().Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }
}

public class ParseException : Exception
{
    private readonly Token _token;
    private readonly string _message;

    public ParseException(Token token, string message)
    {
        _token = token;
        _message = message;
    }

    public override string ToString()
    {
        return _token.ToString() + " " + _message;
    }
}