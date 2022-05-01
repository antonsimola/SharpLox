namespace SharpLox.AbstractSyntaxTree;


public abstract record Statement {
public abstract T  Accept<T>(IVisitorStatement<T> visitor);
}


public interface IVisitorStatement<out T> {
    T VisitBlockStatement(BlockStatement blockstatement);
    T VisitExpressionStatement(ExpressionStatement expressionstatement);
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


