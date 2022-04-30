using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SharpLox;
using SharpLox.AbstractSyntaxTree;
using SharpLox.Parser;

namespace SharpLoxTests;

public class TestVisitorPattern
{

    public class AstPrinter : IVisitor<string>
    {
        
        private String parenthesize(String name, params Expression[] exprs) {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach  (Expression expr in exprs) {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
        public string VisitBinaryExpression(BinaryExpression binaryexpression)
        {
            return parenthesize(binaryexpression.Operator.Lexeme, binaryexpression.Left, binaryexpression.Right);
        }

        public string VisitGroupingExpression(GroupingExpression groupingexpression)
        {
            return parenthesize("group", groupingexpression.Expression);
        }

        public string VisitLiteralExpression(LiteralExpression literalexpression)
        {
            return literalexpression.Value?.ToString() ?? "nil";
        }

        public string VisitUnaryExpression(UnaryExpression unaryexpression)
        {
            return parenthesize(unaryexpression.Token.Lexeme, unaryexpression.Right);
        }

        public string Print(Expression expression)
        {
            return expression.Accept(this);
        }
    }
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestChapter5()
    {
        Expression expression = new BinaryExpression(
            new UnaryExpression(
                new Token(TokenType.MINUS, "-", null, 1),
                new LiteralExpression(123)),
            new Token(TokenType.STAR, "*", null, 1),
            new GroupingExpression(
                new LiteralExpression(45.67)));

        Console.WriteLine(new AstPrinter().Print(expression));
    }
    
    [Test]
    public void TestChapter6_1()
    {
        var tests = new List<string>()
        {
            "(1 + 2) / 3 * 4",
            "true == true and false == true",
            "1 != 2"
        };

        foreach (var test in tests)
        {
            var tokenizer =  new Tokenizer(test);
            var tokens = tokenizer.GetTokens();
            var parser = new Parser(tokens);
            var expression = parser.Parse();
            Console.WriteLine(new AstPrinter().Print(expression));    
        }
        
    }
}