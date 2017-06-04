using Dapper;
using LibraryApplicationAPI.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>

namespace LibraryApplicationAPI.Repository
{
    public class BookRepository : IRepository<Book>
    {
        private string connectionString;
        public BookRepository(IConfiguration configuration)
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
        /// Add a book
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public Book Add(Book book)
        {
            Book AddedBook;
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                bool isAvailable = true;
                if (book.librarydetails.libraryid != 0)
                {
                    var sql = "SELECT * FROM libraries where libraryid=" + book.librarydetails.libraryid;
                    List<Library> libraryList = Connection.Query<Library>(sql).ToList();
                    if (libraryList.Count == 0)
                    {
                        return null;
                    }

                    dbConnection.Execute("INSERT INTO books(title, publisheddate, libraryid, isavailable) VALUES" +
                        "(@title, @publisheddate, @libraryid, @isavailable)", new { title = book.title, publisheddate = book.publisheddate, libraryid = book.librarydetails.libraryid, isavailable = isAvailable });
                }
                else
                {
                    dbConnection.Execute("INSERT INTO books(title, publisheddate, isavailable) VALUES" +
                        "(@title, @publisheddate, @isavailable)", new { title = book.title, publisheddate = book.publisheddate, isavailable = isAvailable });
                }
                var BooksList = FindAll();
                Book bookElement = BooksList.Last();
                int addedBookID = bookElement.bookid;
                foreach (var author in book.bookauthors)
                {
                    var getAuthor = "select * from authors where authorid = " + author.authorid;
                    List<BookAuthor> authorsList = Connection.Query<BookAuthor>(getAuthor).ToList();
                    if(authorsList.Count == 0)
                    {
                        dbConnection.Execute("DELETE FROM books WHERE bookid=@bookid", new { bookid = addedBookID });
                        return null;
                    }
                    dbConnection.Execute("INSERT INTO bookauthor(bookid, authorid) VALUES (@bookid, @authorid)", new { bookid = addedBookID, authorid = author.authorid });
                }
                AddedBook = FindByID(addedBookID);
                dbConnection.Close();
            }
            return AddedBook;
        }

        /// <summary>
        /// Gets all the books
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Book> FindAll()
        {
            var sql = "SELECT * FROM books";
            List<Book> booksList = Connection.Query<Book>(sql).ToList();

            foreach (Book book in booksList)
            {
                var getLibraryDetails = "select * from libraries where libraryid = " + book.libraryid;
                Library libraryDetailsList = Connection.Query<Library>(getLibraryDetails).FirstOrDefault();

                Library library = new Library();
                if(libraryDetailsList != null)
                {
                    library.libraryid = book.libraryid;
                    library.libraryname = libraryDetailsList.libraryname;
                    library.address = libraryDetailsList.address;
                }

                var getPatronDetails = "select * from patrons where patronid = " + book.patronid;
                Patron patronDetails = Connection.Query<Patron>(getPatronDetails).FirstOrDefault();

                var getBookAuthors = "select authorid, bookid from bookauthor where bookid = " + book.bookid;
                List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                List<BookAuthor> bookAuthors = new List<BookAuthor>();
                foreach (BookAuthor bookAuthor in bookAuthorsList)
                {
                    var getAuthorID = bookAuthor.authorid;
                    var getBooksDetails = "select * from authors where authorid=" + getAuthorID;
                    List<Author> authorsList = Connection.Query<Author>(getBooksDetails).ToList();
                    foreach (Author author in authorsList)
                    {
                        bookAuthor.authorid = author.authorid;
                        bookAuthor.Author = author;
                    }
                    bookAuthors.Add(bookAuthor);
                }
                book.librarydetails = library;
                book.bookauthors = bookAuthors;
                book.patrondetails = patronDetails;
            }
            return booksList;
        }


