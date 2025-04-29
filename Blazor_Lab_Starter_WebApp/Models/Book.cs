namespace Blazor_Lab_Starter_WebApp.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public bool IsBorrowed { get; set; }
    public string ISBN { get; set; } = "";
}