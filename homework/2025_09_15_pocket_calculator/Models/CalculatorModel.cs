using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace _2025_09_15_pocket_calculator.Models;

public static partial class CalculatorModel
{
    [GeneratedRegex(@"([*\/]|\b\s*-|\b\s*\+)")]
    private static partial Regex SplitRegex();
    [GeneratedRegex(@"^-?\d+(\.\d+)?$")]
    private static partial Regex NumbersRegex();
    
    [GeneratedRegex(@"^[+\-*/]$")]
    private static partial Regex OperatorsRegex();

    public static int CalculateFromString(string expression)
    {
        var tokens = SplitExpression(expression);
        var numbers = ParseNumbers(tokens);
        var operators = ParseOperators(tokens);
        return CalculateResult(numbers, operators);
    }

    private static string[] SplitExpression(string expression)
    {
        return SplitRegex().Split(expression);
    }

    private static int[] ParseNumbers(string[] tokens)
    {
        return tokens
            .Where(s => NumbersRegex().IsMatch(s)).Select(int.Parse).ToArray();
    }

    private static string[] ParseOperators(string[] tokens)
    {
        return tokens.Where(s => OperatorsRegex().IsMatch(s)).ToArray();
    }

    private static int CalculateResult(int[] numbers, string[] operators)
    {
        if (numbers.Length == 0 || numbers.Length <= operators.Length)
        {
            throw new Exception("Invalid expression");
        }
        var currentResult = numbers.First();
        for (var i = 0; i < operators.Length; i++)
        {
            currentResult = operators[i] switch
            {
                "+" => currentResult + numbers[i + 1],
                "-" => currentResult - numbers[i + 1],
                "*" => currentResult * numbers[i + 1],
                "/" => numbers[i + 1] == 0
                    ? throw new DivideByZeroException("It is not allowed to divide by Zero!")
                    : currentResult / numbers[i + 1],
                _ => throw new Exception("Unknown operator")
            };
        }
        return currentResult;
    }
}