        /// <summary>
        /// Gets the book by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Book FindByID(int id)
        {
            Book book;
            IDbConnection dbConnection = Connection;
            dbConnection.Open();
            book = dbConnection.Query<Book>("SELECT * FROM books WHERE bookid = @bookid", new { bookid = id }).FirstOrDefault();
            if(book != null)
            {
                var getPatronDetails = "select * from patrons where patronid = " + book.patronid;
                Patron patronDetails = Connection.Query<Patron>(getPatronDetails).FirstOrDefault();
                var getLibraryDetails = "select * from libraries where libraryid = " + book.libraryid;
                Library libraryDetailsList = Connection.Query<Library>(getLibraryDetails).FirstOrDefault();
                Library library = new Library();
                if (libraryDetailsList != null)
                {
                    library.libraryid = book.libraryid;
                    library.libraryname = libraryDetailsList.libraryname;
                    library.address = libraryDetailsList.address;
                }
                var getBookAuthors = "select authorid, bookid from bookauthor where bookid = " + book.bookid;
                List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                List<BookAuthor> bookAuthors = new List<BookAuthor>();
                foreach (BookAuthor bookAuthor in bookAuthorsList)
                {
                    var getAuthorID = bookAuthor.authorid;
                    var getBooksDetails = "select * from authors where authorid=" + getAuthorID;
                    List<Author> authorsList = Connection.Query<Author>(getBooksDetails).ToList();
                    foreach (Author author in authorsList)
                    {
                        bookAuthor.authorid = author.authorid;
                        bookAuthor.Author = author;
                    }
                    bookAuthors.Add(bookAuthor);
                }
                book.librarydetails = library;
                book.bookauthors = bookAuthors;
                book.patrondetails = patronDetails;
                dbConnection.Close();
            }
            return book;
        }

