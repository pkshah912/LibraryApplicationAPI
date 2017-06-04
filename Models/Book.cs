using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>
namespace LibraryApplicationAPI.Models
{
    public class Book : BaseEntity
    {
        [Key]
        public int bookid { get; set; }
        public string title { get; set; }
        public DateTime publisheddate { get; set; }
        public IEnumerable<BookAuthor> bookauthors { get; set; }
        public int libraryid { get; set; }
        public Library librarydetails { get; set; }
        public bool isavailable { get; set; }
        public int patronid { get; set; }
        public Patron patrondetails { get; set; }
        public DateTime duedate { get; set; }

        public Book()
        {
            bookauthors = new List<BookAuthor>();
        }
    }
}
