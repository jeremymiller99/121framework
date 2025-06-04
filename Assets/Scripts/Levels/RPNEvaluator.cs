using System.Collections.Generic;
using System.Globalization;

public class RPNEvaluator
{
    public static int Evaluate(string expression, Dictionary<string, int> variables = null)
    {
        if (string.IsNullOrEmpty(expression))
            return 0;

        var tokens = expression.Split(' ');
        var stack = new Stack<int>();

        foreach (var token in tokens)
        {
            if (string.IsNullOrEmpty(token))
                continue;

            // Check if it's a number
            if (int.TryParse(token, out int number))
            {
                stack.Push(number);
            }
            // Check if it's a variable
            else if (variables != null && variables.ContainsKey(token))
            {
                stack.Push(variables[token]);
            }
            // Check if it's an operator
            else if (IsOperator(token))
            {
                if (stack.Count < 2)
                    throw new System.ArgumentException($"Not enough operands for operator '{token}'");

                int b = stack.Pop(); // Second operand
                int a = stack.Pop(); // First operand

                int result = ApplyOperator(token, a, b);
                stack.Push(result);
            }
            else
            {
                throw new System.ArgumentException($"Unknown token: '{token}'");
            }
        }

        if (stack.Count != 1)
            throw new System.ArgumentException("Invalid RPN expression");

        return stack.Pop();
    }

    private static bool IsOperator(string token)
    {
        return token == "+" || token == "-" || token == "*" || token == "/" || token == "%";
    }

    private static int ApplyOperator(string op, int a, int b)
    {
        switch (op)
        {
            case "+": return a + b;
            case "-": return a - b;
            case "*": return a * b;
            case "/": 
                if (b == 0) throw new System.DivideByZeroException("Division by zero");
                return a / b;
            case "%": 
                if (b == 0) throw new System.DivideByZeroException("Modulo by zero");
                return a % b;
            default:
                throw new System.ArgumentException($"Unknown operator: '{op}'");
        }
    }
} 