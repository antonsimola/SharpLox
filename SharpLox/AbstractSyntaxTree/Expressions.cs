namespace SharpLox.AbstractSyntaxTree;


public abstract record Expression {
public abstract T  Accept<T>(IVisitor<T> visitor);
}


public interface IVisitor<out T> {
    T VisitBinaryExpression(BinaryExpression binaryexpression);
    T VisitGroupingExpression(GroupingExpression groupingexpression);
    T VisitLiteralExpression(LiteralExpression literalexpression);
    T VisitUnaryExpression(UnaryExpression unaryexpression);
    }


public record BinaryExpression(Expression Left, Token Operator, Expression Right) : Expression {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitBinaryExpression(this);
    }
}


public record GroupingExpression(Expression Expression) : Expression {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitGroupingExpression(this);
    }
}


public record LiteralExpression(object Value) : Expression {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitLiteralExpression(this);
    }
}


public record UnaryExpression(Token Token, Expression Right) : Expression {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitUnaryExpression(this);
    }
}


