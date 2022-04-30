﻿namespace SharpLox;

public class Diagnostics
{
    public static bool HadError { get; set; }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }
    
    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF) {
            Report(token.Line, " at end", message);
        } else {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    
    public static  void Report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error{where}: {message}");

        HadError = true; 
    }
}