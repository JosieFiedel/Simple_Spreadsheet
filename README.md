**[CS 2420 Fall 2022] Simple Spreadsheet**
-------------------------------------------

	Simple Spreadsheet is functionally and stylistically similar to the Microsoft Excel 
	computer program. Built using C# and .NET Maui, it allows data to be stored in visual 'cell'
	boxes. Simple equations involving arithmetic operations may be computed, with equation
	variables representing other cell boxes within the spreadsheet. Spreadsheet files may
	be saved, new files may be created, and .sprd files may be opened. AutoSave allows for
	a spreadsheet to be automatically saved every 2 minutes. 
 
  This project was made in my CS 2420 class to practice C#, GUI, and the Visual Studio IDE. A screenshot 
  of the spreadsheet in action is below. 

<img width="688" alt="image" src="https://github.com/JosieFiedel/Simple_Spreadsheet/assets/112005344/c788e375-e22e-427a-89f0-b71376c7f205">


**== How to Use ==**
	
	When the application begins execution, the user is greeted with the spreadsheet cell 
	grid, along with several additional feature buttons in the menu bar. There are 99 rows
	and 26 columns, representing cells A1-Z99. The menu bar contains a label that represents
	the selected cell, a label that represents the selected cell value, and a textbox to
	input cell data. To input data into the spreadsheet, a user must select one of these cells 
	and click on the text box in the menu bar above to input the cell content. Once the user 
	clicks away or returns from that cell, the cell display changes to represent the value of 
	the cell instead of the inputted content. 

	In the menu bar above, there are three main menu bar items: File, Help, and Sort. 
	
	File contains the following sub-items: New, Open, Save, and SaveAs. Pressing 'New' creates 
	a new spreadsheet that doesn't contain any cell values. Pressing 'Open' allows the user to 
	select a .sprd file to be loaded to the spreadsheet display. Pressing 'Save' saves the
	file if a filepath is already provided; otherwise, it prompts the user to enter in a
	filepath to save the spreadsheet data. Pressing 'SaveAs' prompts the user to enter in a
	filepath to save the spreadsheet data.

	Help contains the following sub-items: Cell Input, File Saves, and AutoSave, which a user can 
	access to learn about the specifics of the spreadsheet functionality.

	Sort contains the following sub-items: Sort Ascending and Sort Descending. Pressing on either
	of these will sort the user's selection in ascending or descending order. To create a larger
	selection, the user may click and drag their mouse around the desired cells. 



**== Project Notes ==**

	1. Cell Text Input

		As stated above, the implementation of Simple Spreadsheet is similar to the functionality and
		design of the Excel application. In Excel, when a user clicks on a cell, they are able to 
		immediately enter in some input string. However, because of object hierarchy in the Simple
		Spreadsheet view, MAUI's solution of this behavior is somewhat bugged. So instead, the user
		must select a cell and then select the textbox to input cell contents. 

	2. Selecting Cells

		Cells may be selected in two different ways: by clicking or by hitting the return key. When
		both actions occur, the cell display is changed from the inputted contents to the calculated
		cell value. Any other cells that depend on this cell's value are also visually changed. 

	3. File Save Prompts

		When attempting to open or create a new spreadsheet file when changes have been made, the user 
		is alerted to save their changes in two ways. If a filepath is already provided (by a previous
		save or by editing a previously-opened file), the file is saved to this same filepath and the
		user is not prompted to input a new filepath. If a filepath is not already provided (by modifying
		a new spreadsheet), the user is prompted to input a new filepath. If a user wants to save the 
		file to a different filepath, they should click the 'SaveAs' button under File. 

	4. SaveAs (*Extra Feature)

		Since there is not currently an efficient way in .NET MAUI to prompt a user to select where a file
		should be saved, when selecting 'SaveAs,' the user is prompted to save to a default filepath, which
		may be modifiable. If a filepath is already tied to the current spreadsheet file, this default
		filepath becomes that filepath. 

	5. AutoSave (*Extra Feature)
		
		An AutoSave has been implemented into the spreadsheet as an extra feature. To toggle AutoSave, there
		is a slider in the menu bar that may be enabled or disabled. When enabling AutoSave, the user is
		prompted to enter in a filepath to initially save the file to. Then, the file is saved every 2 minutes
		using this same filepath. When disabling AutoSave, the automatic saving process stops and no more saving
		is done.

	6. Errors

		In Simple Spreadsheet, there are two types of errors: FormulaErrors and Exception errors. 

		FormulaErrors occur when a user inputs an undefined expression. There are two types of FormulaErrors 
		that may occur within a cell: 
			a. Err: #DIV/0! represents an expression that attempts to divide by zero (Ex: =1/0).
			b. Err: Undefined represents an undefined variable or a reference to a cell that contains the
			   divide by zero error. 

		Exception errors occur when an illegal action occurs. Exception errors may be due to problems with
		reading and accessing files, writing to files, or circular exceptions inside of the spreadsheet
		cells. A circular exception occurs when a spreadsheet cell depends on it's own value (Ex: Setting 
		the contents of A1 to be =A1).

	7. Sorting (*Extra Feature)

		The addition of multiple cell select has been implemented to handle cell selection sorting. A user must
		click and drag their mouse around the desired cell data to be sorted. Then, they must click one of the two
		sorting options: Sort Ascending or Sort Descending. These options are found in the "Sort" button near the
		top of the spreadsheet. Sort Ascending sorts the given cell values in order, with numbers occurring before
		values containing letters. Sort Descending orders similarly to Sort Ascending, but in the reverse order.
		FormulaErrors may not be sorted and an error prompt occurs if a sorting attempt is made for a FormulaError.



