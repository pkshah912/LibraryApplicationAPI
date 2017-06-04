using LibraryApplicationAPI.Models;
using System.Collections.Generic;

/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>
namespace LibraryApplicationAPI.Repository
{
    public interface IPatronRepository<T> where T : BaseEntity
    {
        T Add(T item);
        List<Book> Remove(int id);
        T Update(T item);
        T FindByID(int id);
        IEnumerable<T> FindAll();
        List<Book> ReturnBook(T item);
        List<Book> Checkout(T item);
    }
}
