using System.Linq.Expressions;
using DFIR.CaseManagement.Entities;

namespace DFIR.CaseManagement.Interfaces;

/// <summary>Generic repository abstraction (Repository Pattern).</summary>
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> Query();
}
