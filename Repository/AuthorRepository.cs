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
    public class AuthorRepository : IRepository<Author>
    {
        private string connectionString;
        public AuthorRepository(IConfiguration configuration)
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
        /// Adds author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        public Author Add(Author author)
        {
            Author AddedAuthor;
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO authors(fname, lname) VALUES" +
                    "(@fname, @lname)", author);
                var AuthorsList = FindAll();
                AddedAuthor = AuthorsList.Last();

                int addedAuthorID = AddedAuthor.authorid;
                foreach (var book in author.bookauthors)
                {
                    var getBooks = "select * from books where bookid = " + book.bookid;
                    List<BookAuthor> booksList = Connection.Query<BookAuthor>(getBooks).ToList();
                    if (booksList.Count == 0)
                    {
                        dbConnection.Execute("DELETE FROM authors WHERE authorid=@authorid", new { authorid = addedAuthorID });
                        return null;
                    }
                    dbConnection.Execute("INSERT INTO bookauthor(bookid, authorid) VALUES (@bookid, @authorid)", new { bookid = book.bookid, authorid = addedAuthorID });
                }
                AddedAuthor = FindByID(addedAuthorID);

                dbConnection.Close();
            }
            return AddedAuthor;
        }

        /// <summary>
        /// Returns all the authors
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Author> FindAll()
        {
            var sql = "SELECT * FROM authors";
            List<Author> authorsList = Connection.Query<Author>(sql).ToList();
            foreach(Author author in authorsList)
            {
                var getBookAuthors = "select authorid, bookid from bookauthor where authorid = "+ author.authorid;
                List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                List<BookAuthor> bookauthors = new List<BookAuthor>();
                foreach(BookAuthor bookAuthor in bookAuthorsList)
                {
                    var getBookID = bookAuthor.bookid;
                    var getBooksDetails = "select * from books where bookid=" + getBookID;
                    List<Book> booksList = Connection.Query<Book>(getBooksDetails).ToList();
                    foreach(Book book in booksList)
                    {
                        var getLibraryDetails = "select * from libraries where libraryid = " + book.libraryid;
                        Library libraryDetailsList = Connection.Query<Library>(getLibraryDetails).FirstOrDefault();
                        Library library = new Library();

                        if (libraryDetailsList != null)
                        {
                            library.libraryid = book.libraryid;
                            library.libraryname = libraryDetailsList.libraryname;
                            library.address = libraryDetailsList.address;
                        }
                        
                        book.librarydetails = library;
                        bookAuthor.bookid = book.bookid;
                        bookAuthor.Book = book;
                    }
                    bookauthors.Add(bookAuthor);
                }
                author.bookauthors = bookauthors;
            }
            return authorsList;
        }

        /// <summary>
        /// Returns the author by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Author FindByID(int id)
        {
            Author author;
            IDbConnection dbConnection = Connection;
            dbConnection.Open();
            author = dbConnection.Query<Author>("SELECT * FROM authors WHERE authorid = @authorid", new { authorid = id }).FirstOrDefault();
            
            if (author!=null)
            {
                var getBookAuthors = "select authorid, bookid from bookauthor where authorid = " + author.authorid;
                List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                List<BookAuthor> bookauthors = new List<BookAuthor>();
                foreach (BookAuthor bookAuthor in bookAuthorsList)
                {
                    var getBookID = bookAuthor.bookid;
                    var getBooksDetails = "select * from books where bookid=" + getBookID;
                    List<Book> booksList = Connection.Query<Book>(getBooksDetails).ToList();
                    foreach (Book book in booksList)
                    {
                        var getLibraryDetails = "select * from libraries where libraryid = " + book.libraryid;
                        Library libraryDetailsList = Connection.Query<Library>(getLibraryDetails).FirstOrDefault();
                        Library library = new Library();
                        if (libraryDetailsList != null)
                        {
                            library.libraryid = book.libraryid;
                            library.libraryname = libraryDetailsList.libraryname;
                            library.address = libraryDetailsList.address;
                        }
                        book.librarydetails = library;
                        bookAuthor.bookid = book.bookid;
                        bookAuthor.Book = book;
                    }
                    bookauthors.Add(bookAuthor);
                }
                author.bookauthors = bookauthors;
                dbConnection.Close();
            }
            return author;
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
                dbConnection.Execute("DELETE FROM authors WHERE authorid=@authorid", new { authorid = id });
                dbConnection.Execute("DELETE FROM bookauthor WHERE authorid=@authorid", new { authorid = id });
                dbConnection.Close();
            }
        }

        /// <summary>
        /// Update the author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        public Author Update(Author author)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var getAuthorDetails = "select * from authors where authorid = " + author.authorid;
                Author authorDetails = Connection.Query<Author>(getAuthorDetails).FirstOrDefault();
                dbConnection.Query("UPDATE authors SET fname = @fname,  lname  = @lname WHERE authorid = @authorid", author);
                foreach(BookAuthor bookAuthor in author.bookauthors)
                {
                    var getBookAuthors = "select authorid, bookid from bookauthor where authorid = " + author.authorid + "and bookid =" + bookAuthor.bookid;
                    List<BookAuthor> bookAuthorsList = Connection.Query<BookAuthor>(getBookAuthors).ToList();
                    if(bookAuthorsList.Count == 0)
                    {
                        var getBook = "select * from books where bookid = " + bookAuthor.bookid;
                        List<BookAuthor> booksList = Connection.Query<BookAuthor>(getBook).ToList();
                        if (booksList.Count == 0)
                        {
                            dbConnection.Query("UPDATE authors SET fname = @fname,  lname  = @lname WHERE authorid = @authorid", new { fname = authorDetails.fname, lname = authorDetails.lname, authorid = author.authorid});
                            return null;
                        }
                        
                        dbConnection.Execute("INSERT INTO bookauthor (bookid, authorid) VALUES(@bookid, @authorid)", new { bookid = bookAuthor.bookid, authorid = author.authorid });
                    }
                }
                Author UpdatedAuthor = FindByID(author.authorid);
                dbConnection.Close();
                return UpdatedAuthor;
            }
        }
    }
}