        /// <summary>
        /// Delete the book
        /// </summary>
        /// <param name="id"></param>
        public void Remove(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("DELETE FROM books WHERE bookid=@bookid", new { bookid = id });
                dbConnection.Close();
            }
        }


        /// <summary>
        /// Delete the book
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public Book Update(Book book)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var getBookDetails = "select * from books where bookid = " + book.bookid;
                Book bookDetails = Connection.Query<Book>(getBookDetails).FirstOrDefault();
                dbConnection.Query("UPDATE books SET title = @title,  publisheddate  = @publisheddate WHERE bookid = @bookid", book);
                foreach (BookAuthor bookAuthor in book.bookauthors)
                {
                    var getBookAuthors = "select authorid, bookid from bookauthor where authorid = " + bookAuthor.authorid + "and bookid =" + book.bookid;
                    List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                    if (bookAuthorsList.Count == 0)
                    {
                        var getAuthor = "select * from authors where authorid = " + bookAuthor.authorid;
                        List<BookAuthor> authorsList = Connection.Query<BookAuthor>(getAuthor).ToList();
                        if(authorsList.Count == 0)
                        {
                            dbConnection.Query("UPDATE books SET title = @title,  publisheddate  = @publisheddate WHERE bookid = @bookid", new { title = bookDetails.title, publisheddate = bookDetails.publisheddate, bookid = book.bookid});
                            return null;
                        }
                        dbConnection.Execute("INSERT INTO bookauthor (bookid, authorid) VALUES(@bookid, @authorid)", new { bookid = book.bookid, authorid = bookAuthor.authorid });
                    }
                }
                Book UpdatedBook = FindByID(book.bookid);
                dbConnection.Close();
                return UpdatedBook;
            }
        }

        /// <summary>
        /// Get books by author
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public IEnumerable<Book> GetByAuthor(string searchString)
        {
            var sql = "select * from authors where fname ilike '%" + searchString + "%' or lname ilike '%" + searchString + "%'";
            List<Author> authorsList = Connection.Query<Author>(sql).ToList();
            List<Book> booksList = new List<Book>();
            foreach(Author author in authorsList)
            {
                var getBooksByAuthors = "select bookid from bookauthor where authorid = " + author.authorid;
                List<BookAuthor> booksByAuthorsList = Connection.Query<BookAuthor>(getBooksByAuthors).ToList();
                foreach (BookAuthor bookAuthor in booksByAuthorsList)
                {
                    var getAuthorID = bookAuthor.authorid;
                    var getBookID = bookAuthor.bookid;
                    var getBooksDetails = "select * from books where bookid=" + getBookID;
                    List<Book> books = Connection.Query<Book>(getBooksDetails).ToList();
                    foreach (Book b in books)
                    {
                        var getLibraryDetails = "select * from libraries where libraryid = " + b.libraryid;
                        Library libraryDetailsList = Connection.Query<Library>(getLibraryDetails).FirstOrDefault();
                        Library library = new Library();
                        if(b.libraryid != 0)
                        {
                            library.libraryid = b.libraryid;
                            library.libraryname = libraryDetailsList.libraryname;
                            library.address = libraryDetailsList.address;
                        }

                        var getPatronDetails = "select * from patrons where patronid = " + b.patronid;
                        Patron patronDetails = Connection.Query<Patron>(getPatronDetails).FirstOrDefault();

                        var getBookAuthors = "select authorid, bookid from bookauthor where bookid = " + b.bookid;
                        List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                        List<BookAuthor> bookAuthors = new List<BookAuthor>();
                        foreach (BookAuthor ba in bookAuthorsList)
                        {
                            var authorID = ba.authorid;
                            var getAuthors = "select * from authors where authorid=" + authorID;
                            List<Author> listAuthors = Connection.Query<Author>(getAuthors).ToList();
                            foreach (Author a in listAuthors)
                            {
                                ba.authorid = a.authorid;
                                ba.Author = a;
                            }
                            bookAuthors.Add(ba);
                        }
                        b.librarydetails = library;
                        b.bookauthors = bookAuthors;
                        b.patrondetails = patronDetails;
                        booksList.Add(b);
                    }
                }
            }
            return booksList;
        }

        /// <summary>
        /// Gets the book that are due
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Book> BooksDue()
        {
            String bookDueDate = DateTime.Now.ToString("yyyy-MM-dd");
            var sql = "SELECT * FROM books where duedate < '" + bookDueDate + "'";
            List<Book> booksList = Connection.Query<Book>(sql).ToList();

            foreach (Book book in booksList)
            {
                var getLibraryDetails = "select * from libraries where libraryid = " + book.libraryid;
                Library libraryDetailsList = Connection.Query<Library>(getLibraryDetails).FirstOrDefault();
                Library library = new Library();
                library.libraryid = book.libraryid;
                library.libraryname = libraryDetailsList.libraryname;
                library.address = libraryDetailsList.address;

                var getPatronDetails = "select * from patrons where patronid = " + book.patronid;
                Patron patronDetails = Connection.Query<Patron>(getPatronDetails).FirstOrDefault();

                var getBookAuthors = "select authorid, bookid from bookauthor where bookid = " + book.bookid;
                List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                List<BookAuthor> bookAuthors = new List<BookAuthor>();
                foreach (BookAuthor bookAuthor in bookAuthorsList)
                {
                    var getAuthorID = bookAuthor.authorid;
                    var getBooksDetails = "select * from authors where authorid=" + getAuthorID;
                    List<Author> authorsList = Connection.Query<Author>(getBooksDetails).ToList();
                    foreach (Author author in authorsList)
                    {
                        bookAuthor.authorid = author.authorid;
                        bookAuthor.Author = author;
                    }
                    bookAuthors.Add(bookAuthor);
                }
                book.librarydetails = library;
                book.bookauthors = bookAuthors;
                book.patrondetails = patronDetails;
            }
            return booksList;
        }

        /// <summary>
        /// Transfers the book to another library
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public int TransferBook(Book b)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var sql = "SELECT * from books where bookid = " + b.bookid;
                Book book = dbConnection.Query<Book>(sql).FirstOrDefault();
                if(book.isavailable == false)
                {
                    return 0;
                }
                else if(book.libraryid == b.libraryid)
                {
                    return 1;
                }
                else
                {
                    var getLibrary = "select * from libraries where libraryid = " + b.libraryid;
                    List<Library> libraryList = Connection.Query<Library>(getLibrary).ToList();
                    if (libraryList.Count == 0)
                    {
                        return 3;
                    }
                    dbConnection.Query("UPDATE books SET libraryid = @libraryid WHERE bookid = @bookid", new { libraryid = b.libraryid, bookid = b.bookid });
                    return 2;
                }
            }
        }
    }
}
