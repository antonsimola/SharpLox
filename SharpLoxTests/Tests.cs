using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpLox;

namespace SharpLoxTests;

public class Tests
{
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
            Assert.AreEqual(expect, RunCode(stmt), stmt);
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
    public void TestSimpleFor()
    {
        var test = @"
var i = 0;
for(; i < 10; i = i +1) print i;
";
        Assert.AreEqual(string.Join("\r\n", Enumerable.Range(0, 10)), RunCode(test));
    }
    
    [Test]
    public void TestSimpleWhile()
    {
        var test = @"
var i = 0;
while( i < 10) {
    print i;
    i = i + 1;
}
";
        Assert.AreEqual(string.Join("\r\n", Enumerable.Range(0, 10)), RunCode(test));
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


    [Test]
    public void TestSimpleBreakIf()
    {
        var test = @"
            var a = 0;

            while (true) {
              if(a > 3)  {
                break;
              }
            a = a +1;
            }

            print a;
            ";
        Assert.AreEqual("4", RunCode(test));
    }

    [Test]
    public void TestChapter10Functions()
    {
        var test = @"
            fun sayHi(first, last) {
                print ""Hi, "" + first + "" "" + last + ""!"";
            }
            
            sayHi(""Anton"", ""Simola"");
";
        Assert.AreEqual("Hi, Anton Simola!", RunCode(test));
    }

    [Test]
    public void TestChapter10Fibonacci()
    {
        var test = @"
            fun fib(n) {
              if (n <= 1) {return n;}
              return fib(n - 2) + fib(n - 1);
            }

            for (var i = 0; i < 20; i = i + 1) {
              print fib(i);
        }";
        Assert.AreEqual(@"0
1
1
2
3
5
8
13
21
34
55
89
144
233
377
610
987
1597
2584
4181", RunCode(test));
    }

    [Test]
    public void TestChapter10Closure()
    {
        var test = @"
fun makeCounter() {
  var i = 0;
  fun count() {
    i = i + 1;
    print i;
  }

  return count;
}

var counter = makeCounter();
counter();
counter();
";

        Assert.AreEqual("1\r\n2", RunCode(test));
    }
    
    [Test]
    public void TestChapter11Classes()
    {
        var test = @"
class Person {
    init(n) {
        this.name = n;
    }

    sayHello() {
        print ""Hello from "" + this.name;    
    }
}

var bob = Person(""Bob"");

bob.sayHello();
";

        Assert.AreEqual("Hello from Bob", RunCode(test));
    }


    public void Print(object any)
    {
        TestContext.Out.WriteLine(any);
    }

    private string RunCode(string code)
    {
        var engine = new LoxScriptEngine();
        return engine.Evaluate(code);
    }
}