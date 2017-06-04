using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using LibraryApplicationAPI.Repository;
using Microsoft.Extensions.Configuration;
using LibraryApplicationAPI.Models;

/// <summary>
/// An author controller that handles the routes
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>
namespace LibraryApplicationAPI.Controllers
{
    [Route("api/[controller]")]
    public class AuthorController : Controller
    {
        public readonly AuthorRepository _authorRepository;

        public AuthorController(IConfiguration configuration)
        {
            _authorRepository = new AuthorRepository(configuration);
        }

        [HttpGet]
        public IEnumerable<Author> GetAll()
        {
            return _authorRepository.FindAll();
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetById(int id)
        {
            var author = _authorRepository.FindByID(id);
            if (author == null)
            {
                return Ok("Author doesn't exist!");
            }
            return Ok(author);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Author item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            var createdAuthor = _authorRepository.Add(item);

            if(createdAuthor == null)
            {
                return Ok("Book doesn't exist!");
            }

            return CreatedAtRoute("GetAuthor", new { id = createdAuthor.authorid }, createdAuthor);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Author item)
        {
            if (item == null || item.authorid != id)
            {
                return BadRequest();
            }

            var author = _authorRepository.FindByID(id);
            if (author == null)
            {
                return Ok("Author doesn't exist!");
            }

            author.fname = item.fname;
            author.lname = item.lname;
            author.bookauthors = item.bookauthors;

            var updatedAuthor = _authorRepository.Update(author);
            if (updatedAuthor == null)
            {
                return Ok("Book doesn't exist!");
            }
            return Ok(updatedAuthor);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var author = _authorRepository.FindByID(id);
            if (author == null)
            {
                return Ok("Author doesn't exist!");
            }

            _authorRepository.Remove(id);
            return Ok("Author successfully deleted!");
        }
    }
}
