using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using LibraryApplicationAPI.Models;
using LibraryApplicationAPI.Repository;
using Microsoft.Extensions.Configuration;

/// <summary>
/// A patron controller that handles the routes
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>

namespace LibraryApplicationAPI.Controllers
{
    [Route("api/[controller]")]
    public class PatronController : Controller
    {
        public readonly PatronRepository _patronRepository;

        public PatronController(IConfiguration configuration)
        {
            _patronRepository = new PatronRepository(configuration);
        }

        [HttpGet]
        public IEnumerable<Patron> GetAll()
        {
            return _patronRepository.FindAll();
        }

        [HttpGet("{id}", Name = "GetPatron")]
        public IActionResult GetById(int id)
        {
            var patron = _patronRepository.FindByID(id);
            if (patron == null)
            {
                return Ok("Patron doesn't exist!");
            }
            return Ok(patron);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Patron item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            var createdPatron = _patronRepository.Add(item);

            return CreatedAtRoute("GetPatron", new { id = createdPatron.patronid }, createdPatron);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Patron item)
        {
            if (item == null || item.patronid != id)
            {
                return BadRequest();
            }

            var patron = _patronRepository.FindByID(id);
            if (patron == null)
            {
                return Ok("Patron doesn't exist!");
            }

            patron.fname = item.fname;
            patron.lname = item.lname;
            patron.email = item.email;

            var updatedPatron = _patronRepository.Update(patron);
            
            return Ok(updatedPatron);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var patron = _patronRepository.FindByID(id);
            if (patron == null)
            {
                return Ok("Patron doesn't exist!");
            }

            var books = _patronRepository.Remove(id);
            if (books.Count > 0)
            {
                String book = "";
                foreach (Book b in books)
                {
                    book += b.bookid + ",";
                }
                book = book.Substring(0, book.Length - 1);
                return Ok("Patron not deleted as books with IDs " + book + " not checked out");
            }
            return Ok("Patron successfully deleted!");
        }

        [HttpPut("checkout", Name = "Checkout")]
        public IActionResult Checkout([FromBody] Patron item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            var patron = _patronRepository.FindByID(item.patronid);
            if (patron == null)
            {
                return Ok("Patron doesn't exist!");
            }

            patron.books = item.books;

            var checkoutBooks = _patronRepository.Checkout(patron);
            if (checkoutBooks.Count > 0)
            {
                String books = "";
                foreach(Book b in checkoutBooks)
                {
                    books += b.bookid + ",";
                }
                books = books.Substring(0, books.Length-1);
                return Ok("Books with IDs " + books + " not checked out as they do not exist or issued by someone else");
            }
            return Ok("Books successfully checked out");
        }

        [HttpPut("returnbook", Name = "ReturnBook")]
        public IActionResult ReturnBook([FromBody] Patron item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            var patron = _patronRepository.FindByID(item.patronid);
            if (patron == null)
            {
                return Ok("Patron doesn't exist!");
            }

            patron.books = item.books;

            var returnBook = _patronRepository.ReturnBook(patron);
            if (returnBook.Count > 0)
            {
                String books = "";
                foreach (Book b in returnBook)
                {
                    books += b.bookid + ",";
                }
                books = books.Substring(0, books.Length - 1);
                return Ok("Books with IDs " + books + " are not returned successfully as they do not exist or is already present in library");
            }
            return Ok("Books successfully returned");
        }
    }
}
