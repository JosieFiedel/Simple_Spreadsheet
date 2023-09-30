/// SpreadsheetGUI implementation by Josie Fiedel--October 21, 2022.
using SpreadsheetUtilities;
using SS;
using System.Text.RegularExpressions;
using System.Timers;

namespace SpreadsheetGUI;

/// <summary>
/// This class contains the functionality of the SpreadsheetGUI object. It is functionally and 
/// stylistically similar to Microsoft Excel. When the application is run, a grid of cells (A1-Z99)
/// is displayed, along with a menu bar containing the following spreadsheet features: New, Open,
/// Save, SaveAs, Help. There are two unmodifyable labels that represent the selected cell name and
/// value. A textbox is used to take user input and display it in the spreadsheet. 
/// 
/// Clicking and Typing event handlers are used to notify method action whenever the user clicks on
/// a cell object or text box or whenever text is modified in the textbox. When the user clicks 'New,'
/// they are prompted to save their changes and an empty spreadsheet is drawn. When the user clicks
/// 'Open,' they are again prompted to save their changes and then are able to choose a .sprd file to
/// open. The contents of this file are then written to the spreadsheetGUI. When the user clicks 'Save,'
/// if an explicit filepath exists (by opening a file/prevously saving a file), then the file is simply 
/// saved to that same filepath without prompting the user to enter in a filepath. If a filepath doesn't
/// exist (by creating a new spreadsheet), then the user is prompted to enter in a filepath. When the
/// user clicks 'SaveAs,' they are always prompted to enter in a filepath.
/// 
/// The 'AutoSave' slider is an extra feature that, when toggled, automatically saves the file every
/// 2 minutes. When untoggled, the feature is disabled.
/// 
/// If any exceptions are thrown in the code, a display alert pops up and the previous changes to cause
/// the exception are reverted. Formula errors are simply displayed within the spreadsheet grid. 
/// 
/// See the README for more information.
/// </summary>
public partial class MainPage : ContentPage
{
    SpreadsheetGrid grid;
    Spreadsheet spreadsheet;    // Implicit version of the spreadsheetGUI
    string prevCell;            // The previously-selected cell
    string filepath;            // The filepath of this spreadsheet
    bool autoSaveOn;            // True if AutoSave is on.
    private static System.Timers.Timer myTimer;     // Used for AutoSave. 

    /// <summary>
    /// Constructor for the spreadsheetGUI. The default selected cell is (0, 0) or A1.
    /// For this spreadsheet, the validator ensures that any given variable is a letter
    /// followed by a number. The normalizer converts all variable letters to uppercase.
    /// </summary>
    public MainPage()
    {
        // A new empty spreadsheet is created with an appropriate validator, normalizer, and version. 
        spreadsheet = new Spreadsheet("", s => Regex.IsMatch(s, @"^[A-Z]([1-9]|[1-9][0-9])$"), 
            s => s.ToUpper(), "ps6");
        filepath = "";

        InitializeComponent();
        spreadsheetGrid.SelectionChanged += DisplaySelection;
        spreadsheet.UpdateCells += UpdatePrevCellAndDependents;

        // Set the default selection. 
        spreadsheetGrid.SetSelection(0, 0);
        cellNameDisplay.Text = "A1";
    }


    /// <summary>
    ///	As the text is modified in the textbox, the grid cell content value is changed accordingly. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBoxChanged(object sender, TextChangedEventArgs e)
    {
        spreadsheetGrid.GetClickedCell(out int col, out int row);
        spreadsheetGrid.SetValue(col, row, textBox.Text);
    }


    /// <summary>
    /// When the return key is entered, the selection is changed by incrementing the row by 1. 
    /// To the user, it will appear as though hitting the return key returns to the next line. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBoxReturned(object sender, EventArgs e)
    {
        spreadsheetGrid.GetClickedCell(out int col, out int row);
        spreadsheetGrid.SetSelection(col, row + 1);
    }


