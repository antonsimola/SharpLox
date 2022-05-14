using System.Text;

namespace SharpLoxAstCodeGen;

public class CodeGen
{
    public static void Main(string[] args)
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
        DefineAst("../../../../SharpLox/AbstractSyntaxTree/Expressions.cs", "Expression", new List<string>()
        {
            "AssignExpression   : Token Name, Expression Value",
            "BinaryExpression   : Expression Left, Token Operator, Expression Right",
            "CallExpression     : Expression Callee, Token Paren, List<Expression> Arguments",
            "GetExpression      : Expression Object, Token Name",
            "GroupingExpression : Expression Expression",
            "LiteralExpression  : object Value",
            "LogicalExpression  : Expression Left, Token Operator, Expression Right",
            "SetExpression      : Expression Object, Token Name, Expression Value",
            "ThisExpression     : Token Keyword",
            "UnaryExpression    : Token Token, Expression Right",
            "VariableExpression : Token Name"
        });
        
        DefineAst("../../../../SharpLox/AbstractSyntaxTree/Statement.cs", "Statement", new List<string>()
        {
            "BlockStatement         : List<Statement> Statements",
            "ClassStatement         : Token Name,  List<FunctionStatement> Methods",
            "ExpressionStatement    : Expression Expression",
            "FunctionStatement      : Token Name, List<Token> Params, List<Statement> Body",
            "IfStatement            : Expression Condition, Statement ThenBranch, Statement ElseBranch",
            "ReturnStatement        : Token Keyword, Expression Expression",
            "WhileStatement         : Expression Condition, Statement Body",
            "BreakStatement         : ",
            "PrintStatement         : Expression Expression",
            "VariableStatement      : Token Name, Expression Initializer"
        });
    }

    public static void DefineAst(string fileName, string baseType, IList<string> subclassDefs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace SharpLox.AbstractSyntaxTree;");
        sb.AppendLine("\n");
        sb.AppendLine($"public abstract record {baseType} {{");
        sb.AppendLine($"public abstract T  Accept<T>(IVisitor{baseType}<T> visitor);");
        
        sb.AppendLine($"}}");
        sb.AppendLine("\n");
        DefineVisitor(sb, baseType, subclassDefs);
        
        sb.AppendLine("\n");
        foreach (var subclassDef in subclassDefs)
        {
            string typeName = subclassDef.Split(":")[0].Trim();
            string properties = subclassDef.Split(":")[1].Trim();
            DefineType(sb, baseType, typeName, properties);
            sb.AppendLine("\n");
        }

        File.WriteAllText(fileName, sb.ToString());
    }

    private static void DefineVisitor(StringBuilder sb, string baseType, IList<string> types)
    {
        sb.AppendLine($"public interface IVisitor{baseType}<out T> {{");

        foreach (string type in types)
        {
            string typeName = type.Split(":")[0].Trim();
            sb.AppendLine($"    T Visit{typeName}({typeName} {typeName.ToLower()});");
        }

        sb.AppendLine("    }");
    }


    private static void DefineType(StringBuilder sb, string baseType, string typeName, string properties)
    {
        sb.AppendLine($"public record {typeName}({properties}) : {baseType} {{");
        sb.AppendLine($"    public override T Accept<T>(IVisitor{baseType}<T> visitor) {{");
        
        sb.AppendLine($"        return visitor.Visit{typeName}(this);");
        sb.AppendLine($"    }}");
        sb.AppendLine($"}}");
    }
}