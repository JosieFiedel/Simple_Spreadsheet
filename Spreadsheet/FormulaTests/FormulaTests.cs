using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FormulaTests
{
    [TestClass]
    public class FormulaTests
    {
        /// <summary>
        /// Test #1 provided in the Formula method XML. The validator returns true if the string consists of one 
        /// letter followed by one digit.
        /// </summary>
        [TestMethod]
        public void SimpleFormulaNoException()
        {
            new Formula("x2+y3", s => s.ToUpper(), s => (s.Length > 1 && char.IsLetter(s[0]) && char.IsDigit(s[1])));
        }

        /// <summary>
        /// Tests combined variables, decimal number, parentheses, and operators with a provided normalizer
        /// and a default validator.
        /// </summary>
        [TestMethod]
        public void SimpleFormulaNoException2()
        {
            Formula formula = new Formula("(_a2 + b4) / 3", s => s.ToUpper(), s => true);
        }

        /// <summary>
        /// Test #2 provided in the Formula method XML. The validator returns true if the string consists of one 
        /// letter followed by one digit. An exception should be thrown since 'x' is not a valid variable name
        /// in terms of the validator.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void SimpleFormulaException()
        {
            new Formula("x+y3", s => s.ToUpper(), s => (s.Length > 1 && char.IsLetter(s[0]) && char.IsDigit(s[1])));
        }

        /// <summary>
        /// Test #3 provided in the Formula method XML. The validator returns true if the string consists of one 
        /// letter followed by one digit. An exception should be thrown since 2x is not a general valid variable
        /// name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void SimpleFormulaException2()
        {
            new Formula("2x+y3", s => s.ToUpper(), s => (s.Length > 1 && char.IsLetter(s[0]) && char.IsDigit(s[1])));
        }

        /// <summary>
        /// Test #1 provided in the Evaluate method XML. For any given variable, the lookup delegate returns a 2.
        /// </summary>
        [TestMethod]
        public void SimpleEvaluate()
        {
            Assert.AreEqual(11.0, new Formula("x+7", s => s.ToUpper(), s => true).Evaluate(s => 4));
        }

        /// <summary>
        /// Test #2 provided in the Evaluate method XML. For any given variable, the lookup delegate returns a 2.
        /// </summary>
        [TestMethod]
        public void SimpleEvaluate2()
        {
            Assert.AreEqual(9.0, new Formula("x+7").Evaluate(s => 2));
        }

        /// <summary>
        /// Test #1 provided in the GetVariables method XML. The following should enumerate "X", "Y", and "Z".
        /// </summary>
        [TestMethod]
        public void SimpleGetVariables()
        {
            IEnumerator<string> e = new Formula("x+y*z", s => s.ToUpper(), s => true).GetVariables().GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("X", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("Y", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("Z", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        /// <summary>
        /// Test #2 provided in the GetVariables method XML. The following should enumerate "X" and "Z".
        /// </summary>
        [TestMethod]
        public void SimpleGetVariables2()
        {
            IEnumerator<string> e = new Formula("x+X*z", s => s.ToUpper(), s => true).GetVariables().GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("X", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("Z", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        /// <summary>
        /// Test #3 provided in the GetVariables method XML. The following should enumerate "x", "X", and "Z".
        /// </summary>
        [TestMethod]
        public void SimpleGetVariables3()
        {
            IEnumerator<string> e = new Formula("x+X*z").GetVariables().GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("x", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("X", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("z", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        /// <summary>
        /// Test #1 provided in the ToString method XML. 
        /// </summary>
        [TestMethod]
        public void SimpleToString()
        {
            Assert.AreEqual("X+Y", new Formula("x + y", s => s.ToUpper(), s => true).ToString());
        }

        /// <summary>
        /// Test #2 provided in the ToString method XML. 
        /// </summary>
        [TestMethod]
        public void SimpleToString2()
        {
            Assert.AreEqual("x+Y", new Formula("x + Y").ToString());
        }

        /// <summary>
        /// Test #1 provided in the Equals method XML. Should return true since spaces should not matter when
        /// comparing two formulas.
        /// </summary>
        [TestMethod]
        public void SimpleEquals()
        {
            Assert.IsTrue(new Formula("x1+y2", s => s.ToUpper(), s => true).Equals(new Formula("X1  +  Y2")));
        }

        /// <summary>
        /// Test #2 provided in the Equals method XML. Should return false since there is no normalizer and
        /// lowercase and uppercase letters are considered to be different.
        /// </summary>
        [TestMethod]
        public void SimpleEquals2()
        {
            Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("X1+Y2")));
        }

        /// <summary>
        /// Test #3 provided in the Equals method XML. Should return false since the variables are out
        /// of order.
        /// </summary>
        [TestMethod]
        public void SimpleEquals3()
        {
            Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("y2+x1")));
        }

        /// <summary>
        /// Test #4 provided in the Equals method XML. Should return true despite the different representation
        /// of decimal numbers (2.0 vs. 2.000).
        /// </summary>
        [TestMethod]
        public void SimpleEquals4()
        {
            Assert.IsTrue(new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")));
        }

        [TestMethod]
        public void NullEquals()
        {
            Assert.IsFalse(new Formula("2.0 + x7").Equals(null));
        }

        [TestMethod]
        public void NotAFormulaEquals()
        {
            Assert.IsFalse(new Formula("2.0 + x7").Equals("2.0 + x7"));
        }

        [TestMethod]
        public void DifferentFormulasEquals()
        {
            Assert.IsFalse(new Formula("2.0 + x7 - 1.0").Equals(new Formula("2.0 + x7 - 2.0")));
        }

        [TestMethod]
        public void DifferentFormulasEquals2()
        {
            Assert.IsFalse(new Formula("2.0 + x7 - 1.0").Equals(new Formula("2.0 + x7 * 1.0")));
        }

        [TestMethod]
        public void DoubleEqualsTrue()
        {
            Assert.IsTrue(new Formula("2.0 + x7 - 1.0") == new Formula("2.0 + x7 - 1"));
        }

        [TestMethod]
        public void DoubleEqualsTrueDoubles()
        {
            Assert.IsTrue(new Formula("2.0") == new Formula("2.000"));
        }

        [TestMethod]
        public void DoubleEqualsTrueDoubles2()
        {
            Assert.IsTrue(new Formula("2") == new Formula("2.00"));
        }

        [TestMethod]
        public void DoubleEqualsTrueVariables()
        {
            Assert.IsTrue(new Formula("X+Y") == new Formula("X + Y"));
        }

        [TestMethod]
        public void DoubleEqualsFalse()
        {
            Assert.IsFalse(new Formula("2.0 + x7 - 1.0") == new Formula("2.0 + x7 * 1.0"));
        }

        [TestMethod]
        public void DoubleEqualsHashCodeTrue()
        {
            Assert.IsTrue(new Formula("2.0 + x7 - 1.0").GetHashCode() == new Formula("2.0 + x7 - 1").GetHashCode());
        }

        [TestMethod]
        public void DoubleEqualsHashCodeFalse()
        {
            Assert.IsFalse(new Formula("2.0 + x7 - 1.0").GetHashCode() == new Formula("2.0 + x7 * 1").GetHashCode());
        }

        [TestMethod]
        public void NotEqualsFalse()
        {
            Assert.IsFalse(new Formula("2.0 + x7 - 1.0") != new Formula("2.0 + x7 - 1"));
        }

        [TestMethod]
        public void NotEqualsTrue()
        {
            Assert.IsTrue(new Formula("2.0 + x7 - 1.0") != new Formula("2.0 + x7 * 1.0"));
        }

        [TestMethod]
        public void SimpleEnclosedByParentheses()
        {
            Formula formula = new Formula("(a)", s => s.ToUpper(), s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void BadSyntaxSpacedIntegers()
        {
            new Formula("1 1");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void BadSyntaxNegativeInteger()
        {
            new Formula("-1 + 1");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void BadSyntaxParenthesisOperator()
        {
            new Formula("(*1-5)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void BadSyntaxMultiplyNegNumber()
        {
            new Formula("1*-6");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void BadSyntaxMoreLeftParenthesis()
        {
            new Formula("((1-5)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void BadSyntaxNormalizer()
        {
            new Formula("A + B", s => s + "$", s => true);
        }

        /// <summary>
        /// Determines if a given string is fully uppercase.
        /// </summary>
        /// <param name="str"> The string to be determined if it is uppercase. </param>
        /// <returns>True if fully uppercase, false otherwise. </returns>
        private bool testValidator(string str)
        {
            foreach (char c in str)
                if (!char.IsUpper(c))
                    return false;
            return true;
        }

        [TestMethod]
        public void BadSyntaxValidator()
        {
            new Formula("A + B", s => s, testValidator);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void BadSyntaxValidator2()
        {
            new Formula("A + b", s => s, testValidator);
        }

        /// <summary>
        /// This validator returns an integer value instead of another variable. This should not be
        /// legal since the validator must return a valid variable.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void IntegerValidator()
        {
            Assert.AreEqual(10, new Formula("A + B", s => "5", s => true));
        }

        /// <summary>
        /// This validator returns an integer value instead of another variable.
        /// This time, it returns 0, which should return a FormulaError object
        /// since a number cannot divide by zero.
        /// </summary>
        [TestMethod]
        public void DividingbyZero()
        {
            Assert.IsInstanceOfType(new Formula("A / B", s => s, s => true).Evaluate(s => 0), typeof(FormulaError));
        }

        [TestMethod]
        public void DividingbyZero2()
        {
            Assert.IsInstanceOfType(new Formula("(3)/(6/3)/(4-4)").Evaluate(s => 0), typeof(FormulaError));
        }

        [TestMethod]
        public void SimpleScientificNotationAddition()
        {
            Assert.AreEqual(4200.0, new Formula("2e2 + 2e3 * 2").Evaluate(s => 0));
        }

        // TESTS FROM PS1 FOR THE EVALUATE METHOD:--------------------------------------------------------------

        [TestMethod(), Timeout(5000)]
        public void TestSingleNumber()
        {
            Assert.AreEqual(5.0, new Formula("5").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestSingleVariable()
        {
            Assert.AreEqual(13.0, new Formula("X5").Evaluate(s => 13));
        }

        [TestMethod(), Timeout(5000)]
        public void TestAddition()
        {
            Assert.AreEqual(8.0, new Formula("5+3").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestSubtraction()
        {
            Assert.AreEqual(8.0, new Formula("18-10").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestMultiplication()
        {
            Assert.AreEqual(8.0, new Formula("2*4").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestDivision()
        {
            Assert.AreEqual(8.0, new Formula("16/2").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestArithmeticWithVariable()
        {
            Assert.AreEqual(5.0, new Formula("X2+1").Evaluate(s => 4));
        }

        [TestMethod(), Timeout(5000)]
        public void TestUnknownVariable()
        {
            Assert.IsInstanceOfType(new Formula("2+X1").Evaluate(s => { throw new ArgumentException("Unknown variable"); }), typeof(FormulaError));
        }

        [TestMethod(), Timeout(5000)]
        public void TestLeftToRight()
        {
            Assert.AreEqual(15.0, new Formula("2*6+3").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestOrderOperations()
        {
            Assert.AreEqual(20.0, new Formula("2+6*3").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestParenthesesTimes()
        {
            Assert.AreEqual(24.0, new Formula("(2+6)*3").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestTimesParentheses()
        {
            Assert.AreEqual(16.0, new Formula("2*(3+5)").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestPlusParentheses()
        {
            Assert.AreEqual(10.0, new Formula("2+(3+5)").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestPlusComplex()
        {
            Assert.AreEqual(50.0, new Formula("2+(3+5*9)").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestOperatorAfterParens()
        {
            Assert.AreEqual(0.0, new Formula("(1*1)-2/2").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestComplexTimesParentheses()
        {
            Assert.AreEqual(26.0, new Formula("2+3*(3+5)").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestComplexAndParentheses()
        {
            Assert.AreEqual(194.0, new Formula("2+3*5+(3+4*8)*5+2").Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        public void TestDivideByZero()
        {
            Assert.IsInstanceOfType(new Formula("5/0").Evaluate(s => 0), typeof(FormulaError));
        }

        [TestMethod(), Timeout(5000)]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSingleOperator()
        {
            new Formula("+").Evaluate(s => 0);
        }

        [TestMethod(), Timeout(5000)]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestExtraOperator()
        {
            new Formula("2+5+").Evaluate(s => 0);
        }

        [TestMethod(), Timeout(5000)]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestExtraParentheses()
        {
            new Formula("2+5*7)").Evaluate(s => 0);
        }

        [TestMethod(), Timeout(5000)]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestInvalidVariable()
        {
            new Formula("$").Evaluate(s => 0);
        }

        [TestMethod(), Timeout(5000)]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestPlusInvalidVariable()
        {
            new Formula("5+$").Evaluate(s => 0);
        }

        [TestMethod(), Timeout(5000)]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestParensNoOperator()
        {
            new Formula("5+7+(5)8").Evaluate(s => 0);
        }

        [TestMethod(), Timeout(5000)]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestEmpty()
        {
            new Formula("").Evaluate(s => 0);
        }

        [TestMethod(), Timeout(5000)]
        public void TestComplexNestedParensRight()
        {
            Assert.AreEqual(6.0, new Formula("x1+(x2+(x3+(x4+(x5+x6))))").Evaluate(s => 1));
        }

        [TestMethod(), Timeout(5000)]
        public void TestComplexNestedParensLeft()
        {
            Assert.AreEqual(12.0, new Formula("((((x1+x2)+x3)+x4)+x5)+x6").Evaluate(s => 2));
        }

        [TestMethod(), Timeout(5000)]
        public void TestRepeatedVar()
        {
            Assert.AreEqual(0.0, new Formula("a4-a4*a4/a4").Evaluate(s => 3));
        }
    }
}