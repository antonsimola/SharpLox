namespace SharpLox.AbstractSyntaxTree;


public abstract record Expression {
public abstract T  Accept<T>(IVisitorExpression<T> visitor);
}


public interface IVisitorExpression<out T> {
    T VisitAssignExpression(AssignExpression assignexpression);
    T VisitBinaryExpression(BinaryExpression binaryexpression);
    T VisitCallExpression(CallExpression callexpression);
    T VisitGroupingExpression(GroupingExpression groupingexpression);
    T VisitLiteralExpression(LiteralExpression literalexpression);
    T VisitLogicalExpression(LogicalExpression logicalexpression);
    T VisitUnaryExpression(UnaryExpression unaryexpression);
    T VisitVariableExpression(VariableExpression expr);
    }


public record AssignExpression(Token Name, Expression Value) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitAssignExpression(this);
    }
}


public record BinaryExpression(Expression Left, Token Operator, Expression Right) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitBinaryExpression(this);
    }
}


public record CallExpression(Expression Callee, Token Paren, List<Expression> Arguments) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitCallExpression(this);
    }
}


public record GroupingExpression(Expression Expression) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitGroupingExpression(this);
    }
}


public record LiteralExpression(object Value) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitLiteralExpression(this);
    }
}


public record LogicalExpression(Expression Left, Token Operator, Expression Right) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitLogicalExpression(this);
    }
}


public record UnaryExpression(Token Token, Expression Right) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitUnaryExpression(this);
    }
}


public record VariableExpression(Token Name) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitVariableExpression(this);
    }
}


