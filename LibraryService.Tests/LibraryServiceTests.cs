using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blazor_Lab_Starter_WebApp.Models;
using Blazor_Lab_Starter_WebApp.Services;
using System.Collections.Generic;
using System.Linq;

namespace LibraryService.Tests
{
    [TestClass]
    public class LibraryServiceTests
    {
        private ILibraryService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _service = new LibraryServiceMock();
        }

        // ─────────────────────── Book Tests ───────────────────────

        [TestMethod]
        public void AddBook_ShouldIncreaseCount()
        {
            var book = new Book { Title = "Book 1", Author = "Author A", ISBN = "123456" };
            _service.AddBook(book);

            Assert.AreEqual(999, _service.Books.Count);
        }

        [TestMethod]
        public void EditBook_ShouldUpdateTitle()
        {
            var book = new Book { Title = "Old", Author = "Author", ISBN = "0000" };
            _service.AddBook(book);
            var saved = _service.Books.First();
            saved.Title = "New";
            _service.EditBook(saved);

            Assert.AreEqual("New", _service.Books.First().Title);
        }

        [TestMethod]
        public void DeleteBook_WhenNotBorrowed_ShouldSucceed()
        {
            var book = new Book { Title = "To Delete", Author = "Author", ISBN = "999" };
            _service.AddBook(book);
            int id = _service.Books.First().Id;

            _service.DeleteBook(id);

            Assert.AreEqual(0, _service.Books.Count);
        }

        [TestMethod]
        public void DeleteBook_WhenBorrowed_ShouldThrow()
        {
            var book = new Book { Title = "Cannot Delete", Author = "Author", ISBN = "111" };
            var user = new User { Name = "Reader", Email = "reader@example.com" };
            _service.AddBook(book);
            _service.AddUser(user);

            _service.BorrowBook(user.Id, book.Id);

            Assert.ThrowsException<InvalidOperationException>(() => _service.DeleteBook(book.Id));
        }

        // ─────────────────────── User Tests ───────────────────────

        [TestMethod]
        public void AddUser_ShouldIncreaseCount()
        {
            var user = new User { Name = "User A", Email = "a@example.com" };
            _service.AddUser(user);

            Assert.AreEqual(1, _service.Users.Count);
        }

        [TestMethod]
        public void EditUser_ShouldUpdateEmail()
        {
            var user = new User { Name = "User B", Email = "b@example.com" };
            _service.AddUser(user);
            var saved = _service.Users.First();
            saved.Email = "updated@example.com";
            _service.EditUser(saved);

            Assert.AreEqual("updated@example.com", _service.Users.First().Email);
        }

        [TestMethod]
        public void DeleteUser_WhenHasNoBorrowedBooks_ShouldSucceed()
        {
            var user = new User { Name = "User C", Email = "c@example.com" };
            _service.AddUser(user);
            int id = _service.Users.First().Id;

            _service.DeleteUser(id);

            Assert.AreEqual(0, _service.Users.Count);
        }

        [TestMethod]
        public void DeleteUser_WhenHasBorrowedBooks_ShouldThrow()
        {
            var book = new Book { Title = "Borrowed", Author = "Author", ISBN = "333" };
            var user = new User { Name = "Blocked", Email = "blocked@example.com" };
            _service.AddBook(book);
            _service.AddUser(user);

            _service.BorrowBook(user.Id, book.Id);

            Assert.ThrowsException<InvalidOperationException>(() => _service.DeleteUser(user.Id));
        }

        // ─────────────────────── Borrowing Tests ───────────────────────

        [TestMethod]
        public void BorrowBook_ShouldMarkBookAsBorrowed()
        {
            var book = new Book { Title = "Borrowed", Author = "Author", ISBN = "444" };
            var user = new User { Name = "Borrower", Email = "b@example.com" };

            _service.AddBook(book);
            _service.AddUser(user);

            _service.BorrowBook(user.Id, book.Id);

            Assert.IsTrue(_service.Books.First().IsBorrowed);
        }

