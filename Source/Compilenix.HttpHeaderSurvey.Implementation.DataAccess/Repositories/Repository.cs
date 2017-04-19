using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        [NotNull]
        protected readonly DataAccessContext Context;

        public IQueryable<TEntity> EntitiesAsQueryable => Entities.AsQueryable();

        [NotNull]
        // ReSharper disable once AssignNullToNotNullAttribute
        protected DbSet<TEntity> Entities => Context.Set<TEntity>();

        protected Repository([NotNull] DataAccessContext context)
        {
            Context = context;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var listAsync = Entities.ToListAsync();
            if (listAsync != null)
            {
                return await listAsync ?? new List<TEntity>();
            }
            return new List<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var listAsync = Entities.Where(predicate).ToListAsync();
            if (listAsync != null)
            {
                return await listAsync ?? new List<TEntity>();
            }
            return new List<TEntity>();
        }

        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var singleOrDefaultAsync = Entities.SingleOrDefaultAsync(predicate);
            if (singleOrDefaultAsync != null)
            {
                return await singleOrDefaultAsync;
            }
            return default(TEntity);
        }

        public virtual TEntity Add(TEntity entity)
        {
            entity.DateCreated = DateTime.Now;
            return Entities.Add(entity);
        }

        public void Remove(TEntity entity)
        {
            Entities.Remove(entity);
        }

        public async Task<TEntity> GetAsync<TTarget>(TTarget id)
        {
            var findAsync = Entities.FindAsync(id);
            if (findAsync != null)
            {
                return await findAsync;
            }
            return default(TEntity);
        }

        public async Task<int> CountAsync()
        {
            var countAsync = Entities.CountAsync();
            if (countAsync != null)
            {
                return await countAsync;
            }
            return default(int);
        }

        public TEntity UpdateExisting(TEntity entity)
        {
            Entities.Attach(entity);
            var dbEntityEntry = Context.Entry(entity);
            if (dbEntityEntry != null)
            {
                dbEntityEntry.State = EntityState.Modified;
            }

            return entity;
        }
    }
}