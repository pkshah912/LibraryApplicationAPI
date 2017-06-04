using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>
namespace LibraryApplicationAPI.Models
{
    public class Library : BaseEntity
    {
        [Key]
        public int libraryid;
        public string libraryname;
        public string address;
        public IEnumerable<Book> books { get; set; }

        public Library()
        {
            books = new List<Book>();
        }
    }
}
