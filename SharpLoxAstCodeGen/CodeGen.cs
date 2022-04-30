using System.Text;

namespace SharpLoxAstCodeGen;

public class CodeGen
{
    public static void Main(string[] args)
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
        DefineAst("../../../../SharpLox/AbstractSyntaxTree/Expressions.cs", "Expression", new List<string>()
        {
            "BinaryExpression   : Expression Left, Token Operator, Expression Right",
            "GroupingExpression : Expression Expression",
            "LiteralExpression  : object Value",
            "UnaryExpression    : Token Token, Expression Right"
        });
    }

    public static void DefineAst(string fileName, string baseType, IList<string> subclassDefs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace SharpLox.AbstractSyntaxTree;");
        sb.AppendLine("\n");
        sb.AppendLine($"public abstract record {baseType} {{");
        sb.AppendLine($"public abstract T  Accept<T>(IVisitor<T> visitor);");
        
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
        sb.AppendLine("public interface IVisitor<out T> {");

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
        sb.AppendLine($"    public override T Accept<T>(IVisitor<T> visitor) {{");
        
        sb.AppendLine($"        return visitor.Visit{typeName}(this);");
        sb.AppendLine($"    }}");
        sb.AppendLine($"}}");
    }
}