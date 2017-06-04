/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>

namespace LibraryApplicationAPI.Models
{
    public class BookAuthor : BaseEntity
    {
        public int bookid { get; set; }
        public Book Book { get; set; }
        public int authorid { get; set; }
        public Author Author { get; set; }
    }
}