        [TestMethod]
        public void ReturnBook_ShouldMarkBookAsNotBorrowed()
        {
            var book = new Book { Title = "Return Me", Author = "Author", ISBN = "555" };
            var user = new User { Name = "Returner", Email = "r@example.com" };

            _service.AddBook(book);
            _service.AddUser(user);
            _service.BorrowBook(user.Id, book.Id);

            _service.ReturnBook(user.Id, book.Id);

            Assert.IsFalse(_service.Books.First().IsBorrowed);
        }

        [TestMethod]
        public void GetAllBorrowedBooksWithUsers_ShouldReturnCorrectMapping()
        {
            var book = new Book { Title = "Mapped Book", Author = "Author", ISBN = "777" };
            var user = new User { Name = "Mapper", Email = "map@example.com" };

            _service.AddBook(book);
            _service.AddUser(user);
            _service.BorrowBook(user.Id, book.Id);

            var result = _service.GetAllBorrowedBooksWithUsers();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Mapped Book", result[0].Book.Title);
            Assert.AreEqual("Mapper", result[0].User.Name);
        }

        [TestMethod]
        public void BorrowBook_OnlyAffectsMockedData()
        {
            var book = new Book { Title = "Mock", Author = "Author", ISBN = "888" };
            var user = new User { Name = "MockUser", Email = "mock@example.com" };

            _service.AddBook(book);
            _service.AddUser(user);
            _service.BorrowBook(user.Id, book.Id);

            Assert.IsTrue(_service.Books.First().IsBorrowed);
        }
    }

    // ─────────────────────── MOCK SERVICE ───────────────────────

    public class LibraryServiceMock : ILibraryService
    {
        public List<Book> Books { get; private set; } = new();
        public List<User> Users { get; private set; } = new();
        public Dictionary<int, List<int>> BorrowedBooks { get; private set; } = new();

        public void AddBook(Book book)
        {
            book.Id = Books.Any() ? Books.Max(b => b.Id) + 1 : 1;
            Books.Add(book);
        }

        public void EditBook(Book book)
        {
            var index = Books.FindIndex(b => b.Id == book.Id);
            if (index >= 0) Books[index] = book;
        }

        public void DeleteBook(int id)
        {
            var book = Books.FirstOrDefault(b => b.Id == id);

            if (book != null && book.IsBorrowed)
                throw new InvalidOperationException("Cannot delete a book that is currently borrowed.");

            if (book != null) Books.Remove(book);
        }

        public void AddUser(User user)
        {
            user.Id = Users.Any() ? Users.Max(u => u.Id) + 1 : 1;
            Users.Add(user);
        }

        public void EditUser(User user)
        {
            var index = Users.FindIndex(u => u.Id == user.Id);
            if (index >= 0) Users[index] = user;
        }

        public void DeleteUser(int id)
        {
            if (BorrowedBooks.ContainsKey(id) && BorrowedBooks[id].Count > 0)
                throw new InvalidOperationException("Cannot delete user who has borrowed books.");

            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user != null) Users.Remove(user);
        }

        public void BorrowBook(int userId, int bookId)
        {
            if (!BorrowedBooks.ContainsKey(userId))
                BorrowedBooks[userId] = new List<int>();

            if (!BorrowedBooks[userId].Contains(bookId))
                BorrowedBooks[userId].Add(bookId);

            var book = Books.FirstOrDefault(b => b.Id == bookId);
            if (book != null) book.IsBorrowed = true;
        }

        public void ReturnBook(int userId, int bookId)
        {
            if (BorrowedBooks.TryGetValue(userId, out var borrowedList))
                borrowedList.Remove(bookId);

            var book = Books.FirstOrDefault(b => b.Id == bookId);
            if (book != null) book.IsBorrowed = false;
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
            var results = new List<(Book, User)>();
            foreach (var entry in BorrowedBooks)
            {
                var user = Users.FirstOrDefault(u => u.Id == entry.Key);
                foreach (var bookId in entry.Value)
                {
                    var book = Books.FirstOrDefault(b => b.Id == bookId);
                    if (user != null && book != null)
                        results.Add((book, user));
                }
            }
            return results;
        }

        public void ReadBooks() { }
        public void ReadUsers() { }
        public void SaveBooks() { }
        public void SaveUsers() { }
    }
}