// Class representing a spreadsheet, incorporating the Formula and GraphDependency classes
// to represent cell dependencies and evaluations of formulas within cells.
//
// Implemented September 30, 2022 -- Josie Fiedel 

using Newtonsoft.Json;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;

namespace SS;

/// <summary>
/// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
/// spreadsheet consists of an infinite number of named cells.
/// 
/// A string is a cell name if and only if it consists of one or more letters,
/// followed by one or more digits AND it satisfies the predicate IsValid.
/// For example, "A15", "a15", "XY032", and "BC7" are cell names so long as they
/// satisfy IsValid.  On the other hand, "Z", "X_", and "hello" are not cell names,
/// regardless of IsValid.
/// 
/// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
/// must be normalized with the Normalize method before it is used by or saved in 
/// this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
/// the Formula "x3+a5" should be converted to "X3+A5" before use.
/// 
/// A spreadsheet contains a cell corresponding to every possible cell name.  
/// In addition to a name, each cell has a contents and a value.  The distinction is
/// important.
/// 
/// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
/// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
/// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
/// 
/// In a new spreadsheet, the contents of every cell is the empty string.
///  
/// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
/// (By analogy, the value of an Excel cell is what is displayed in that cell's position
/// in the grid.)
/// 
/// If a cell's contents is a string, its value is that string.
/// 
/// If a cell's contents is a double, its value is that double.
/// 
/// If a cell's contents is a Formula, its value is either a double or a FormulaError,
/// as reported by the Evaluate method of the Formula class.  The value of a Formula,
/// of course, can depend on the values of variables.  The value of a variable is the 
/// value of the spreadsheet cell it names (if that cell's value is a double) or 
/// is undefined (otherwise).
/// 
/// Spreadsheets are never allowed to contain a combination of Formulas that establish
/// a circular dependency.  A circular dependency exists when a cell depends on itself.
/// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
/// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
/// dependency.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Spreadsheet : AbstractSpreadsheet
{
    public delegate void UpdateCellsHandler(string text, string prevCell, string input);
    public event UpdateCellsHandler UpdateCells;

    [JsonProperty]
    private readonly Dictionary<string, Cell> cells;  // Holds all of the non-empty cells in the spreadsheet.
    private readonly DependencyGraph graph;           // Keeps track of relations among cell names.


    /// <summary>
    /// True if this spreadsheet has been modified since it was created or saved                  
    /// (whichever happened most recently); false otherwise.
    /// </summary>
    public override bool Changed { get; protected set; }


    /// <summary>
    /// Creates a default, empty spreadsheet. 
    /// Invokes the 4-parameter constructor, passing in default
    /// parameter values to be initialized. 
    /// </summary>
    public Spreadsheet() : this("", s => true, s => s, "default")
    { }
    

    /// <summary>
    /// Creates an empty spreadsheet.
    /// Invokes the 4-parameter constructor, passing in the following user-provided parameters:
    /// a validity delegate, a normalization delegate, and a version.
    /// 
    /// </summary>
    /// <param name="valDelegate"> Validity delegate </param>
    /// <param name="normDelegate"> Normalization delegate </param>
    /// <param name="versString"> Version </param>
    public Spreadsheet(Func<string, bool> valDelegate, Func<string, string> normDelegate, string versString)
        : this("", valDelegate, normDelegate, versString)
    { }


    /// <summary>
    /// "Recreates" a saved spreadsheet from a file. If the file exists, it is deserialized and all
    /// cell values in the spreadsheet are restored.
    /// Initializes the following user-provided parameters: a file path, a validity delegate, a
    /// normalization delegate, and a version.
    /// 
    /// </summary>
    /// <param name="filePath"> File path </param>
    /// <param name="valDelegate"> Validity delgate </param>
    /// <param name="normDelegate"> Normalization delegate </param>
    /// <param name="versString"> Version </param>
    public Spreadsheet(string filePath, Func<string, bool> valDelegate, Func<string, string> normDelegate, 
        string versString) : base(valDelegate, normDelegate, versString)
    {
        cells = new();
        graph = new();
        IsValid = valDelegate;
        Normalize = normDelegate;
        Changed = false;

        if (filePath != "")
        {
            // Read the saved spreadsheet from the filePath. 
            Spreadsheet? ss;
            try { ss = JsonConvert.DeserializeObject<Spreadsheet>(File.ReadAllText(filePath)); }
            catch { throw new SpreadsheetReadWriteException("An error occurred when reading the file."); }

            if(ss != null)
            {
                // Repopulate this cells dictionary using the saved spreadsheet.
                foreach (string name in ss.GetNamesOfAllNonemptyCells())
                    if (ss.cells.TryGetValue(name, out Cell? cell))
                        SetContentsOfCell(name, (string)cell.Content);
                
                if (ss.Version != versString)
                    throw new SpreadsheetReadWriteException("The spreadsheet version and parameter version do not align.");
            }
        }
    }


    /// <summary>
    /// Writes the contents of this spreadsheet to the named file using a JSON format.
    /// The JSON object should have the following fields:
    /// "Version" - the version of the spreadsheet software (a string)
    /// "cells" - an object containing 0 or more cell objects
    ///           Each cell object has a field named after the cell itself 
    ///           The value of that field is another object representing the cell's contents
    ///               The contents object has a single field called "stringForm",
    ///               representing the string form of the cell's contents
    ///               - If the contents is a string, the value of stringForm is that string
    ///               - If the contents is a double d, the value of stringForm is d.ToString()
    ///               - If the contents is a Formula f, the value of stringForm is "=" + f.ToString()
    /// 
    /// For example, if this spreadsheet has a version of "default" 
    /// and contains a cell "A1" with contents being the double 5.0 
    /// and a cell "B3" with contents being the Formula("A1+2"), 
    /// a JSON string produced by this method would be:
    /// 
    /// {
    ///   "cells": {
    ///     "A1": {
    ///       "stringForm": "5"
    ///     },
    ///     "B3": {
    ///       "stringForm": "=A1+2"
    ///     }
    ///   },
    ///   "Version": "default"
    /// }
    /// 
    /// If there are any problems opening, writing, or closing the file, the method should throw a
    /// SpreadsheetReadWriteException with an explanatory message.
    /// </summary>
    public override void Save(string filename)
    {
        string jsonText = JsonConvert.SerializeObject(this, Formatting.Indented);

        try { File.WriteAllText(filename, jsonText); }
        catch { throw new SpreadsheetReadWriteException("Error in reading/writing the file."); }

        Changed = false;
    }


    /// <summary>
    /// Enumerates the names of all the non-empty cells in the spreadsheet.
    /// </summary>
    public override IEnumerable<string> GetNamesOfAllNonemptyCells()
    {
        return new HashSet<string>(cells.Keys);
    }


    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
    /// value should be either a string, a double, or a Formula.
    /// </summary>
    public override object GetCellContents(string name)
    {
        name = Normalize(name);
        IsValidNameCheck(name);

        // If the cell is empty (not contained in the dictionary), an empty string is returned.
        if (!cells.TryGetValue(name, out Cell? cell))
            return "";

        // Return the appropriate type of object (string, double, or Formula).
        string content = (string)cell.Content;
        if(content.StartsWith('='))
            return new Formula(content[1..], Normalize, IsValid);
        if (double.TryParse(content, out double d))
           return d;
        return content;
    }


    /// <summary>
    /// Checks if the string is a valid cell name. The string passes the general test if it is
    /// one or more letters followed by 1 or more numbers. The second
    /// test uses the IsValid test, which is passed in by anything utilizing the Spreadsheet class.
    /// If either test fails, the string name is invalid and an InvalidNameException is thrown.
    /// </summary>
    /// <param name="name"> Valid/invalid cell name </param>
    /// <returns> true if the name is a valid cell name </returns>
    /// <exception cref="InvalidNameException"> Thrown if the name is invalid. </exception>
    private bool IsValidNameCheck(string name)
    {
        if (!Regex.IsMatch(name, @"^[A-Za-z]+[0-9]+$") || !IsValid(name))
            throw new InvalidNameException();
        return true;
    }
    

    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, the contents of the named cell becomes number.  The method returns a
    /// list consisting of name plus the names of all other cells whose value depends, 
    /// directly or indirectly, on the named cell.
    /// 
    /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    /// list {A1, B1, C1} is returned.
    /// </summary>
    protected override IList<string> SetCellContents(string name, double number)
    {
        // If the cell already exists in the list, change its contents and value.
        if (cells.TryGetValue(name, out Cell? cell))
            cell.Content = number;
        // Otherwise, add the new cell to the cells list.
        else
            cells.Add(name, new Cell(number, LookupValue));
        // The newly-changed cell should have no dependees.
        graph.ReplaceDependees(name, new HashSet<string>());

        return new List<string>(GetCellsToRecalculate(name));
    }


    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, the contents of the named cell becomes text.  The method returns a
    /// list consisting of name plus the names of all other cells whose value depends, 
    /// directly or indirectly, on the named cell.
    /// 
    /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    /// list {A1, B1, C1} is returned.
    /// </summary>
    protected override IList<string> SetCellContents(string name, string text)
    {
        // If the text is empty, the cell is removed from the cells list if it exists.
        if (text == "")
            cells.Remove(name);
        else
        {
            // If the cell already exists in the list, change its contents.
            if (cells.TryGetValue(name, out Cell? cell))
                cell.Content = text;
            // Otherwise, add the new cell to the cells list.
            else
                cells.Add(name, new Cell(text, LookupValue));
        }
        // The newly-changed cell should have no dependees.
        graph.ReplaceDependees(name, new HashSet<string>());
        
        return new List<string>(GetCellsToRecalculate(name));
    }


    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
    /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
    /// 
    /// Otherwise, the contents of the named cell becomes formula.  The method returns a
    /// list consisting of name plus the names of all other cells whose value depends,
    /// directly or indirectly, on the named cell.
    /// 
    /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    /// list {A1, B1, C1} is returned.
    /// </summary>
    protected override IList<string> SetCellContents(string name, Formula formula)
    {
        // Save the the old dependees and contents of the cell in case of an exception.
        IEnumerable<string> oldDependees = graph.GetDependees(name);
        object oldContents = "";
        if(cells.ContainsKey(name))
            oldContents = cells[name].Content;
    
        // Replace the dependees of the cell with the new formula dependees.
        graph.ReplaceDependees(name, formula.GetVariables());
        IList<string> dependentCells = new List<string>();
        try
        {
            // If the cell already exists in the list, change its contents.
            if (cells.TryGetValue(name, out Cell? cell))
                cell.Content = formula;
            // Otherwise, add the new cell to the cells list.
            else
                cells.Add(name, new Cell(formula, LookupValue));

            // The list of dependents of the given cell.
            dependentCells = new List<string>(GetCellsToRecalculate(name));
        }
        // If a circular exception is thrown when adding/updating the cell contents, all relationships are reverted
        // back in the spreadsheet and the exception is thrown.
        catch
        {
            graph.ReplaceDependees(name, oldDependees);
            if (oldContents.Equals(""))
                cells.Remove(name);
            else if (cells.TryGetValue(name, out Cell? oldCell))
                oldCell.Content = oldContents;
            throw new CircularException();
        }
        return new List<string>(dependentCells);
    }


    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, if content parses as a double, the contents of the named
    /// cell becomes that double.
    /// 
    /// Otherwise, if content begins with the character '=', an attempt is made
    /// to parse the remainder of content into a Formula f using the Formula
    /// constructor.  There are then three possibilities:
    /// 
    ///   (1) If the remainder of content cannot be parsed into a Formula, a 
    ///       SpreadsheetUtilities.FormulaFormatException is thrown.
    ///       
    ///   (2) Otherwise, if changing the contents of the named cell to be f
    ///       would cause a circular dependency, a CircularException is thrown,
    ///       and no change is made to the spreadsheet.
    ///       
    ///   (3) Otherwise, the contents of the named cell becomes f.
    /// 
    /// Otherwise, the contents of the named cell becomes content.
    /// 
    /// If an exception is not thrown, the method returns a list consisting of
    /// name plus the names of all other cells whose value depends, directly
    /// or indirectly, on the named cell. The order of the list should be any
    /// order such that if cells are re-evaluated in that order, their dependencies 
    /// are satisfied by the time they are evaluated.
    /// 
    /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    /// list {A1, B1, C1} is returned.
    /// </summary>
    public override IList<string> SetContentsOfCell(string name, string content)
    {
        name = Normalize(name);
        IsValidNameCheck(name);

        IEnumerable<string> recalcCells;
        // If the content is a double, it is stored in the cell as a double.
        if (double.TryParse(content, out double doub))
            recalcCells = SetCellContents(name, doub);
        // If the content is a formula, it is stored in the cell as a formula.
        else if (content.StartsWith('='))
        {
            Formula f;
            try { f = new(content[1..], Normalize, IsValid); }
            catch { throw new FormulaFormatException("Illegal formula name."); }

            recalcCells = SetCellContents(name, f);
        }
        // Otherwise, the content is stored in the cell as a string.
        else
            recalcCells = SetCellContents(name, content);

        // Update the cell value of any dependent cell.
        foreach (string dependentCell in recalcCells)
            if (cells.TryGetValue(dependentCell, out Cell? cell))
                cell.UpdateCellValue();

        Changed = true;
        return new List<string>(recalcCells);
    }


    /// <summary>
    /// Returns an enumeration, without duplicates, of the names of all cells whose
    /// values depend directly on the value of the named cell.  In other words, returns
    /// an enumeration, without duplicates, of the names of all cells that contain
    /// formulas containing name.
    /// 
    /// For example, suppose that
    /// A1 contains 3
    /// B1 contains the formula A1 * A1
    /// C1 contains the formula B1 + A1
    /// D1 contains the formula B1 - C1
    /// The direct dependents of A1 are B1 and C1
    /// </summary>
    protected override IEnumerable<string> GetDirectDependents(string name)
    {
        return graph.GetDependents(Normalize(name));
    }


    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
    /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
    /// </summary>
    public override object GetCellValue(string name)
    {
        name = Normalize(name);
        IsValidNameCheck(name);

        if (!cells.ContainsKey(name))
            return "";
        return cells[name].Value;
    }


    /// <summary>
    /// Looks up the cell value from the provided variable. If no cell exists, an ArgumentException is thrown.
    /// Otherwise, a double value from that looked up cell is returned.
    /// </summary>
    /// <param name="variable"></param>
    /// <returns> The double corresponding to the cell that is looked up. </returns>
    /// <exception cref="ArgumentException"> Thrown if no value exists. </exception>
    private double LookupValue(string variable)
    {
        if (cells.ContainsKey(variable) && cells[variable].Value is double dub)
            return dub;
        throw new ArgumentException("No value exists.");
    }


    /// <summary>
    /// Passes in an AscendingSort IComparer to the sort method to sort the given selection.
    /// </summary>
    /// <param name="clickedCell"> The selected cell </param>
    /// <param name="selection"> Array of lists of all cells in the selection </param>
    /// <returns> The new contents of the clicked cell </returns>
    public string InOrderSort(string clickedCell, List<string>[] selection)
    {
        return SortSelection(clickedCell, selection, new AscendingSort());   
    }


    /// <summary>
    /// Passes in a DescendingSort IComparer to the sort method to sort the given selection.
    /// </summary>
    /// <param name="clickedCell"> The selected cell </param>
    /// <param name="selection"> Array of lists of all cells in the selection </param>
    /// <returns> The new contents of the clicked cell </returns>
    public string ReverseSort(string clickedCell, List<string>[] selection)
    {
        return SortSelection(clickedCell, selection, new DescendingSort());
    }


    /// <summary>
    /// Creates a list for each column in the selection, calling the appropriate sort method
    /// (ascending / descending) to sort the columns. If one of the cells in the selection contains
    /// a FormulaError object, then an ArgumentException is thrown. As the cells are updated, their
    /// dependents are also updated by the use of an event. 
    /// </summary>
    /// <param name="clickedCell"> The selected cell </param>
    /// <param name="selection"> Array of lists of all cells in the selection </param>
    /// <param name="sort"> The Comparer to sort the columns </param>
    /// <returns> Text: The new contents of the clicked cell </returns>
    /// <exception cref="ArgumentException"> Thrown if there is a FormulaError to be sorted </exception>
    public string SortSelection(string clickedCell, List<string>[] selection, IComparer<string> sort)
    {
        // A single cell selection is unnecessary to sort.
        if (selection.Length > 1 || selection[0].Count > 1)
        {
            // Columns are traversed and each list is sorted. 
            for (int i = 0; i < selection.Length; i++)
            {
                // Retrieve the string values of each cell and store in a new list.
                List<string> sortedCol = new();
                for (int j = 0; j < selection[i].Count; j++)
                {
                    if (cells.ContainsKey(selection[i][j]))
                    {
                        if (cells[selection[i][j]].Value is FormulaError)
                            throw new ArgumentException("Error: Cannot sort formula errors.");
                        sortedCol.Add(cells[selection[i][j]].Value.ToString()!);
                    }
                    else
                        sortedCol.Add("");
                }
                sortedCol.Sort(sort);

                // For every cell in the original list, change its value to the sorted value.
                for (int k = 0; k < sortedCol.Count; k++)
                    UpdateCells?.Invoke(sortedCol[0], selection[i][k], sortedCol[k]);
            }
        }

        // Determine the contents of the clicked cell to return. 
        object contents = GetCellContents(clickedCell);
        string text;
        if (contents is Formula formula)
            text = "=" + formula.ToString();
        else
            text = contents.ToString()!;

        return text;
    }


    /// <summary>
    /// The AscendingSort class contains a Compare method to sort the given string values
    /// in ascending order. 
    /// </summary>
    private class AscendingSort : IComparer<string>
    {
        /// <summary>
        /// If the passed in strings can be parsed into doubles, then the strings are compared
        /// as doubles. Otherwise, they are compared as strings. 
        /// 
        /// Return cases:
        /// -1 means that the first string is smaller than the second.
        /// 0 means that both strings are equal.
        /// 1 means that the first string is larger than the second. 
        /// 
        /// </summary>
        /// <param name="x"> First string to be compared </param>
        /// <param name="y"> Second string to be compared </param>
        /// <returns> *See cases above* </returns>
        public int Compare(string? x, string? y)
        {
            // If the strings are both numbers:
            if (double.TryParse(x, out double doub1) && double.TryParse(y, out double doub2))
            {
                if (doub1 - doub2 > 0)
                    return 1;
                else if (doub1 - doub2 < 0)
                    return -1;
                return 0;
            }
                
            // Otherwise, compare with the default CompareTo:
            return (x!.CompareTo(y));
        }
    }


    /// <summary>
    /// The DescendingSort class contains a Compare method to sort the given string values
    /// in descending order. 
    /// </summary>
    private class DescendingSort : IComparer<string>
    {
        /// <summary>
        /// If the passed in strings can be parsed into doubles, then the strings are compared
        /// as doubles. Otherwise, they are compared as strings. 
        /// 
        /// Return cases:
        /// -1 means that the first string is larger than the second.
        /// 0 means that both strings are equal.
        /// 1 means that the first string is smaller than the second. 
        /// 
        /// </summary>
        /// <param name="x"> First string to be compared </param>
        /// <param name="y"> Second string to be compared </param>
        /// <returns> *See cases above* </returns>
        public int Compare(string? x, string? y)
        {
            // If the strings are both numbers:
            if (double.TryParse(x, out double doub1) && double.TryParse(y, out double doub2))
            {
                if (doub1 - doub2 > 0)
                    return -1;
                else if (doub1 - doub2 < 0)
                    return 1;
                return 0;
            }

            // Otherwise, compare with the default CompareTo:
            return -(x!.CompareTo(y));
        }
    }


    /// <summary>
    /// The Cell class represents a single unit in the spreadsheet, containing either a string, a double,
    /// or a Formula as its contents. The value is either a string, a double, or a Formula error, respective
    /// to the values of other dependee cells.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    private class Cell
    {
        private object contents; // The held string contents of a cell (Formula, double, or just string).
        private object val;      // The value of the cell's contents (string, double, or FormulaError).
        private readonly Func<string, double> lookup;   // Passed method to look up the cell value.

        /// <summary>
        /// Creates a Cell containing a string form of a Formula, double, or just a string as the contents.
        /// </summary>
        /// <param contents="contents"> The string, double, or Formula contents as a string </param>
        public Cell(object contents, Func<string, double> lookup)
        {
            this.contents = contents;
            this.lookup = lookup;
            val = contents;   // The value is updated after the contents are set.
        }

        /// <summary>
        /// Returns and sets the cell contents. It is returned in string form.
        /// </summary>
        [JsonProperty(PropertyName ="stringForm")]
        public object Content
        {
            get 
            {
                if (contents is Formula f)
                    return "=" + f.ToString();
                if(contents is Double d)
                    return d.ToString();
                return (string)contents;
            }
            set { contents = value; }
        }

        /// <summary>
        /// Returns the cell value.
        /// </summary>
        [JsonIgnore]
        public object Value
        {
            get { return val; }
        }

        /// <summary>
        /// Updates the value of the cell whenever its contents are modified.
        /// </summary>
        public void UpdateCellValue()
        {
            if (contents is Formula f)
                val = f.Evaluate(lookup);
            else if (contents is double d)
                val = d;
            else
                val = contents;
        }
    }
}
