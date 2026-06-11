namespace Muses_Player_Console;

public class Category
{
    public string? CategoryName { get; set; }
    public string? CategoryId { get; set; }
    
    
    public Category()
    {
        // Default constructor
    }
    public Category(string categoryId, string categoryName)
    {
        CategoryId = categoryId;
        CategoryName = categoryName;
    }
}