using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Integration.DataAccess.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Get<TTarget>(TTarget id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate);
        TEntity Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
