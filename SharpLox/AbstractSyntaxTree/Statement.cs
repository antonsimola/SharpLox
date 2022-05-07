namespace SharpLox.AbstractSyntaxTree;


public abstract record Statement {
public abstract T  Accept<T>(IVisitorStatement<T> visitor);
}


public interface IVisitorStatement<out T> {
    T VisitBlockStatement(BlockStatement blockstatement);
    T VisitExpressionStatement(ExpressionStatement expressionstatement);
    T VisitFunctionStatement(FunctionStatement functionstatement);
    T VisitIfStatement(IfStatement ifstatement);
    T VisitReturnStatement(ReturnStatement returnstatement);
    T VisitWhileStatement(WhileStatement whilestatement);
    T VisitBreakStatement(BreakStatement breakstatement);
    T VisitPrintStatement(PrintStatement printstatement);
    T VisitVariableStatement(VariableStatement variablestatement);
    }


public record BlockStatement(List<Statement> Statements) : Statement {
    public override T Accept<T>(IVisitorStatement<T> visitor) {
        return visitor.VisitBlockStatement(this);
    }
}


public record ExpressionStatement(Expression Expression) : Statement {
    public override T Accept<T>(IVisitorStatement<T> visitor) {
        return visitor.VisitExpressionStatement(this);
    }
}


public record FunctionStatement(Token Name, List<Token> Params, List<Statement> Body) : Statement {
    public override T Accept<T>(IVisitorStatement<T> visitor) {
        return visitor.VisitFunctionStatement(this);
    }
}


public record IfStatement(Expression Condition, Statement ThenBranch, Statement ElseBranch) : Statement {
    public override T Accept<T>(IVisitorStatement<T> visitor) {
        return visitor.VisitIfStatement(this);
    }
}


public record ReturnStatement(Token Keyword, Expression Expression) : Statement {
    public override T Accept<T>(IVisitorStatement<T> visitor) {
        return visitor.VisitReturnStatement(this);
    }
}


public record WhileStatement(Expression Condition, Statement Body) : Statement {
    public override T Accept<T>(IVisitorStatement<T> visitor) {
        return visitor.VisitWhileStatement(this);
    }
}


public record BreakStatement() : Statement {
    public override T Accept<T>(IVisitorStatement<T> visitor) {
        return visitor.VisitBreakStatement(this);
    }
}


public record PrintStatement(Expression Expression) : Statement {
    public override T Accept<T>(IVisitorStatement<T> visitor) {
        return visitor.VisitPrintStatement(this);
    }
}


public record VariableStatement(Token Name, Expression Initializer) : Statement {
    public override T Accept<T>(IVisitorStatement<T> visitor) {
        return visitor.VisitVariableStatement(this);
    }
}


