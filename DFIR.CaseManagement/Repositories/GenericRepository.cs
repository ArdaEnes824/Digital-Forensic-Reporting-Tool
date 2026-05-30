using System.Linq.Expressions;
using DFIR.CaseManagement.Data;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DFIR.CaseManagement.Repositories;

/// <summary>Concrete Repository Pattern implementation over EF Core.</summary>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> Set;

    public GenericRepository(AppDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
        => await Set.AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await Set.Where(predicate).ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
        => await Set.FindAsync(id);

    public async Task AddAsync(T entity)
        => await Set.AddAsync(entity);

    public void Update(T entity)
    {
        entity.UpdatedDate = DateTime.UtcNow;
        Set.Update(entity);
    }

    public void Remove(T entity) => Set.Remove(entity);

    public async Task<int> CountAsync() => await Set.CountAsync();

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        => await Set.CountAsync(predicate);

    public IQueryable<T> Query() => Set.AsQueryable();
}
