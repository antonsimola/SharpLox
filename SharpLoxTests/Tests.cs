using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SharpLox;
using SharpLox.AbstractSyntaxTree;
using SharpLox.Parser;

namespace SharpLoxTests;

public class Tests
{
    // public class AstPrinter : IVisitorExpression<string>, IVisitorStatement<string>
    // {
    //     private String parenthesize(String name, params Expression[] exprs)
    //     {
    //         StringBuilder builder = new StringBuilder();
    //
    //         builder.Append("(").Append(name);
    //         foreach (Expression expr in exprs)
    //         {
    //             builder.Append(" ");
    //             builder.Append(expr.Accept(this));
    //         }
    //
    //         builder.Append(")");
    //
    //         return builder.ToString();
    //     }
    //
    //     public string VisitBinaryExpression(BinaryExpression binaryexpression)
    //     {
    //         return parenthesize(binaryexpression.Operator.Lexeme, binaryexpression.Left, binaryexpression.Right);
    //     }
    //
    //     public string VisitGroupingExpression(GroupingExpression groupingexpression)
    //     {
    //         return parenthesize("group", groupingexpression.Expression);
    //     }
    //
    //     public string VisitLiteralExpression(LiteralExpression literalexpression)
    //     {
    //         return literalexpression.Value?.ToString() ?? "nil";
    //     }
    //
    //     public string VisitUnaryExpression(UnaryExpression unaryexpression)
    //     {
    //         return parenthesize(unaryexpression.Token.Lexeme, unaryexpression.Right);
    //     }
    //
    //     public string VisitVariableExpression(VariableExpression variableexpression)
    //     {
    //         throw new NotImplementedException();
    //     }
    //
    //     public string Print(Expression expression)
    //     {
    //         return expression.Accept(this);
    //     }
    //
    //     public string VisitExpressionStatement(ExpressionStatement expressionstatement)
    //     {
    //         throw new NotImplementedException();
    //     }
    //
    //     public string VisitPrintStatement(PrintStatement printstatement)
    //     {
    //         throw new NotImplementedException();
    //     }
    //
    //     public string VisitVariableStatement(VariableStatement variablestatement)
    //     {
    //         throw new NotImplementedException();
    //     }
    // }

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

        // Console.WriteLine(new AstPrinter().Print(expression));
    }

    [Test]
    public void TestChapter6_1()
    {
        var tests = new List<string>()
        {
            "(1 + 2) / 3 * 4",
            "true == true",
            "1 != 2"
        };

        foreach (var test in tests)
        {
            var tokenizer = new Tokenizer(test);
            var tokens = tokenizer.GetTokens();
            var parser = new Parser(tokens);
            var expression = parser.Parse();
            //Console.WriteLine(new AstPrinter().Print(expression));
        }
    }

    // does not work anymore after chapter 8 since interpreter now accepts statements.

    // [Test]
    // public void TestChapter7()
    // {
    //     var tests = new Dictionary<string, object>()
    //     {
    //         { "(1 + 2) / 3 * 4", 4.0 },
    //         { "true == true", true },
    //         { "false == true ", false },
    //         { "false == false ", true },
    //         { "true == false", false },
    //         { "1 != 2", true },
    //         { "1 == 2", false },
    //         { "1 == 1", true },
    //         { "\"Hello\" == \"Hello\"", true },
    //         { "\"Hello\" != \"World\"", true },
    //         { "\"Hello\" + \"World\"", "HelloWorld" }
    //     };
    //
    //     foreach (var (expr, expect) in tests)
    //     {
    //         var tokenizer = new Tokenizer(expr);
    //         var tokens = tokenizer.GetTokens();
    //         var parser = new Parser(tokens);
    //         var expression = parser.Parse();
    //         var interpreter = new Interpreter();
    //         var result = interpreter.Interpret(expression);
    //         Assert.AreEqual(expect, result, message: expr);
    //     }
    // }

    [Test]
    public void TestChapter8Statements()
    {
        var tests = new Dictionary<string, string>()
        {
            { "print (1 + 2) / 3 * 4;", "4" },
            { "print true == true;", "True" },
            { "print false == true;", "False" },
            { "print false == false; ", "True" },
            { "print true == false;", "False" },
            { "print 1 != 2;", "True" },
            { "print 1 == 2;", "False" },
            { "print 1 == 1;", "True" },
            { "print \"Hello\" == \"Hello\";", "True" },
            { "print \"Hello\" != \"World\";", "True" },
            { "print \"Hello\" + \"World\";", "HelloWorld" }
        };

        foreach (var (stmt, expect) in tests)
        {
            Assert.AreEqual(expect,RunCode(stmt), stmt);
        }
    }

    [Test]
    public void TestChapter8Variables()
    {
        var tests = new Dictionary<string, string>()
        {
            { "var a = 1;var b =3; print a + b;", "4" },
            { "var a = \"Hello\";var b = \"World\"; print a + b;", "HelloWorld" },
        };

        foreach (var (stmt, expect) in tests)
        {
            Assert.AreEqual(expect, RunCode(stmt), stmt);
        }
    }

    [Test]
    public void TestChapter8Scoping()
    {
        var test = @"
        var a = ""global a"";
        var b = ""global b"";
        var c = ""global c"";
        {
            var a = ""outer a"";
            var b = ""outer b"";
            {
                var a = ""inner a"";
                print a;
                print b;
                print c;
            }
            print a;
            print b;
            print c;
        }
        print a;
        print b;
        print c;
";
        Print(RunCode(test));
        

    }
    [Test]
    public void TestChapter9IfElse()
    {
        var test = @"
            var a = 1;
            if(a > 1) {print ""large"";} else {print ""small"";}
            if(a == 1) {print ""1"";}
";

        var results = RunCode(test);
        Assert.AreEqual("small\r\n1", results, test);
        
    }


    [Test]
    public void SimpleFibonacci()
    {
        var test = @"
            var a = 0;
            var temp ;

            for (var b = 1; a < 10000; b = temp + b) {
              print a;
              temp = a;
              a = b;
            }";
        Print(RunCode(test));
        
    }
    
    [Test]
    public void TestSimpleBreak()
    {
        var test = @"
            var a = 0;
            var temp ;

            while (a < 3) {
              break;
              print 2;
              a = a + 1;
            }
            print 1;
            ";
        Assert.AreEqual("1", RunCode(test));
        
    }

    public void Print(object any)
    {
        TestContext.Out.WriteLine(any);
    }

    private string RunCode(string code)
    {
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var tokenizer = new Tokenizer(code );
        var tokens = tokenizer.GetTokens();
        var parser = new Parser(tokens);
        var statements = parser.Parse();
        var interpreter = new Interpreter();
        interpreter.Interpret(statements);
        var result = stringWriter.ToString().Trim();
        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
        standardOutput.AutoFlush = true;
        Console.SetOut(standardOutput);
        return result; 
    }
}