    /// <summary>
    /// Notified whenever a selection changed event has occurred. When a cell selection is changed,
    /// the previously selected cell is updated so that it's visual value represents the cell value
    /// instead of the cell contents. All of the cells dependents are also updated, along with the 
    /// new selection and the textbox.
    /// </summary>
    /// <param name="grid"></param>
    private void DisplaySelection(SpreadsheetGrid grid)
    {
        this.grid = grid;

        // Update the spreadsheet with the previous cell's change (if any).
        if (prevCell != null)
            UpdatePrevCellAndDependents(textBox.Text, prevCell, textBox.Text);

        spreadsheetGrid.GetClickedCell(out int col, out int row);
        spreadsheetGrid.GetValue(col, row, out string value);

        // Set the textbox text to the string cell value (if it's a formula, '=' is added).
        if (spreadsheet.GetCellContents(SpreadsheetGrid.GetCellName(col, row)) is Formula formula)
            textBox.Text = "=" + formula.ToString();
        else
            textBox.Text = spreadsheet.GetCellContents(SpreadsheetGrid.GetCellName(col, row)).ToString();

        // When a grid cell is first clicked on, the cell value is displayed instead of the contents.
        spreadsheetGrid.SetValue(col, row, value);

        // Display the cell's name above the grid. If the name is
        // greater than 14 characters, it is displayed in a shortened form. 
        if (value.Length > 14)
            cellValueDisplay.Text = "Cell Value: " + value[0..10] + "...";
        else
            cellValueDisplay.Text = "Cell Value: " + value;

        // Display the cell's value above the grid. If it is a multi-selection,
        // then the dimensions of the cell are displayed. 
        List<string>[] selection = grid.GetSelection();
        if (selection.Length == 1 && selection[0].Count == 1)
            cellNameDisplay.Text = selection[0].First();
        else
            cellNameDisplay.Text = selection[0].Count + "R x " + selection.Length + "C";

        // Save information about this selected cell as the 'previous cell.'
        prevCell = SpreadsheetGrid.GetCellName(col, row);
    }


    /// <summary>
    /// Updates the displayed value of the previous cell (the cell that was just interacted with), along
    /// with its dependent cells. If any exception is caught while updating the dependencies, an alert is
    /// displayed and the changes are reverted. 
    /// </summary>
    private void UpdatePrevCellAndDependents(string text, string prevCell, string input)
    {
        try
        {
            // Update the cell value and all of it's dependents' values.
            IList<string> dependentCells = spreadsheet.SetContentsOfCell(prevCell, input);
            foreach (string cell in dependentCells)
            {
                string valString = spreadsheet.GetCellValue(cell).ToString();
                // Any formula errors are displayed as Err: #DIV/0! or Err: Undefined. 
                if (spreadsheet.GetCellValue(cell) is FormulaError err)
                    valString = err.Reason;
                // Converts the cell name into column/row integers to set the cell value.
                spreadsheetGrid.SetValue(cell[0] - 65, Int32.Parse(cell[1..]) - 1, valString);
            }

            // When sorting, the textbox must also change.
            textBox.Text = text;
        }
        // If any sort of exception is thrown when providing input, a pop-up appears to notify of the invalid
        // input and the changes are reverted.
        catch (Exception ex)
        {
            string message = ex.Message;
            if (ex is CircularException)
                message = "A cell cannot depend on itself!";
            else if (ex is InvalidNameException)
                message = "Invalid cell name.";

            DisplayAlert("INVALID INPUT: \"" + input + "\" IN CELL " + prevCell + ".", message, "OK");
            spreadsheetGrid.SetValue(prevCell[0] - 65, Int32.Parse(prevCell[1..]) - 1, "");
        }
    }


    /// <summary>
    /// Creates a new empty spreadsheet object.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void NewClicked(Object sender, EventArgs e)
    {
        // The file is saved if there was a change in the spreadsheet and the user inputs "Yes."
        if (spreadsheet.Changed && await DisplayAlert("Save new file changes?", "", "Yes", "No"))
            SaveClicked(sender, e);

        ResetSpreadsheet("");
        spreadsheetGrid.SetSelection(0, 0);
    }


    /// <summary>
    /// If there are any unsaved changes, the user is prompted to save before the file is opened.
    /// Then, the user is able to choose a file, the previous spreadsheet data is cleared, and
    /// the spreadsheet is filled with the data from the opened spreadsheet file. If there are any
    /// issues reading/writing with the filepath, an alert pops up. 
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        // Prompt the user to save the file. 
        if (spreadsheet.Changed && await DisplayAlert("Save new file changes?", "", "Yes", "No"))
            SaveClicked(sender, e);

        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                ResetSpreadsheet(fileResult.FullPath);

