using SharpLox.AbstractSyntaxTree;
using static SharpLox.TokenType;

namespace SharpLox;

public class Interpreter : IVisitor<object>
{
    public object Interpret(Expression expression)
    {
        try
        {
            Object value = Evaluate(expression);
            return value;
        }
        catch (RuntimeException error)
        {
            Diagnostics.RuntimeError(error);
            return null;
        }
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
    
    private void CheckNumberOperands(Token oper,
        Object left, Object right) {
        if (left is double  && right is double) return;
    
        throw new RuntimeException(oper, "Operands must be numbers.");
    }

    private void CheckNumberOperand(Token oper,
        Object obj) {
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
}