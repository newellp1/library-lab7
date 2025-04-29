using Blazor_Lab_Starter_WebApp.Models;

namespace Blazor_Lab_Starter_WebApp.Services;

public interface ILibraryService
{
    // Book methods
    void ReadBooks();
    void SaveBooks();
    void AddBook(Book book);
    void EditBook(Book book);
    void DeleteBook(int id);

    // User methods
    void ReadUsers();
    void SaveUsers();
    void AddUser(User user);
    void EditUser(User user);
    void DeleteUser(int id);

    // Borrowing methods
    void BorrowBook(int userId, int bookId);
    void ReturnBook(int userId, int bookId);
    List<Book> GetBorrowedBooksByUser(int userId);
    List<(Book Book, User User)> GetAllBorrowedBooksWithUsers();

    // Access lists
    List<Book> Books { get; }
    List<User> Users { get; }
}