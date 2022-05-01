using System.Globalization;
using SharpLox.AbstractSyntaxTree;
using static SharpLox.TokenType;

namespace SharpLox;

public class Interpreter : IVisitorExpression<object>, IVisitorStatement<object>
{
    public  Environment Environment { get; set; } = new();
    
    public void Interpret(List<Statement> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeException error)
        {
            Diagnostics.RuntimeError(error);
        }
    }

    private void Execute(Statement statement)
    {
        statement.Accept(this);
    }

    public object VisitAssignExpression(AssignExpression assignexpression)
    {
        var val = Evaluate(assignexpression.Value);
        Environment.Assign(assignexpression.Name, val);
        return val;
    }

    public object VisitBinaryExpression(BinaryExpression binaryexpression)
    {
        Object left = Evaluate(binaryexpression.Left);
        Object right = Evaluate(binaryexpression.Right);
        switch (binaryexpression.Operator.Type)
        {
            case BANG_EQUAL: return !IsEqual(left, right);
            case EQUAL_EQUAL: return IsEqual(left, right);
            case GREATER:
                return (double)left > (double)right;
            case GREATER_EQUAL:
                CheckNumberOperands(binaryexpression.Operator, left, right);
                return (double)left >= (double)right;
            case LESS:
                CheckNumberOperands(binaryexpression.Operator, left, right);
                return (double)left < (double)right;
            case LESS_EQUAL:
                CheckNumberOperands(binaryexpression.Operator, left, right);
                return (double)left <= (double)right;
            case MINUS:
                CheckNumberOperands(binaryexpression.Operator, left, right);
                return (double)left - (double)right;
            case SLASH:
                CheckNumberOperands(binaryexpression.Operator, left, right);
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperands(binaryexpression.Operator, left, right);
                return (double)left * (double)right;
            case PLUS:
                if (left is double a && right is double b)
                {
                    return a + b;
                }

                if (left is string s1 && right is string s2)
                {
                    return (string)s1 + s2;
                }

                break;
        }

        return null;
    }

    private bool IsEqual(object? a, object? b)
    {
        return object.Equals(a, b);
    }

    public object VisitGroupingExpression(GroupingExpression groupingexpression)
    {
        return Evaluate(groupingexpression.Expression);
    }

    public object VisitLiteralExpression(LiteralExpression literalexpression)
    {
        return literalexpression.Value;
    }

    public object VisitUnaryExpression(UnaryExpression unaryexpression)
    {
        Object right = Evaluate(unaryexpression.Right);

        switch (unaryexpression.Token.Type)
        {
            case BANG:
                return !IsTruthy(right);
            case MINUS:
                CheckNumberOperand(unaryexpression.Token, right);
                return -(double)right;
        }

        return null;
    }

    public object VisitVariableExpression(VariableExpression variableexpression)
    {
        return Environment.Get(variableexpression.Name.Lexeme);
    }

    private void CheckNumberOperands(Token oper,
        Object left, Object right)
    {
        if (left is double && right is double) return;

        throw new RuntimeException(oper, "Operands must be numbers.");
    }

    private void CheckNumberOperand(Token oper,
        Object obj)
    {
        if (obj is double) return;

        throw new RuntimeException(oper, "Operand must be a number.");
    }

    private bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool) return (bool)obj;
        return true;
    }

    private Object Evaluate(Expression expr)
    {
        return expr.Accept(this);
    }

    public object VisitBlockStatement(BlockStatement blockstatement)
    {
        ExecuteBlock(blockstatement.Statements, new Environment(Environment));
        return null;
    }

    private void ExecuteBlock(List<Statement> statements, Environment environment)
    {
        Environment previous = this.Environment;
        try {
            this.Environment = environment;

            foreach (var  statement in statements) {
                Execute(statement);
            }
        } finally {
            this.Environment = previous;
        }
    }


    public object VisitExpressionStatement(ExpressionStatement expressionstatement)
    {
        Evaluate(expressionstatement.Expression);
        return null;
    }

    public object VisitPrintStatement(PrintStatement printstatement)
    {
        var obj = Evaluate(printstatement.Expression);
        Console.WriteLine(Stringify(obj));
        return null;
    }

    public object VisitVariableStatement(VariableStatement variablestatement)
    {
        object value = null;
        if (variablestatement.Initializer != null)
        {
            value = Evaluate(variablestatement.Initializer);
        }

        Environment.Define(variablestatement.Name.Lexeme, value);
        return null;
    }

    private string Stringify(object obj)
    {
        {
            if (obj == null) return "nil";

            if (obj is double d)
            {
                String text = d.ToString(CultureInfo.InvariantCulture);
                if (text.EndsWith(".0"))
                {
                    text = text[0 .. (text.Length - 2)];
                }

                return text;
            }

            return obj.ToString();
        }
    }
}