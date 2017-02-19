using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly DataAccessContext Context;
        public IQueryable<TEntity> EntitiesAsQueryable => Entities;
        protected DbSet<TEntity> Entities => Context?.Set<TEntity>();

        protected Repository(DataAccessContext context)
        {
            Context = context;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await Entities?.ToListAsync() ?? new List<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Entities?.Where(predicate).ToListAsync();
        }

        public Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Entities?.SingleOrDefaultAsync(predicate);
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

        public async Task<TEntity> GetAsync<TTarget>(TTarget id)
        {
            return await Entities?.FindAsync(id);
        }

        public async Task<int> CountAsync()
        {
            return await Entities?.CountAsync();
        }

        public TEntity Update(TEntity entity)
        {
            Entities.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
            return entity;
        }
    }
}