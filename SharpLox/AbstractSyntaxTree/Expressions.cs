namespace SharpLox.AbstractSyntaxTree;


public abstract record Expression {
public abstract T  Accept<T>(IVisitorExpression<T> visitor);
}


public interface IVisitorExpression<out T> {
    T VisitAssignExpression(AssignExpression assignexpression);
    T VisitBinaryExpression(BinaryExpression binaryexpression);
    T VisitCallExpression(CallExpression callexpression);
    T VisitGetExpression(GetExpression getexpression);
    T VisitGroupingExpression(GroupingExpression groupingexpression);
    T VisitLiteralExpression(LiteralExpression literalexpression);
    T VisitLogicalExpression(LogicalExpression logicalexpression);
    T VisitSetExpression(SetExpression setexpression);
    T VisitThisExpression(ThisExpression thisexpression);
    T VisitUnaryExpression(UnaryExpression unaryexpression);
    T VisitVariableExpression(VariableExpression variableexpression);
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


public record GetExpression(Expression Object, Token Name) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitGetExpression(this);
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


public record SetExpression(Expression Object, Token Name, Expression Value) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitSetExpression(this);
    }
}


public record ThisExpression(Token Keyword) : Expression {
    public override T Accept<T>(IVisitorExpression<T> visitor) {
        return visitor.VisitThisExpression(this);
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


