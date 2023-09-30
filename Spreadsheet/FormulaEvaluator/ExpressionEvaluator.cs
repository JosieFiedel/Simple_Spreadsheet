///  Josie Fiedel
///  September 5, 2022
using System;
using System.Text.RegularExpressions;

namespace FormulaEvaluator;

/// <summary>
/// This class contains methods to evaluate a given arithmetic integer infix expression. For a given
/// variable (ex: A7), a delegate is utilized to retrieve the variable's integer value. By following
/// a particular algorithm to compute the expression, either a single value will be returned, or an
/// exception will be thrown if an error occurs.
/// </summary>
public static class Evaluator
{
    /// <summary>
    /// "Looks up" a string to determine its corresponding integer value. 
    /// </summary>
    /// <param name="v"> The string variable that corresponds to an integer value. </param>
    /// <returns> The integer corresponding to the variable. </returns>
    /// <exception cref="ArgumentException"> Thrown when the variable has no value. </exception>
    public delegate int Lookup(String v);

    /// <summary>
    /// Determines if the given string follows the definition of a variable: letter(s) followed by number(s).
    /// </summary>
    /// <param name="token"> The token string to be determined if it is a variable. </param>
    /// <returns> True if the string is a variable, false otherwise. </returns>   
    /// <exception cref="ArgumentException"> Thrown when the variable is invalid. </exception>
    private static bool IsVar(string token)
    {
        string varPattern = @"^[A-Za-z]+[0-9]+$";
        if (!Regex.IsMatch(token, varPattern))
            return false;
        return true;
    }

    /// <summary>
    /// Given two integer values and an operator, the respective computations are made
    /// on the variables and the result is returned.
    /// </summary>
    /// <param name="val1"> The first variable of the computation. </param>
    /// <param name="oper"> The operator to be used in the computation. </param>
    /// <param name="val2"> The second variable of the computation. </param>
    /// <returns> The result of the computation. </returns>
    private static int PerformComputation(int val1, string oper, int val2)
    {
        int result = 0;
        switch (oper)
        {
            case "+":
                result = val1 + val2;
                break;
            case "-":
                result = val2 - val1;
                break;
            case "*":
                result = val1 * val2;
                break;
            case "/":
                if (val1 == 0)
                    throw new ArgumentException("A division by zero is not legal.");
                result = val2 / val1;
                break;
        }
        return result;
    }

    /// <summary>
    /// Given an integer infix expression and a LookUp function, the expression is evaluated and its
    /// integer result is returned. If there are any variables in the expression, the LookUp function
    /// is utilized to determine the integer value of the corresponding variable. 
    /// </summary>
    /// <param name="exp"> The expression to be evaluated. </param>
    /// <param name="variableEvaluator"> The LookUp function to convert variables to integers. </param>
    /// <returns> The integer result of the expression. </returns>
    /// <exception cref="ArgumentException"> Thrown when the expression is invalid. </exception>
    public static int Evaluate(String exp, Lookup variableEvaluator)
    {
        Stack<int> valStack = new();      // Value token stack
        Stack<string> operStack = new();  // Operator token stack

        // Splits the expression's tokens into integers, variables, and operators.
        string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

        foreach (string token in substrings)
        {
            string newToken = token.Trim();   // Remove whitespace, if any.

            // Integer & variable action:
            if (int.TryParse(newToken, out int result) || IsVar(newToken))
            {
                if(IsVar(newToken))
                    result = variableEvaluator(newToken);

                if (operStack.OnTop("*", "/"))
                {
                    if (valStack.Count < 1)
                        throw new ArgumentException("Illegal Argument. (Cannot Pop an empty stack)");
                    valStack.Push(PerformComputation(result, operStack.Pop(), valStack.Pop()));
                }
                else
                    valStack.Push(result);
            }

            // Operator action:
            else if (newToken == "*" || newToken == "/" || newToken == "(")
                operStack.Push(newToken);

            else if (newToken == "+" || newToken == "-")
            {
                if (operStack.OnTop("+", "-"))
                {
                    if (valStack.Count < 2)
                        throw new ArgumentException("Illegal Argument. (Cannot Pop an empty stack)");
                    valStack.Push(PerformComputation(valStack.Pop(), operStack.Pop(), valStack.Pop()));
                }
                operStack.Push(newToken);
            }

            else if (newToken == ")")
            {
                if (operStack.OnTop("+", "-"))
                {
                    if (valStack.Count < 2)
                        throw new ArgumentException("Illegal Argument. (Cannot Pop an empty stack)");
                    valStack.Push(PerformComputation(valStack.Pop(), operStack.Pop(), valStack.Pop()));
                }

                if (!operStack.IsOnTop("("))
                    throw new ArgumentException("Invalid Expression.");
                operStack.Pop();

                if (operStack.OnTop("*", "/"))
                {
                    if (valStack.Count < 2)
                        throw new ArgumentException("Illegal Argument. (Cannot Pop an empty stack)");
                    valStack.Push(PerformComputation(valStack.Pop(), operStack.Pop(), valStack.Pop()));
                }
            }
            
            // All remaining strings are not valid, ignoring empty substrings. 
            else if (newToken != "")
                throw new ArgumentException("Invalid Expression.");
        }
        // If elements remain in the operator stack:
        if (operStack.Count == 1 && operStack.OnTop("+", "-") && valStack.Count == 2)
            return PerformComputation(valStack.Pop(), operStack.Pop(), valStack.Pop());

        // If no elements remain in the operator stack:
        else if (valStack.Count != 1 || (operStack.Count == 1 && valStack.Count == 1))
            throw new ArgumentException("Invalid Expression.");

        return valStack.Pop();  // The final result is in valStack.
    }
}

/// <summary>
/// This class holds all extension methods that act as 'syntactic sugar' for the Stack class.
/// </summary>
public static class PS1StackExtensions
{
    /// <summary>
    /// For a given stack, checks that the stack is not empty and that the top element of the stack is
    /// the given value parameter. 
    /// </summary>
    /// <param name="s"> The stack to be checked </param>
    /// <param name="val"> The value to be checked if it's on the top of the stack </param>
    /// <returns> True if the stack isn't empty and the value is on top, false otherwise. </returns>
    public static bool IsOnTop(this Stack<string> s, string val)
    {
        return s.Count > 0 && s.Peek() == val;
    }

    /// <summary>
    /// For a given stack, checks that the stack is not empty and that the top element of the stack is
    /// one of the given operator parameters.
    /// </summary>
    /// <param name="s"> The stack to be checked </param>
    /// <param name="oper1"> The first potential operator to be on top </param>
    /// <param name="oper2"> The second potential operator to be on top </param>
    /// <returns> True if the stack isn't empty and either operator is on top, false otherwise. </returns>
    public static bool OnTop(this Stack<string> s, string oper1, string oper2)
    {
        return s.Count > 0 && (s.Peek() == oper1 || s.Peek() == oper2);
    }
}