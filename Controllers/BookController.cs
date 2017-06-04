using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using LibraryApplicationAPI.Repository;
using Microsoft.Extensions.Configuration;
using LibraryApplicationAPI.Models;

/// <summary>
/// A book controller that handles the routes
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>
namespace LibraryApplicationAPI.Controllers
{
    [Route("api/[controller]")]
    public class BookController : Controller
    {
        public readonly BookRepository _bookRepository;

        public BookController(IConfiguration configuration)
        {
            _bookRepository = new BookRepository(configuration);
        }

        [HttpGet]
        public IEnumerable<Book> GetAll()
        {
            return _bookRepository.FindAll();
        }

        [HttpGet("{id}", Name = "GetBook")]
        public IActionResult GetById(int id)
        {
            var book = _bookRepository.FindByID(id);
            if (book == null)
            {
                return Ok("Book doesn't exist!");
            }
            return Ok(book);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Book item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            Library librarydetails = new Library();
            if(item.librarydetails != null)
            {
                librarydetails.libraryid = item.librarydetails.libraryid;
            }
            Book book = new Book();
            book.title = item.title;
            book.publisheddate = item.publisheddate;
            book.librarydetails = librarydetails;
            book.bookauthors = item.bookauthors;

            var createdBook = _bookRepository.Add(book);

            if(createdBook == null)
            {
                return Ok("Author doesn't exist!");
            }

            return CreatedAtRoute("GetBook", new { id = createdBook.bookid }, createdBook);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Book item)
        {
            if (item == null || item.bookid != id)
            {
                return BadRequest();
            }

            var book = _bookRepository.FindByID(id);
            if (book == null)
            {
                return Ok("Book doesn't exist!");
            }

            book.title = item.title;
            book.publisheddate = item.publisheddate;
            book.bookauthors = item.bookauthors;

            var updatedBook = _bookRepository.Update(book);
            if(updatedBook == null)
            {
                return Ok("Author doesn't exist!");
            }
            return Ok(updatedBook);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var book = _bookRepository.FindByID(id);
            if (book == null)
            {
                return Ok("Book doesn't exist!");
            }

            _bookRepository.Remove(id);
            return Ok("Book successfully deleted!");
        }

        [HttpGet("getByAuthor/{searchString}")]
        public IActionResult GetByAuthor(string searchString)
        {
            var book = _bookRepository.GetByAuthor(searchString);
            return Ok(book);
        }

        [HttpGet("getBooksDue")]
        public IEnumerable<Book> GetBooksDue()
        {
            return _bookRepository.BooksDue();
        }

        [HttpPut("transferBook")]
        public IActionResult TransferBook([FromBody] Book item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            var book = _bookRepository.FindByID(item.bookid);
            if (book == null)
            {
                return Ok("Book doesn't exist!");
            }

            book.bookid = item.bookid;
            book.libraryid = item.libraryid;

            var transferToOtherLibrary = _bookRepository.TransferBook(book);
            if (transferToOtherLibrary == 0)
            {
                return Ok("Can't transfer book to other library as it is already issued! Try again when the book is returned");
            }
            else if(transferToOtherLibrary == 1)
            {
                return Ok("Can't transfer book as it is in the same library!");
            }
            else if(transferToOtherLibrary == 3)
            {
                return Ok("Library doesn't exist!");
            }
            var transferredBook = _bookRepository.FindByID(book.bookid);
            return Ok(transferredBook);
        }
    }
}
