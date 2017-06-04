using Dapper;
using LibraryApplicationAPI.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>

namespace LibraryApplicationAPI.Repository
{
    public class PatronRepository : IPatronRepository<Patron>
    {
        private string connectionString;
        public PatronRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetValue<string>("DBInfo:ConnectionString");
        }

        internal IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(connectionString);
            }
        }

        /// <summary>
        /// Add a patron
        /// </summary>
        /// <param name="patron"></param>
        /// <returns></returns>
        public Patron Add(Patron patron)
        {
            Patron AddedPatron;
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("insert into patrons(fname, lname, email)  VALUES (@fname, @lname, @email)", new { fname = patron.fname, lname = patron.lname, email = patron.email });
                var PatronList = FindAll();
                AddedPatron = PatronList.Last();
                dbConnection.Close();
            }
            return AddedPatron;
        }

        /// <summary>
        /// Get all patrons
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Patron> FindAll()
        {
            var sql = "SELECT * FROM patrons";
            List<Patron> patronList = Connection.Query<Patron>(sql).ToList();
            foreach(Patron patron in patronList)
            {
                var getBooks = "select * from books where patronid = " + patron.patronid;
                List<Book> bookList = Connection.Query<Book>(getBooks).ToList();
                foreach (Book book in bookList)
                {
                    var getBookAuthors = "select authorid, bookid from bookauthor where bookid = " + book.bookid;
                    List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                    List<BookAuthor> bookauthors = new List<BookAuthor>();
                    foreach (BookAuthor bookAuthor in bookAuthorsList)
                    {
                        var getAuthorID = bookAuthor.authorid;
                        var getAuthorDetails = "select * from authors where authorid=" + getAuthorID;
                        List<Author> authorsList = Connection.Query<Author>(getAuthorDetails).ToList();
                        foreach (Author author in authorsList)
                        {
                            bookAuthor.authorid = author.authorid;
                            bookAuthor.Author = author;
                        }
                        bookauthors.Add(bookAuthor);
                    }
                    book.bookauthors = bookauthors;
                    Library library = Connection.Query<Library>("SELECT * FROM libraries WHERE libraryid = @libraryid", new { libraryid = book.libraryid }).FirstOrDefault();
                    book.librarydetails = library;

                    Connection.Close();
                }
                patron.books = bookList;
            }
            return patronList;
        }

        /// <summary>
        /// Get patron by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Patron FindByID(int id)
        {
            Patron patron;
            IDbConnection dbConnection = Connection;
            dbConnection.Open();
            patron = dbConnection.Query<Patron>("SELECT * FROM patrons WHERE patronid = @patronid", new { patronid = id }).FirstOrDefault();
            if (patron != null)
            {
                var getBooks = "select * from books where patronid = " + patron.patronid;
                List<Book> bookList = Connection.Query<Book>(getBooks).ToList();
                foreach (Book book in bookList)
                {
                    var getBookAuthors = "select authorid, bookid from bookauthor where bookid = " + book.bookid;
                    List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                    List<BookAuthor> bookauthors = new List<BookAuthor>();
                    foreach (BookAuthor bookAuthor in bookAuthorsList)
                    {
                        var getAuthorID = bookAuthor.authorid;
                        var getAuthorDetails = "select * from authors where authorid=" + getAuthorID;
                        List<Author> authorsList = Connection.Query<Author>(getAuthorDetails).ToList();
                        foreach (Author author in authorsList)
                        {
                            bookAuthor.authorid = author.authorid;
                            bookAuthor.Author = author;
                        }
                        bookauthors.Add(bookAuthor);
                    }
                    book.bookauthors = bookauthors;
                    Library library = dbConnection.Query<Library>("SELECT * FROM libraries WHERE libraryid = @libraryid", new { libraryid = book.libraryid }).FirstOrDefault();
                    book.librarydetails = library;

                    dbConnection.Close();
                }
                patron.books = bookList;
            }
            return patron;
            
        }

        /// <summary>
        /// Delete a patron
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Book> Remove(int id)
        {
            List<Book> booksList;
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var sql = "select * from books where patronid=" + id;
                booksList = Connection.Query<Book>(sql).ToList();
                if(booksList.Count == 0)
                {
                    dbConnection.Execute("DELETE FROM patrons WHERE patronid=@patronid", new { patronid = id });
                }
                dbConnection.Close();
            }
            return booksList;
        }

        /// <summary>
        /// Update a patron
        /// </summary>
        /// <param name="patron"></param>
        /// <returns></returns>
        public Patron Update(Patron patron)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Query("UPDATE patrons SET fname = @fname, lname = @lname,  email  = @email WHERE patronid = @patronid", new { fname = patron.fname, lname = patron.lname, email = patron.email });
                Patron UpdatedPatron = FindByID(patron.patronid);
                dbConnection.Close();
                return UpdatedPatron;
            }
        }

        /// <summary>
        /// Checkout a book
        /// </summary>
        /// <param name="patron"></param>
        /// <returns></returns>
        public List<Book> Checkout(Patron patron)
        {
            List<Book> nonCheckedOutBooks = new List<Book>();
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                
                foreach(Book book in patron.books)
                {
                    var sql = "select * from books where bookid=" + book.bookid;
                    Book b = Connection.Query<Book>(sql).FirstOrDefault();
                    if(b == null)
                    {

                        b = new Book();
                        b.bookid = book.bookid;
                        nonCheckedOutBooks.Add(b);
                    }
                    else if (b.isavailable == true)
                    {
                        DateTime bookDueDate = DateTime.Parse(DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"));
                        dbConnection.Query("UPDATE books SET isavailable = @isavailable, patronid=@patronid, duedate=@duedate WHERE bookid = @bookid", new { isavailable = false, patronid = patron.patronid, bookid = book.bookid, duedate = bookDueDate });
                    }
                    else
                    {
                        nonCheckedOutBooks.Add(b);
                    }
                }
                return nonCheckedOutBooks;
            }
        }

        /// <summary>
        /// Return book
        /// </summary>
        /// <param name="patron"></param>
        /// <returns></returns>
        public List<Book> ReturnBook(Patron patron)
        {
            List<Book> notReturnedBooks = new List<Book>();
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();

                foreach (Book book in patron.books)
                {
                    var sql = "select * from books where bookid=" + book.bookid;
                    Book b = Connection.Query<Book>(sql).FirstOrDefault();
                    if (b == null)
                    {

                        b = new Book();
                        b.bookid = book.bookid;
                        notReturnedBooks.Add(b);
                    }
                    else if(b.isavailable == false)
                    {
                        int? patronID = (int?)null;
                        DateTime? bookDueDate = (DateTime?)null;
                        dbConnection.Query("UPDATE books SET isavailable = @isavailable, patronid=@patronid, duedate=@duedate WHERE bookid = @bookid", new { isavailable = true, patronid = patronID, bookid = book.bookid, duedate = bookDueDate });
                    }
                    else
                    {
                        notReturnedBooks.Add(b);
                    }
                }
                return notReturnedBooks;
            }
        }
    }
}
