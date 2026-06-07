namespace Muses_Player_Console;

public class Category
{
    public string CategoryName { get; set; }
    public string CategoryID { get; set; }
    
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
}