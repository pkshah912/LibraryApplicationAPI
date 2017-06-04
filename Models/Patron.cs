using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>
namespace LibraryApplicationAPI.Models
{
    public class Patron : BaseEntity
    {
        [Key]
        public int patronid { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string email { get; set; }
        public IEnumerable<Book> books { get; set; }

        public Patron()
        {
            books = new List<Book>();
        }
    }
}
