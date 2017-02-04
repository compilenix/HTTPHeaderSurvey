using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Integration.DataAccess.Repositories
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        IQueryable<TEntity> EntitiesAsQueryable { get; }
        TEntity Add(TEntity entity);
        IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities);
        int Count();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        TEntity Get<TTarget>(TTarget id);
        IEnumerable<TEntity> GetAll();
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate);
    }
}