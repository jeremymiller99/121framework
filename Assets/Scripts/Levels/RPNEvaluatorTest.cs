using UnityEngine;
using System.Collections.Generic;

public class RPNEvaluatorTest : MonoBehaviour
{
    [ContextMenu("Test RPN Evaluator")]
    void TestRPN()
    {
        var variables = new Dictionary<string, int>
        {
            ["wave"] = 3,
            ["base"] = 20
        };

        // Test basic operations
        Debug.Log($"5 wave + = {RPNEvaluator.Evaluate("5 wave +", variables)}"); // Should be 8
        Debug.Log($"base 5 wave * + = {RPNEvaluator.Evaluate("base 5 wave * +", variables)}"); // Should be 35
        Debug.Log($"wave 3 / = {RPNEvaluator.Evaluate("wave 3 /", variables)}"); // Should be 1
        Debug.Log($"30 wave * = {RPNEvaluator.Evaluate("30 wave *", variables)}"); // Should be 90
        Debug.Log($"wave 5 / 1 wave 5 % - * = {RPNEvaluator.Evaluate("wave 5 / 1 wave 5 % - *", variables)}"); // Should be 0
        
        // Test with different wave values
        variables["wave"] = 7;
        Debug.Log($"Wave 7: wave 5 / 1 wave 5 % - * = {RPNEvaluator.Evaluate("wave 5 / 1 wave 5 % - *", variables)}"); // Should be 1
        
        variables["wave"] = 12;
        Debug.Log($"Wave 12: wave 5 / 1 wave 5 % - * = {RPNEvaluator.Evaluate("wave 5 / 1 wave 5 % - *", variables)}"); // Should be 2
    }
} 