using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Integration.DataAccess.Entitys;

namespace Integration.DataAccess.Repositories
{
    public interface IRepository<TEntity>
        where TEntity : BaseEntity
    {
        IQueryable<TEntity> EntitiesAsQueryable { get; }
        TEntity Add(TEntity entity);
        int Count();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        TEntity Get<TTarget>(TTarget id);
        IEnumerable<TEntity> GetAll();
        void Remove(TEntity entity);
        TEntity Update(TEntity entity);
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate);
    }
}