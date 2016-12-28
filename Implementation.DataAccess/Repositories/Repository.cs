using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Implementation.DataAccess.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly DbContext Context;
        protected DbSet<TEntity> Entities => Context?.Set<TEntity>();

        public Repository(DbContext context)
        {
            Context = context;
        }

        public IEnumerable<TEntity> GetAll()
        {
            return Entities?.ToList();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return predicate != null ? Entities?.Where(predicate) : null;
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return predicate != null ? Entities?.SingleOrDefault(predicate) : null;
        }

        public virtual TEntity Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.DateCreated = DateTime.Now;

            return Entities?.Add(entity);
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var entity in entities)
            {
                entity.DateCreated = DateTime.Now;
            }

            Entities?.AddRange(entities);
        }

        public void Remove(TEntity entity)
        {
            if (entity == null)
            {
                throw new  ArgumentNullException(nameof(entity));
            }

            Entities?.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            Entities?.RemoveRange(entities);
        }

        public TEntity Get<TTarget>(TTarget id)
        {
            return Entities?.Find(id);
        }
    }
}
