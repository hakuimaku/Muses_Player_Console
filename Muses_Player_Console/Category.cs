namespace Muses_Player_Console;

public class Category
{
    string CategoryName { get; set; }
    string CategoryID { get; set; }
    
    const int CategoryIDWidth = 15;
    const int CategoryNameWidth = 30;
    
    public Category()
    {
        // Default constructor
    }
    public Category(string categoryID, string categoryName)
    {
        CategoryID = categoryID;
        CategoryName = categoryName;
    }

    public void PrintCategory()
    {
        Console.WriteLine(
            $"{ConsoleTableFormatter.PadRightDisplay(CategoryID, CategoryIDWidth)}" +
            $"{ConsoleTableFormatter.PadRightDisplay(CategoryName, CategoryNameWidth)}"
        );
    }

    public void PrintHeader()
    {
        Console.WriteLine(
            $"{ConsoleTableFormatter.PadRightDisplay("CategoryID", CategoryIDWidth)}" +
            $"{ConsoleTableFormatter.PadRightDisplay("CategoryName", CategoryNameWidth)}"
        );
        
        Console.WriteLine(new string('-', CategoryIDWidth + CategoryNameWidth));
    }
}