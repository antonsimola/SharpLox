﻿using SharpLox.AbstractSyntaxTree;

namespace SharpLox.Parser;

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
        try {
            if (Match(VAR)) return VariableDeclaration();

            return Statement();
        } catch (ParseException error) {
            Synchronize();
            return null;
        }  
    }

    private Statement VariableDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expect variable name.");
        
        Expression initializer = null;
        if (Match(EQUAL)) {
            initializer = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after variable declaration.");
        return new VariableStatement(name, initializer);
    }

    private Statement Statement()
    {
        
        if (Match(PRINT)) return PrintStatement();
        
        if (Match(LEFT_BRACE)) return  new BlockStatement(Block());

        return ExpressionStatement();
    }

    private List<Statement> Block()
    {
        List<Statement> statements = new List<Statement>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd()) {
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
        Expression expr = Equality();

        if (Match(EQUAL)) {
            Token equals = Previous();
            Expression value = Assignment();

            if (expr is VariableExpression ve) {
                Token name = ve.Name;
                return new AssignExpression(name, value);
            }

            error(equals, "Invalid assignment target."); 
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

        return Primary();
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
        return new ParseException();
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
}