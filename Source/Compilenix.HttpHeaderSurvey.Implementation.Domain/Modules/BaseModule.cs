using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    public class BaseModule<TRepository, TItem> : IBaseModule<TRepository, TItem>
        where TItem : BaseEntity where TRepository : class, IRepository<TItem>
    {
        protected IRepository<TItem> BaseEntityRepository;
        public bool SaveChanges { get; set; }
        public virtual int Count => BaseEntityRepository.CountAsync().Result;
        protected IUnitOfWork UnitOfWork { get; }

        public BaseModule(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
            BaseEntityRepository = UnitOfWork.Repository<TRepository>();
            SaveChanges = true;
        }

        public virtual async Task<TItem> AddOrUpdateAsync(TItem item)
        {
            var existingItem = await BaseEntityRepository.GetAsync(item.Id);

            if (existingItem != null)
            {
                return Update(MappingUtils.Map<TItem>(existingItem, item));
            }

            return Add(item);
        }

        public virtual async Task<IEnumerable<TItem>> FindAsync(Expression<Func<TItem, bool>> predicate)
        {
            return await BaseEntityRepository.FindAsync(predicate);
        }

        public virtual async Task<TItem> GetAsync<TId>(TId id)
        {
            return await BaseEntityRepository.GetAsync(id);
        }

        public virtual async Task<IEnumerable<TItem>> GetAllAsync()
        {
            return await BaseEntityRepository.GetAllAsync();
        }

        public virtual void Remove(TItem item)
        {
            BaseEntityRepository.Remove(item);
        }

        // TODO mybe move UnitOfWork.Complete() into function
        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            SaveAsync(UnitOfWork).Wait();
            UnitOfWork?.Dispose();
        }

        public virtual TItem Add(TItem item)
        {
            return BaseEntityRepository.Add(item);
        }

        public TItem Update(TItem item)
        {
            return BaseEntityRepository.Update(item);
        }

        protected async Task SaveAsync(IUnitOfWork unit)
        {
            if (SaveChanges)
            {
                await unit?.CompleteAsync();
            }
        }
    }
}