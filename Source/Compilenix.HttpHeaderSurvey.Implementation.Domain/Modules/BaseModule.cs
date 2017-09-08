using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    public class BaseModule<TRepository, TItem> : IBaseModule<TRepository, TItem>
        where TItem : BaseEntity where TRepository : class, IRepository<TItem>
    {
        // ReSharper disable once MemberCanBePrivate.Global

        protected readonly IRepository<TItem> Repository;

        public virtual int Count => Repository.CountAsync().Result;

        protected BaseModule(TRepository repository)
        {
            Repository = repository;
        }

        public virtual async Task<TItem> AddOrUpdateAsync(TItem item)
        {
            var existingItem = await Repository.GetAsync(item.Id);

            if (existingItem != null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return UpdateExisting(MappingUtils.Map<TItem>(existingItem, item));
            }

            return Add(item);
        }

        public virtual async Task<IEnumerable<TItem>> FindAsync(Expression<Func<TItem, bool>> predicate)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return await Repository.FindAsync(predicate);
        }

        public virtual async Task<TItem> GetAsync<TId>(TId id)
        {
            return await Repository.GetAsync(id);
        }

        public virtual async Task<IEnumerable<TItem>> GetAllAsync()
        {
            return await Repository.GetAllAsync();
        }

        public virtual void Remove(TItem item)
        {
            Repository.Remove(item);
        }

        public virtual TItem Add(TItem item)
        {
            return Repository.Add(item);
        }

        public TItem UpdateExisting(TItem item)
        {
            return Repository.UpdateExisting(item);
        }
    }
}