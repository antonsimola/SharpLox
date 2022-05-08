using System.Data;
using System.Linq.Expressions;
using System.Xml.Schema;
using SharpLox.AbstractSyntaxTree;
using BinaryExpression = SharpLox.AbstractSyntaxTree.BinaryExpression;
using Expression = SharpLox.AbstractSyntaxTree.Expression;
using UnaryExpression = SharpLox.AbstractSyntaxTree.UnaryExpression;

namespace SharpLox;

public class Resolver : IVisitorExpression<object>, IVisitorStatement<object>
{
    private readonly Interpreter _interpreter;
    private readonly Stack<IDictionary<String, Boolean>> _scopes = new();


    public Resolver(Interpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public object VisitAssignExpression(AssignExpression expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object VisitBinaryExpression(BinaryExpression binaryexpression)
    {
        Resolve(binaryexpression.Left);
        Resolve(binaryexpression.Right);
        return null;
    }

    public object VisitCallExpression(CallExpression callexpression)
    {
        Resolve(callexpression.Callee);
        foreach (var arg in callexpression.Arguments)
        {
            Resolve(arg);
        }

        return null;
    }

    public object VisitGroupingExpression(GroupingExpression groupingexpression)
    {
        Resolve(groupingexpression.Expression);
        return null;
    }

    public object VisitLiteralExpression(LiteralExpression literalexpression)
    {
        return null;
    }

    public object VisitLogicalExpression(LogicalExpression logicalexpression)
    {
        Resolve(logicalexpression.Left);
        Resolve(logicalexpression.Right);
        return null;
    }

    public object VisitUnaryExpression(UnaryExpression unaryexpression)
    {
        Resolve(unaryexpression.Right);
        return null;
    }

    public object VisitVariableExpression(VariableExpression expr)
    {
        if (_scopes.Count > 0 &&
             _scopes.Peek().ContainsKey(expr.Name.Lexeme) 
            && 
            _scopes.Peek()[expr.Name.Lexeme] == false)
        {
            Diagnostics.Error(expr.Name,   "Can't read local variable in its own initializer.");
        }
        
        ResolveLocal(expr, expr.Name);
        return null;
    }

    private void ResolveLocal(Expression expr, Token name)
    {

        var scopeArray = _scopes.ToArray();
        for (int i =scopeArray.Length - 1; i >= 0; i--) {
            if (scopeArray[i].TryGetValue(name.Lexeme, out var x))
            {
                _interpreter.Resolve(expr, _scopes.Count - 1 - i);
                return;
            }
        }
    }

    public object VisitBlockStatement(BlockStatement blockstatement)
    {
        BeginScope();
        Resolve(blockstatement.Statements);
        EndScope();
        return null;
    }

    private void BeginScope()
    {
        _scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        _scopes.Pop();
    }

    public  void Resolve(List<Statement> blockstatementStatements)
    {
        foreach (Statement statement in blockstatementStatements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Statement statement)
    {
        statement.Accept(this);
    }

    private void Resolve(Expression expression)
    {
        expression.Accept(this);
    }


    public object VisitExpressionStatement(ExpressionStatement expressionstatement)
    {
        Resolve(expressionstatement.Expression);
        return null;
    }

    public object VisitFunctionStatement(FunctionStatement functionstatement)
    {
        Declare(functionstatement.Name);
        Define(functionstatement.Name);

        ResolveFunction(functionstatement);
        return null;
    }

    private void ResolveFunction(FunctionStatement functionstatement)
    {
        BeginScope();
        
        foreach (var param in functionstatement.Params)
        {
            Declare(param);
            Define(param);
        }
        Resolve(functionstatement.Body);
        EndScope();
    }

    public object VisitIfStatement(IfStatement ifstatement)
    {
        Resolve(ifstatement.Condition);
        Resolve(ifstatement.ThenBranch);
        
        if (ifstatement.ElseBranch != null) Resolve(ifstatement.ElseBranch);
        return null;
    }

    public object VisitReturnStatement(ReturnStatement returnstatement)
    {
        if(returnstatement.Expression != null)
            Resolve(returnstatement.Expression);

        return null;
    }

    public object VisitWhileStatement(WhileStatement whilestatement)
    {
        Resolve(whilestatement.Body);
        Resolve(whilestatement.Condition);
        return null;
    }

    public object VisitBreakStatement(BreakStatement breakstatement)
    {
        // =???? EndScope();
        return null; //TODO
    }

    public object VisitPrintStatement(PrintStatement printstatement)
    {
            Resolve(printstatement.Expression);
            return null;
    }

    public object VisitVariableStatement(VariableStatement variablestatement)
    {
        Declare(variablestatement.Name);
        if (variablestatement.Initializer != null)
        {
            Resolve(variablestatement.Initializer);
        }

        Define(variablestatement.Name);
        return null;
    }

    private void Declare(Token token)
    {
        if (_scopes.Count == 0)
        {
            return;
        }

        var scope = _scopes.Peek();
        scope[token.Lexeme] = false;
    }

    private void Define(Token token)
    {
        if (_scopes.Count == 0)
        {
            return;
        }

        var scope = _scopes.Peek();
        scope[token.Lexeme] = true;
    }
}