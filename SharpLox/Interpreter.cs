using System.Globalization;
using SharpLox.AbstractSyntaxTree;
using static SharpLox.TokenType;

namespace SharpLox;

public class Interpreter : IVisitorExpression<object>, IVisitorStatement<object>
{
    public readonly Environment Globals = new();
    private  readonly  IDictionary<Expression, int> _locals = new Dictionary<Expression, int>();
    public Environment Environment { get; set; }

    public Interpreter()
    {
        Environment = Globals;
    }

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
        if (_locals.TryGetValue(assignexpression, out var res))
        {
            Environment.AssignAt(res, assignexpression.Name, val);
        }
        else
        {
            Globals.Assign(assignexpression.Name, val);
        }
        
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

    public object VisitCallExpression(CallExpression callexpression)
    {
        Object callee = Evaluate(callexpression.Callee);
        List<Object> arguments = new List<object>();

        foreach (Expression argument in callexpression.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (callee is not ILoxCallable)
        {
            throw new RuntimeException(callexpression.Paren,
                "Can only call functions and classes.");
        }

        ILoxCallable function = (ILoxCallable)callee;

        if (function.Arity != arguments.Count)
        {
            throw new RuntimeException(callexpression.Paren,
                $"Expected {function.Arity} arguments but got {arguments.Count}.");
        }

        return function.Call(this, arguments);
    }

    private bool IsEqual(object? a, object? b)
    {
        return Equals(a, b);
    }

    public object VisitGroupingExpression(GroupingExpression groupingexpression)
    {
        return Evaluate(groupingexpression.Expression);
    }

    public object VisitLiteralExpression(LiteralExpression literalexpression)
    {
        return literalexpression.Value;
    }

    public object VisitLogicalExpression(LogicalExpression logicalexpression)
    {
        var left = Evaluate(logicalexpression.Left);
        if (logicalexpression.Operator.Type == OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(logicalexpression.Right);
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

    public object VisitVariableExpression(VariableExpression expr)
    {
        return LookupVariable(expr.Name, expr);
    }

    private object LookupVariable(Token exprName, Expression expr)
    {
        if (_locals.TryGetValue(expr, out var res))
        {
            return Environment.GetAt(res, exprName.Lexeme);
        }

        return Globals.Get(exprName.Lexeme);
        
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

    public void ExecuteBlock(List<Statement> statements, Environment environment)
    {
        Environment previous = this.Environment;
        try
        {
            this.Environment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            this.Environment = previous;
        }
    }


    public object VisitExpressionStatement(ExpressionStatement expressionstatement)
    {
        Evaluate(expressionstatement.Expression);
        return null;
    }

    public object VisitFunctionStatement(FunctionStatement functionstatement)
    {
        LoxFunction function = new LoxFunction(functionstatement, Environment);
        Environment.Define(functionstatement.Name.Lexeme, function);
        return null;
    }

    public object VisitIfStatement(IfStatement ifstatement)
    {
        if (IsTruthy(Evaluate(ifstatement.Condition)))
        {
            Execute(ifstatement.ThenBranch);
        }
        else if (ifstatement.ElseBranch != null)
        {
            Execute(ifstatement.ElseBranch);
        }

        return null;
    }

    public object VisitReturnStatement(ReturnStatement returnstatement)
    {
        object value = null;
        if (returnstatement.Expression != null) value = Evaluate(returnstatement.Expression);

        throw new ReturnException(value);
    }

    public object VisitWhileStatement(WhileStatement whilestatement)
    {
        while (IsTruthy(Evaluate(whilestatement.Condition)))
        {
            try
            {
                Execute(whilestatement.Body);
            }
            catch (BreakException e)
            {
                // ok, break from the loop
                break;
            }
        }

        return null;
    }

    public object VisitBreakStatement(BreakStatement breakstatement)
    {
        throw new BreakException();
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

    public void Resolve(Expression expr, int depth)
    {
        _locals[expr] = depth;
    }
}