using LibraryApplicationAPI.Models;
using System.Collections.Generic;

/// <summary>
/// Reference: http://techbrij.com/asp-net-core-postgresql-dapper-crud
/// </summary>
namespace LibraryApplicationAPI.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        T Add(T item);
        void Remove(int id);
        T Update(T item);
        T FindByID(int id);
        IEnumerable<T> FindAll();
    }
}
