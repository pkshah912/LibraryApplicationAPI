using Dapper;
using LibraryApplicationAPI.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;

/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>
namespace LibraryApplicationAPI.Repository
{
    public class LibraryRepository : IRepository<Library>
    {
        private string connectionString;
        public LibraryRepository(IConfiguration configuration)
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
        /// Add a library
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        public Library Add(Library library)
        {
            Library AddedLibrary;
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("insert into libraries(libraryname, address)  VALUES (@libraryname, @address)", new { libraryname= library.libraryname, address = library.address});
                var LibraryList = FindAll();
                Library libraryElement = LibraryList.Last();
                int addedLibraryID = libraryElement.libraryid;
                foreach (var book in library.books)
                {
                    var getBook = "select * from books where bookid = " + book.bookid;
                    List<Book> booksList = Connection.Query<Book>(getBook).ToList();
                    if (booksList.Count == 0)
                    {
                        dbConnection.Execute("DELETE FROM libraries WHERE libraryid=@libraryid", new { libraryid = addedLibraryID });
                        return null;
                    }
                    dbConnection.Execute("update books set libraryid = @libraryid where bookid=@bookid", new { libraryid = addedLibraryID, bookid = book.bookid });
                }
                AddedLibrary = FindByID(addedLibraryID);
                dbConnection.Close();
            }
            return AddedLibrary;
        }

        /// <summary>
        /// Get all the libraries
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Library> FindAll()
        {
            var sql = "SELECT * FROM libraries";
            List<Library> libraryList = Connection.Query<Library>(sql).ToList();
            foreach(Library library in libraryList)
            {
                int libraryID = library.libraryid;
                var getBooks = "select * from books where libraryid=" + libraryID;
                List<Book> bookList = Connection.Query<Book>(getBooks).ToList();
                List<Book> bookDetails = new List<Book>();
                foreach(Book book in bookList)
                {
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
                    Book currentBook = new Book();
                    currentBook.bookid = book.bookid;
                    currentBook.title = book.title;
                    currentBook.publisheddate = book.publisheddate;
                    currentBook.libraryid = library.libraryid;
                    currentBook.bookauthors = bookAuthors;
                    bookDetails.Add(currentBook);
                }
                library.books = bookDetails;
            }
            return libraryList;
        }

        /// <summary>
        /// Get library by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Library FindByID(int id)
        {
            Library library;
            IDbConnection dbConnection = Connection;
            dbConnection.Open();
            library = dbConnection.Query<Library>("SELECT * FROM libraries WHERE libraryid = @libraryid", new { libraryid = id }).FirstOrDefault();
            if(library != null)
            {
                int libraryID = library.libraryid;
                var getBooks = "select * from books where libraryid=" + libraryID;
                List<Book> bookList = Connection.Query<Book>(getBooks).ToList();
                List<Book> bookDetails = new List<Book>();
                foreach (Book book in bookList)
                {
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
                    Book currentBook = new Book();
                    currentBook.bookid = book.bookid;
                    currentBook.title = book.title;
                    currentBook.publisheddate = book.publisheddate;
                    currentBook.libraryid = library.libraryid;
                    currentBook.bookauthors = bookAuthors;
                    bookDetails.Add(currentBook);
                }
                library.books = bookDetails;
            }
            return library;
        }

        /// <summary>
        /// Delete a library
        /// </summary>
        /// <param name="id"></param>
        public void Remove(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("DELETE FROM libraries WHERE libraryid=@libraryid", new { libraryid = id });
                dbConnection.Close();
            }
        }

        /// <summary>
        /// Update the library
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        public Library Update(Library library)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var getLibraryDetails = "select * from libraries where libraryid = " + library.libraryid;
                Library libraryDetails = Connection.Query<Library>(getLibraryDetails).FirstOrDefault();

                dbConnection.Query("UPDATE libraries SET libraryname = @libraryname,  address  = @address WHERE libraryid = @libraryid", new { libraryname = library.libraryname, address = library.address, libraryid = library.libraryid });
                foreach (var book in library.books)
                {
                    var getBook = "select * from books where bookid = " + book.bookid;
                    List<Book> booksList = Connection.Query<Book>(getBook).ToList();
                    if (booksList.Count == 0)
                    {
                        dbConnection.Query("UPDATE libraries SET libraryname = @libraryname,  address  = @address WHERE libraryid = @libraryid", new { libraryname = libraryDetails.libraryname, address = libraryDetails.address, libraryid = library.libraryid });
                        return null;
                    }
                    dbConnection.Execute("update books set libraryid = @libraryid where bookid=@bookid", new { libraryid = library.libraryid, bookid = book.bookid });
                }
                Library UpdatedLibrary = FindByID(library.libraryid);
                dbConnection.Close();
                return UpdatedLibrary;
            }
        }
    }
}