                // For every cell in the new spreadsheet, add its value to the spreadsheetGUI.
                foreach (string cell in spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    string valString = spreadsheet.GetCellValue(cell).ToString();
                    if (spreadsheet.GetCellValue(cell) is FormulaError err)
                        valString = err.Reason;
                    spreadsheetGrid.SetValue(cell[0] - 65, Int32.Parse(cell[1..]) - 1, valString);
                }
                spreadsheetGrid.SetSelection(0, 0);
            }
            else
            {
                await DisplayAlert("No file selected.", "", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error opening file: ", ex.Message, "OK");
        }
    }


    /// <summary>
    /// Resets the current spreadsheet by disabling the AutoSave feature, creating a new spreadsheet 
    /// with a given filepath, resetting the prevCell to null, and clearing the spreadsheetGrid. 
    /// </summary>
    /// <param name="filepath"></param>
    private void ResetSpreadsheet(string filepath)
    {
        toggleAutoSave.IsToggled = false;
        autoSave.Text = "Not saved";

        spreadsheet = new Spreadsheet(filepath, s => Regex.IsMatch(s, @"^[A-Z]([1-9]|[1-9][0-9])$"),
            s => s.ToUpper(), "ps6");

        prevCell = null;
        this.filepath = filepath;
        spreadsheetGrid.Clear();
    }


    /// <summary>
    /// If a filepath is provided, saves the spreadsheet to that file without prompting the user. Otherwise,
    /// the user is prompted to enter in a filepath. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SaveClicked(object sender, EventArgs e)
    {
        if (filepath == "")
            FileSaveAs();
        else
            FileSave();
    }


    /// <summary>
    /// Tries to save the file using the spreadsheet filepath. If any error occurs when saving the file,
    /// a display alert appears. 
    /// </summary>
    private async void FileSave()
    {
        try
        {
            spreadsheet.Save(filepath);
            await DisplayAlert("Spreadsheet was successfully saved.", "", "OK");
            autoSave.Text = "Last saved: " + DateTime.Now.ToShortTimeString();

            // Enable the timer if the file is saved and AutoSave is toggled. 
            if (autoSaveOn && !myTimer.Enabled)
                myTimer.Enabled = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error saving file: ", ex.Message, "OK");
        }
    }


    /// <summary>
    /// Calls the FileSaveAs method to save the file with a filepath. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SaveAsClicked(object sender, EventArgs e)
    {
        FileSaveAs();
    }


    /// <summary>
    /// Prompts the user to enter in a filepath that is used to save the file. 
    /// The default filepath in the prompt is the original filepath if it exists. 
    /// Otherwise, it is a random, modifyable filepath. 
    /// </summary>
    private async void FileSaveAs()
    {
        string defaultPath = "C:\\Users\\Josie\\source\\repos\\spreadsheet-JosieFiedel\\Spreadsheet" +
            "\\FileSaves\\TestFile.sprd";

        // Sets the default path to be the original filepath if it exists.
        if (filepath != "")
            defaultPath = filepath;

        // Prompts the user to save using a specific provided modifyable filepath. 
        string filename = await DisplayPromptAsync("File Save:", "", "OK", "Cancel",
            placeholder: "Enter filepath here: ", initialValue: defaultPath);

        // If the filename is null, it means that the user clicked 'cancel,' so the file is not to be saved.
        // If the filepath already exists, it is not the same filepath as the initial spreadsheet filepath,
        // and the user doesn't want overwrite the file, then the file is not saved and AutoSave is turned off. 
        if (filename == null || (File.Exists(filename) && filename != filepath && 
            !await DisplayAlert("File already exists.", "Overwrite this file?", "OK", "Cancel")))
        {
            autoSaveOn = false;
            toggleAutoSave.IsToggled = false;
            return;
        }

        // Otherwise, the file is then saved.
        filepath = filename;
        FileSave();
    }


    /// <summary>
    /// An automatic saving function that acts when the AutoSave slider is toggled. If it is toggled on, then the file is 
    /// saved and a timer is started, which automatically saves the spreadsheet every 2 minutes. When it is toggled off, 
    /// the AutoSave is disabled and saving stops. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToggleAutoSave(object sender, ToggledEventArgs e)
    {
        if (toggleAutoSave.IsToggled)
        {
            autoSaveOn = true;
            if (filepath == "")
                FileSaveAs();
            else
                FileSave();

            myTimer = new System.Timers.Timer(120000);
            myTimer.Elapsed += AutoSaveEvent;
            myTimer.AutoReset = true;
        }
        else
        {
            autoSaveOn = false;
            if (myTimer != null)
                myTimer.Enabled = false;
        }
    }


    /// <summary>
    /// Part of the AutoSave functionality; the file is saved and the save time is updated. If any error
    /// occurs in saving the spreadsheet, the AutoSave is toggled off and an error message is displayed. 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private async void AutoSaveEvent(Object source, ElapsedEventArgs e)
    {
        try
        {
            spreadsheet.Save(filepath);
            Dispatcher.Dispatch(() => autoSave.Text = "Last saved: " + DateTime.Now.ToShortTimeString());
        }
        catch (Exception ex)
        {
            toggleAutoSave.IsToggled = false;
            await DisplayAlert("Error saving file: ", ex.Message, "OK");
        }
    }


    /// <summary>
    /// When a user clicks on the 'Sort Ascending,' the columns of the selection
    /// are sorted in ascending order. (i.e. numbers come before strings containing letters).
    /// An error occurs if a FormulaError cell is contained in the selection to be sorted.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void InOrderSort(object sender, EventArgs e)
    {
        try
        {
            grid.GetClickedCell(out int col, out int row);
            string text = spreadsheet.InOrderSort(SpreadsheetGrid.GetCellName(col, row), grid.GetSelection());

            // Change the textbox to reflect the new cell value.
            textBox.Text = text;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error saving file: ", ex.Message, "OK");
        }
    }


    /// <summary>
    /// When a user clicks on the 'Sort Descending,' the columns of the selection 
    /// are sorted in descending order (i.e. numbers come after strings containing letters).
    /// An error occurs if a FormulaError cell is contained in the selection to be sorted.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ReverseSort(object sender, EventArgs e)
    {
        try
        {
            grid.GetClickedCell(out int col, out int row);
            string text = spreadsheet.ReverseSort(SpreadsheetGrid.GetCellName(col, row), grid.GetSelection());

            // Change the textbox to reflect the new cell value.
            textBox.Text = text;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error saving file: ", ex.Message, "OK");
        }
    }


    /// <summary>
    /// Enabled when a user clicks on the 'Cell Input' menu button on the spreadsheet. 
    /// Information about entering cell input.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CellInput(object sender, EventArgs e)
    {
        DisplayAlert("Entering Cell Input:", "Click on any cell (A1-Z99) and select the " +
            "textbox to enter cell input. Click away or hit the return button to select a different cell!", "OK");
    }


    /// <summary>
    /// Enabled when a user clicks on the 'File Saves' menu button on the spreadsheet. Information about saving files.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FileSaves(object sender, EventArgs e)
    {
        DisplayAlert("Saving a File:", "Saving a spreadsheet file is super easy! If a filepath is already " +
            "tied to the file (because it's an opened/already saved file), the 'Save' button in the menu bar " +
            "simply saves the file. If a filepath is not given (because it's a new file), you are prompted to " +
            "enter in a filepath. The 'SaveAs' button allows you to select a filepath.", "OK");
    }


    /// <summary>
    /// Enabled when a user clicks on the 'AutoSave' menu button on the spreadsheet. Information about AutoSave.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AutoSaveFeature(object sender, EventArgs e)
    {
        DisplayAlert("Using AutoSave:", "The AutoSave slider on the right of the menu bar allows for the file to be" +
            " automatically saved every 2 minutes. Just hit the button, enter in a filepath, and every two minutes " +
            "the 'Last saved' text will update to the last saved time!", "OK");
    }


    /// <summary>
    /// Enabled when a user clicks on the 'Sort' menu button on the spreadsheet. Information about sorting selections.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Sorting(object sender, EventArgs e)
    {
        DisplayAlert("Sorting a Selection:", "To create and sort a cell selection, start by selecting a region of " +
            "cells by clicking and dragging your mouse. Then, click on the \"Sort\" button to select one of the " +
            "two possible sorts: ascending order or descending order.", "OK");
    }
}