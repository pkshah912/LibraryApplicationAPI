using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using LibraryApplicationAPI.Repository;
using Microsoft.Extensions.Configuration;
using LibraryApplicationAPI.Models;

/// <summary>
/// A library controller that handles the routes
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>

namespace LibraryApplicationAPI.Controllers
{
    [Route("api/[controller]")]
    public class LibraryController : Controller
    {
        public readonly LibraryRepository _libraryRepository;

        public LibraryController(IConfiguration configuration)
        {
            _libraryRepository = new LibraryRepository(configuration);
        }

        [HttpGet]
        public IEnumerable<Library> GetAll()
        {
            return _libraryRepository.FindAll();
        }

        [HttpGet("{id}", Name = "GetLibrary")]
        public IActionResult GetById(int id)
        {
            var library = _libraryRepository.FindByID(id);
            if (library == null)
            {
                return Ok("Library doesn't exist!");
            }
            return Ok(library);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Library item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            var createdLibrary = _libraryRepository.Add(item);
            if(createdLibrary == null)
            {
                return Ok("Book doesn't exist!");
            }
            
            return CreatedAtRoute("GetLibrary", new { id = createdLibrary.libraryid }, createdLibrary);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Library item)
        {
            if (item == null || item.libraryid != id)
            {
                return BadRequest();
            }

            var library = _libraryRepository.FindByID(id);
            if (library == null)
            {
                return Ok("Library doesn't exist!");
            }

            library.libraryname = item.libraryname;
            library.address = item.address;
            library.books = item.books;
            
            var updatedLibrary = _libraryRepository.Update(library);

            if (updatedLibrary == null)
            {
                return Ok("Book doesn't exist!");
            }

            return Ok(updatedLibrary);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var library = _libraryRepository.FindByID(id);
            if (library == null)
            {
                return Ok("Library doesn't exist!");
            }

            _libraryRepository.Remove(id);
            return Ok("Library successfully deleted!");
        }
    }
}
