using System;
using UnityEngine;

public sealed class Calculator
{
    private readonly Func<string, string> resolveVariable;
    private readonly Func<string, string> resolveMemory;

    public Calculator(Func<string, string> resolveVariable, Func<string, string> resolveMemory)
    {
        this.resolveVariable = resolveVariable;
        this.resolveMemory = resolveMemory;
    }

    public string Evaluate(string expr)
    {
        string cleanExpr = expr.Replace("(", "").Replace(")", "").Replace("\"", "")
                               .Replace('×', '*').Replace('÷', '/').Trim();

        if (cleanExpr.Contains("<"))
        {
            string[] p = cleanExpr.Split('<');
            if (TryEvalInt(p[0], out int left) && TryEvalInt(p[1], out int right))
                return left < right ? "TRUE" : "FALSE";
            return "FALSE";
        }

        foreach (char op in new[] { '+', '-', '*', '/' })
        {
            int idx = cleanExpr.IndexOf(op);
            if (idx <= 0) continue;

            if (!TryEvalInt(cleanExpr.Substring(0, idx), out int a) ||
                !TryEvalInt(cleanExpr.Substring(idx + 1), out int b))
                return cleanExpr;

            if (op == '/' && b == 0)
            {
                Debug.LogError("[Calculator] Divisió per zero.");
                return cleanExpr;
            }

            int res = op switch { '+' => a + b, '-' => a - b, '*' => a * b, '/' => a / b, _ => 0 };
            return res.ToString();
        }

        if (int.TryParse(cleanExpr, out _)) return cleanExpr;

        string varValue = resolveVariable(cleanExpr);
        if (!string.IsNullOrEmpty(varValue)) return varValue;

        string ramValue = resolveMemory(cleanExpr);
        if (!string.IsNullOrEmpty(ramValue)) return ramValue;

        return cleanExpr;
    }

    private bool TryEvalInt(string expr, out int result)
    {
        string evaluated = Evaluate(expr);
        if (int.TryParse(evaluated, out result)) return true;

        Debug.LogError($"[Calculator] No s'ha pogut avaluar '{expr.Trim()}' com a número.");
        result = 0;
        return false;
    }
}