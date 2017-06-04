using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>
namespace LibraryApplicationAPI.Models
{
    public class Author : BaseEntity
    {
        public int authorid { get; set; }
        public string fname { get; set; }
        [Required]
        public string lname { get; set; }
        public IEnumerable<BookAuthor> bookauthors { get; set; }

        public Author()
        {
            bookauthors = new List<BookAuthor>();
        }
    }
}
