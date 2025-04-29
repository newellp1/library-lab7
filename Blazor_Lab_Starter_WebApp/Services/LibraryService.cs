using Blazor_Lab_Starter_WebApp.Models;

namespace Blazor_Lab_Starter_WebApp.Services;

public class LibraryService : ILibraryService
{
    private readonly string booksFile = "Data/Books.csv";
    private readonly string usersFile = "Data/Users.csv";

    public List<Book> Books { get; private set; } = new();
    public List<User> Users { get; private set; } = new();
    public Dictionary<int, List<int>> BorrowedBooks { get; private set; } = new();

    public LibraryService()
    {
        ReadBooks();
        ReadUsers();
    }

    // ─────────────── BOOK METHODS ───────────────

    public void ReadBooks()
    {
        if (File.Exists(booksFile))
        {
            Books = File.ReadAllLines(booksFile)
                .Skip(1)
                .Select(line => line.Split(','))
                .Where(parts => parts.Length >= 5 && int.TryParse(parts[0], out _))
                .Select(parts => new Book
                {
                    Id = int.Parse(parts[0]),
                    Title = parts[1],
                    Author = parts[2],
                    IsBorrowed = bool.TryParse(parts[3], out bool b) && b,
                    ISBN = parts[4]
                })
                .ToList();
        }
    }

    public void SaveBooks() =>
        File.WriteAllLines(booksFile, new[] { "Id,Title,Author,IsBorrowed,ISBN" }
            .Concat(Books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.IsBorrowed},{b.ISBN}")));

    public void AddBook(Book book)
    {
        book.Id = Books.Any() ? Books.Max(b => b.Id) + 1 : 1;
        Books.Add(book);
        SaveBooks();
    }

    public void EditBook(Book book)
    {
        var index = Books.FindIndex(b => b.Id == book.Id);
        if (index >= 0)
        {
            Books[index] = book;
            SaveBooks();
        }
    }

    public void DeleteBook(int id)
    {
        var book = Books.FirstOrDefault(b => b.Id == id);

        // Prevent deletion if book is currently borrowed
        if (book != null && book.IsBorrowed)
        {
            throw new InvalidOperationException("Cannot delete a book that is currently borrowed.");
        }

        if (book != null)
        {
            Books.Remove(book);
            SaveBooks();
        }
    }

    // ─────────────── USER METHODS ───────────────

    public void ReadUsers()
    {
        if (File.Exists(usersFile))
        {
            Users = File.ReadAllLines(usersFile)
                .Skip(1)
                .Select(line => line.Split(','))
                .Where(parts => parts.Length >= 3)
                .Select(parts => new User
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1],
                    Email = parts[2]
                })
                .ToList();
        }
    }

    public void SaveUsers() =>
        File.WriteAllLines(usersFile, new[] { "Id,Name,Email" }
            .Concat(Users.Select(u => $"{u.Id},{u.Name},{u.Email}")));

    public void AddUser(User user)
    {
        user.Id = Users.Any() ? Users.Max(u => u.Id) + 1 : 1;
        Users.Add(user);
        SaveUsers();
    }

    public void EditUser(User user)
    {
        var index = Users.FindIndex(u => u.Id == user.Id);
        if (index >= 0)
        {
            Users[index] = user;
            SaveUsers();
        }
    }

    public void DeleteUser(int id)
    {
        // Prevent deletion if user has borrowed books
        if (BorrowedBooks.ContainsKey(id) && BorrowedBooks[id].Count > 0)
        {
            throw new InvalidOperationException("Cannot delete user who has borrowed books.");
        }

        var user = Users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            Users.Remove(user);
            SaveUsers();
        }
    }

    // ─────────────── BORROWING METHODS ───────────────

    public void BorrowBook(int userId, int bookId)
    {
        if (!BorrowedBooks.ContainsKey(userId))
            BorrowedBooks[userId] = new List<int>();

        if (!BorrowedBooks[userId].Contains(bookId))
            BorrowedBooks[userId].Add(bookId);

        var book = Books.FirstOrDefault(b => b.Id == bookId);
        if (book != null)
        {
            book.IsBorrowed = true;
            EditBook(book);
        }
    }

    public void ReturnBook(int userId, int bookId)
    {
        if (BorrowedBooks.TryGetValue(userId, out var borrowedList) && borrowedList.Contains(bookId))
        {
            borrowedList.Remove(bookId);
            if (borrowedList.Count == 0)
                BorrowedBooks.Remove(userId);
        }

        var book = Books.FirstOrDefault(b => b.Id == bookId);
        if (book != null && book.IsBorrowed)
        {
            book.IsBorrowed = false;
            EditBook(book);
        }
    }

    public List<Book> GetBorrowedBooksByUser(int userId)
    {
        return BorrowedBooks.TryGetValue(userId, out var ids)
            ? ids.Select(id => Books.FirstOrDefault(b => b.Id == id))
                 .Where(b => b is not null)
                 .ToList()!
            : new List<Book>();
    }

    public List<(Book Book, User User)> GetAllBorrowedBooksWithUsers()
    {
        var result = new List<(Book, User)>();
        foreach (var kvp in BorrowedBooks)
        {
            var user = Users.FirstOrDefault(u => u.Id == kvp.Key);
            foreach (var bookId in kvp.Value)
            {
                var book = Books.FirstOrDefault(b => b.Id == bookId);
                if (user != null && book != null)
                {
                    result.Add((book, user));
                }
            }
        }
        return result;
    }
}