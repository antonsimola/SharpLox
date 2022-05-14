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
    private readonly List<IDictionary<String, Boolean>> _scopes = new();

    public enum FunctionType
    {
        None,
        Function,
        Method,
        Initializer
    }
    private enum ClassType
    {
        None,
        Class
    }

    private FunctionType currentFunction = FunctionType.None;
    private ClassType currentClass = ClassType.None;

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

    public object VisitGetExpression(GetExpression getexpression)
    {
        Resolve(getexpression.Object);
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

    public object VisitSetExpression(SetExpression setexpression)
    {
        Resolve(setexpression.Value);
        Resolve(setexpression.Object);
        return null;
    }

    public object VisitThisExpression(ThisExpression thisexpression)
    {
        if (currentClass == ClassType.None) {
            Diagnostics.Error(thisexpression.Keyword, "Can't use 'this' outside of a class.");
            return null;
        }
        
        ResolveLocal(thisexpression, thisexpression.Keyword);
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
            _scopes[_scopes.Count-1].ContainsKey(expr.Name.Lexeme)
            &&
            _scopes[_scopes.Count-1][expr.Name.Lexeme] == false)
        {
            Diagnostics.Error(expr.Name, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    private void ResolveLocal(Expression expr, Token name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].ContainsKey(name.Lexeme))
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

    public object VisitClassStatement(ClassStatement classstatement)
    {
        ClassType enclosingClass = currentClass;
        currentClass = ClassType.Class;
        
        Define(classstatement.Name);
        BeginScope();
        _scopes[_scopes.Count-1]["this"] = true;
        foreach (FunctionStatement method in classstatement.Methods)
        {
            FunctionType declaration = FunctionType.Method;
            if (method.Name.Lexeme == "init")
            {
                declaration = FunctionType.Initializer;
            }
            ResolveFunction(method, declaration);
        }
        
        EndScope();

        currentClass = enclosingClass;
        return null;
    }

    private void BeginScope()
    {
        _scopes.Add(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        _scopes.RemoveAt(_scopes.Count-1);
    }

    public void Resolve(List<Statement> blockstatementStatements)
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

        ResolveFunction(functionstatement, FunctionType.Function);
        return null;
    }

    private void ResolveFunction(FunctionStatement functionstatement, FunctionType functionType)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = functionType;
        
        BeginScope();
        foreach (var param in functionstatement.Params)
        {
            Declare(param);
            Define(param);
        }

        Resolve(functionstatement.Body);
        EndScope();
        currentFunction = enclosingFunction;

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
        
        if (returnstatement.Expression != null)
        {
            if (currentFunction == FunctionType.Initializer) {
                Diagnostics.Error(returnstatement.Keyword, "Can't return a value from an initializer.");
            }
            Resolve(returnstatement.Expression);
        }
            

        return null;
    }

    public object VisitWhileStatement(WhileStatement whilestatement)
    {
        Resolve(whilestatement.Condition);
        Resolve(whilestatement.Body);
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

        var scope = _scopes[_scopes.Count -1];
        
        if (scope.ContainsKey(token.Lexeme)) {
            Diagnostics.Error(token, "Already a variable with this name in this scope.");
        }
        scope[token.Lexeme] = false;
    }

    private void Define(Token token)
    {
        if (_scopes.Count == 0)
        {
            return;
        }

        var scope = _scopes[_scopes.Count - 1];
        scope[token.Lexeme] = true;
    }
}

