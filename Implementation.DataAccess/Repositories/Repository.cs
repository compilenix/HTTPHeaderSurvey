using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Implementation.DataAccess.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly DbContext Context;
        public IQueryable<TEntity> EntitiesAsQueryable => Entities;
        protected DbSet<TEntity> Entities => Context?.Set<TEntity>();

        protected Repository(DbContext context)
        {
            Context = context;
        }

        public IEnumerable<TEntity> GetAll()
        {
            return Entities?.ToList() ?? new List<TEntity>();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return Entities?.Where(predicate).ToList();
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return Entities?.SingleOrDefault(predicate);
        }

        public virtual TEntity Add(TEntity entity)
        {
            entity.DateCreated = DateTime.Now;
            return Entities?.Add(entity);
        }

        public void Remove(TEntity entity)
        {
            Entities?.Remove(entity);
        }

        public TEntity Get<TTarget>(TTarget id)
        {
            return Entities?.Find(id);
        }

        public int Count()
        {
            return Entities?.Count() ?? 0;
        }

        public TEntity Update(TEntity entity)
        {
            Entities.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
            return entity;
        }
    }
}