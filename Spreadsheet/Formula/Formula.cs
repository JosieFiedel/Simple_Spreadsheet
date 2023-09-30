// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!

// Change log:
// Last updated: 9/8, updated for non-nullable types
// Implementation done by Josie Fiedel, September 16, 2022.

using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision 
/// floating-point syntax (without unary preceeding '-' or '+'); 
/// variables that consist of a letter or underscore followed by 
/// zero or more letters, underscores, or digits; parentheses; and the four operator 
/// symbols +, -, *, and /.  
/// 
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and "y"; "x23" is a single variable; 
/// and "x 23" consists of a variable "x" and a number "23".
/// 
/// Associated with every formula are two delegates:  a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form, and the validator is used
/// to add extra restrictions on the validity of a variable (beyond the standard requirement 
/// that it consist of a letter or underscore followed by zero or more letters, underscores,
/// or digits.)  Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    private readonly string formula;

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    /// 
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.  
    /// </summary>
    public Formula(String formula) :
        this(formula, s => s, s => true)
    {
    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    /// 
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.  
    /// 
    /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
    /// throws a FormulaFormatException with an explanatory message. 
    /// 
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    /// 
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    /// 
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        // Before the tokens are normalized, the initial formula syntax is checked.
        IsValidExp(formula);
        StringBuilder newFormula = new();   // Will hold the normalized formula.

        // Iterate through the tokens in the expression, normalizing and checking the validity of each one.
        // Append each token (including the new normalized variables) to create a new formula string.
        IEnumerable<string> tokens = GetTokens(formula);
        foreach (string token in tokens)
        {
            string normToken = token;
            // If the token is a variable, normalize it and check if it's legal.
            if (IsValidVariable(normToken))
            {
                normToken = normalize(token);
                if (!IsValidVariable(normToken))
                    throw new FormulaFormatException("The normalized variable \""
                        + normToken + "\" is not a legal variable in terms of the" +
                        " general syntax rules.");
                if (!isValid(normToken))
                    throw new FormulaFormatException("The normalized variable \""
                        + normToken + "\" is not a legal variable in terms of the" +
                        " validator's rules.");
            }
            newFormula.Append(normToken);
        }
        this.formula = newFormula.ToString().Trim();
    }

    /// <summary>
    /// Checks if the given token is a variable (an underscore or letter followed by 0 or more underscores,
    /// letters, or numbers).
    /// </summary>
    /// <param name="variable"> The string to be checked if it is a variable. </param>
    /// <returns> True if it is a variable, false otherwise. </returns>
    private static bool IsValidVariable(string token)
    {
        return Regex.IsMatch(token, @"^[_A-Za-z][_0-9A-Za-z]*$");
    }

    /// <summary>
    /// Checks if the given string expression is syntactically valid; that is, the expression
    /// follows the rules for standard infix notation. If the expression is invalid, a 
    /// FormulaFormatException is thrown and an appropriate reasoning is provided to the user.
    /// </summary>
    /// <param name="expression"> The expression to be checked for syntax correctness. </param>
    /// <exception cref="FormulaFormatException"> Thrown when an expression token is invalid. </exception>
    private static void IsValidExp(string expression)
    {
        int leftParenthCount = 0;
        int rightParenthCount = 0;
        int tokenCount = 0;
        string prevToken = "";
        IEnumerable<string> tokens = GetTokens(expression);
        foreach (string token in tokens)
        {
            // [Parsing]: Is the token a valid variable, operator, or decimal number?
            if (!IsValidVariable(token) && !Regex.IsMatch(token, @"^[+\-*/\)\(]$") && !double.TryParse(token, out _))
                throw new FormulaFormatException("Invalid expression. The token \"" +
                    token + "\" is not valid.");

            // [Starting Token]: The first token must be a number, variable, or opening parenthesis.
            if (tokenCount == 0 && !double.TryParse(token, out _) && !IsValidVariable(token) && token != "(")
                throw new FormulaFormatException("Invalid expression. The first token of the " +
                    "expression must start with a number, a variable, or an opening parenthesis.");

            else if (token == "(")
                leftParenthCount++;
            else if (token == ")")
                rightParenthCount++;

            // [Operator Following]: Any token following an open parenthesis or an operator must be
            // a number, a variable, or an opening parenthesis.
            if (Regex.IsMatch(prevToken, @"^[+\-*\(/]$") && !IsValidVariable(token) && 
                !double.TryParse(token, out _) && token != "(")
                throw new FormulaFormatException("Invalid expression. A token following an open " +
                    "parenthesis must be a number, variable, or another opening parenthesis.");

            // [Extra Following]: Any token following a number, variable, or a closing parenthesis
            // must be an operator or another closing parenthesis.
            if ((double.TryParse(prevToken, out _) || IsValidVariable(prevToken) || prevToken == ")")
                && !Regex.IsMatch(token, @"^[+\-*\)/]$"))
                throw new FormulaFormatException("Invalid expression. A token following a " +
                    "number, variable, or a closing parenthesis must be an operator or another " +
                    "closing parenthesis.");

            // [Right Parentheses]: The number of closing parentheses may not exceed the number of
            // opening parentheses.
            if (rightParenthCount > leftParenthCount)
                throw new FormulaFormatException("Invalid expression. The number of closing " +
                    "parentheses is greater than the number of opening parentheses.");

            prevToken = token;
            tokenCount++;
        }
        // [One Token]: There must be at least one token.
        if (tokenCount < 1)
            throw new FormulaFormatException("Invalid expression. There must be at least one " +
                "token in the expression.");

        // [Ending Token]: The last token must be a number, a variable, or a closing parenthesis.
        else if (!Double.TryParse(prevToken, out _) && !IsValidVariable(prevToken) && prevToken != ")")
            throw new FormulaFormatException("Invalid expression. The first token of the " +
                "expression must start with a number, a variable, or an opening parenthesis.");

        // [Balanced Parentheses]: The number of parentheses must be balanced.
        else if (leftParenthCount != rightParenthCount)
            throw new FormulaFormatException("Invalid expression. The number of opening parentheses " +
                "must match the number of closing parentheses.");
    }

    /// <summary>
    /// Given two integer values and an operator, the respective computations are made
    /// on the variables and the result is returned.
    /// </summary>
    /// <param name="val1"> The first variable of the computation. </param>
    /// <param name="oper"> The operator to be used in the computation. </param>
    /// <param name="val2"> The second variable of the computation. </param>
    /// <returns> The result of the computation. </returns>
    private static double PerformComputation(double val1, string oper, double val2)
    {
        double result = 0;
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
                result = val2 / val1;
                break;
        }
        return result;
    }

    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
    /// the constructor.)
    /// 
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
    /// in a string to upper case:
    /// 
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    /// 
    /// Given a variable symbol as its parameter, lookup returns the variable's value 
    /// (if it has one) or throws an ArgumentException (otherwise).
    /// 
    /// If no undefined variables or divisions by zero are encountered when evaluating 
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        Stack<double> valStack = new();   // Value token stack
        Stack<string> operStack = new();  // Operator token stack

        IEnumerable<string> tokens = GetTokens(formula);
        foreach (string token in tokens)
        {
            // Integer & variable action:
            if (double.TryParse(token, out double result) || IsValidVariable(token))
            {
                // If the token is a variable, use lookup to retrieve the double value.
                if (IsValidVariable(token))
                {
                    // If there is no defined lookup value for the token, a FormulaError object is returned.
                    try { result = lookup(token); }
                    catch { return new FormulaError("Err: Undefined"); }
                }

                if (operStack.OnTop("*", "/"))
                {
                    if (operStack.Peek() == "/" && result == 0)
                        return new FormulaError("Err: #DIV/0!");
                    valStack.Push(PerformComputation(result, operStack.Pop(), valStack.Pop()));
                }
                else
                    valStack.Push(result);
            }

            // Operator action--multiply, divide, opening parenthesis:
            else if (Regex.IsMatch(token, @"^[*(\/]$"))
                operStack.Push(token);

            // Operator action--add or subtract:
            else if (Regex.IsMatch(token, @"^[+\-]$"))
            {
                if (operStack.OnTop("+", "-"))
                    valStack.Push(PerformComputation(valStack.Pop(), operStack.Pop(), valStack.Pop()));
                operStack.Push(token);
            }

            else if (token == ")")
            {
                if (operStack.OnTop("+", "-"))
                    valStack.Push(PerformComputation(valStack.Pop(), operStack.Pop(), valStack.Pop()));
                operStack.Pop();    // Pops an opening parenthesis.

                if (operStack.OnTop("*", "/"))
                {
                    if (operStack.Peek() == "/" && valStack.Peek() == 0)
                        return new FormulaError("A division by zero is not legal.");
                    valStack.Push(PerformComputation(valStack.Pop(), operStack.Pop(), valStack.Pop()));
                }
            }
        }
        // If elements remain in the operator stack:
        if (operStack.Count == 1 && operStack.OnTop("+", "-") && valStack.Count == 2)
            return PerformComputation(valStack.Pop(), operStack.Pop(), valStack.Pop());
        return valStack.Pop();  // Otherwise, the final result is in valStack.
    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this 
    /// formula.  No normalization may appear more than once in the enumeration, even 
    /// if it appears more than once in this Formula.
    /// 
    /// For example, if N is a method that converts all the letters in a string to upper case:
    /// 
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<String> GetVariables()
    {
        HashSet<string> variablesSet = new();
        IEnumerable<string> tokens = GetTokens(formula);
        // For all tokens in the formula, store only the variables in the variables HashSet.
        foreach (string token in tokens)
            if (IsValidVariable(token))
                variablesSet.Add(token);
        return variablesSet;
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    /// 
    /// For example, if N is a method that converts all the letters in a string to upper case:
    /// 
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        return formula;
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    /// 
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings 
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" 
    /// by C#'s standard conversion from string to double, then back to string. This 
    /// eliminates any inconsistencies due to limited floating point precision.
    /// Variable tokens are considered equal if their normalized forms are equal, as 
    /// defined by the provided normalizer.
    /// 
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///  
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not Formula)
            return false;
        Formula objFormula = (Formula)obj;

        IEnumerable<string> thisTokens = GetTokens(formula);
        IEnumerable<string> objTokens = GetTokens(objFormula.ToString());

        int count = 0;
        foreach (string token in thisTokens)
        {
            // If the token is a number:
            if (double.TryParse(token, out double thisNum))
            {
                double.TryParse(objTokens.ElementAt(count), out double objNum);
                if (thisNum.ToString() != objNum.ToString())
                    return false;
            }
            // If the token is a variable, operator, or parenthesis sign:
            else if (token != objTokens.ElementAt(count))
                return false;
            count++;
        }
        return true;
    }

    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.Equals(f2);
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !f1.Equals(f2);
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        double hash = 0;
        // Sum the values of all tokens in the formula. 
        IEnumerable<string> tokens = GetTokens(formula);
        foreach (string token in tokens)
        {
            // If it is a decimal number, simply add the number to the hash.
            if (double.TryParse(token, out double num))
                hash += num;
            // Otherwise, add the hashcode of each character to the hash.
            else
                foreach (char c in token)
                    hash += c.GetHashCode();
        }
        return (int)hash;
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
    /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
    /// match one of those patterns.  There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(String formula)
    {
        // Patterns for individual tokens
        String lpPattern = @"\(";
        String rpPattern = @"\)";
        String opPattern = @"[\+\-*/]";
        String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        String spacePattern = @"\s+";

        // Overall pattern
        String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }
    }
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(String message)
        : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(String reason)
        : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}


/// <summary>
/// This class holds all extension methods that act as 'syntactic sugar' for the Stack class.
/// </summary>
public static class PS1StackExtensions
{
    /// <summary>
    /// For a given stack, checks that the stack is not empty and that the top element of the stack is
    /// one of the given operator parameters.
    /// </summary>
    /// <param name="s"> The stack to be checked </param>
    /// <param name="oper1"> The first potential operator to be on top </param>
    /// <param name="oper2"> The second potential operator to be on top </param>
    /// <returns> True if the stack isn't empty and either operator is on top, false otherwise. </returns>
    public static bool OnTop(this Stack<string> stack, string oper1, string oper2)
    {
        return stack.Count > 0 && (stack.Peek() == oper1 || stack.Peek() == oper2);
    }
}