**== Project Versioning History ==**

	[Version PS1--September 2, 2022]

		Completion of Evaluator class, which evaluates a given string arithmetic integer infix 
		expression. A basic lookup function is utilized to return a value, given an 
		appropriate variable (letters followed by numbers). This basic functionality is modified
		in future versions to suit spreadsheet formula arithmetic operations. 

	[Version PS2--September 9, 2022]
		
		Completion of DependencyGraph class, which takes ordered pairs and 'connects' them according
		to their depencencies, creating an implicit mapping between values. When adding a dependency,
		the two values in the ordered pair are added to two dictionaries, dependents and dependees, to
		represent the value relations. The dependency functionality is utilized in future versions to
		modify cells according to their dependencies. 

	[Version PS3--September 16, 2022]

		Completion of Formula class, which constructs a formula when provided a string, a normalizer,
		and a validator. The Formula class uses the same functionality as the Evaluator class to
		evaluate a given string arithmetic integer infix expression. If valid, the variables are 
		limited to being an underscore/letter followed by underscore(s), letter(s), and number(s), as
		well as passing the additional restrictions imposed by the validity function. The Formula 
		functionality serves as the basis for user-inputted expression computations in the final 
		spreadsheet implementation. 

	[Version PS4--September 23, 2022]

		Partial completion of Spreadsheet class, which constructs an implicit spreadsheet, containing
		cells and dependencies between cells. This class utilizes the Formula class to compute provided
		formulas and the DependencyGraph class to keep track of cell relations. Along with keeping
		track of cell relations, additional checks are placed, including a check for circular 
		dependencies between cell dependencies. Cell objects are now used to hold cell contents set by 
		an outside source. This basic Spreadsheet implementation allows for cells to be added/modified/
		removed from the spreadsheet, updating dependencies when any cell content is changed.

	[Version PS5--September 30, 2022]

		Completion of Spreadsheet class. Updates to this class include a Save method, which converts 
		the spreadsheet to a string version, which may be 'loaded' when creating a new spreadsheet 
		object. Cell content inputs must be in string form, and cell values are created from the 
		cell contents. The value, as well as the contents, from any given cell in the spreadsheet may
		be retrieved and modified. 

	[Version PS6--October 21, 2022]

		Completion of the Spreadsheet GUI, which graphically represents an abstract spreadsheet. A user
		may provide an input, which is displayed in a selected cell (A1-Z99) in the spreadsheet. If this
		input is an expression, the appropriate computations are made and the value of the expression is
		displayed once the selection changes. With the click of a button, a new spreadsheet can be
		constructed using 'New,' a spreadsheet may be saved using 'Save' or 'SaveAs', and a previously-
		saved spreadsheet may be opened using 'Open.' 

	[Version PS7--December 28,2022]

		New features implemented, including mouse multi-selection and cell selection sorting.
