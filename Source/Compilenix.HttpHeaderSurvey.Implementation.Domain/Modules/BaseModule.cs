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
        public bool SaveChanges { get; set; }

        public virtual int Count => UnitOfWork.Repository<TRepository>().Count();

        protected IUnitOfWork UnitOfWork { get; }

        public BaseModule(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
            SaveChanges = true;
        }

        public virtual TItem AddOrUpdate(TItem item)
        {
            var existingItem = UnitOfWork.Repository<TRepository>().Get(item.Id);

            if (existingItem != null)
            {
                return Update(MappingUtils.Map<TItem>(existingItem, item));
            }

            return Add(item);
        }

        public virtual IEnumerable<TItem> Find(Expression<Func<TItem, bool>> predicate)
        {
            return UnitOfWork.Repository<TRepository>().Find(predicate);
        }

        public virtual TItem Get<TId>(TId id)
        {
            return UnitOfWork.Repository<TRepository>().Get(id);
        }

        public virtual IEnumerable<TItem> GetAll()
        {
            return UnitOfWork.Repository<TRepository>().GetAll();
        }

        public virtual void Remove(TItem item)
        {
            UnitOfWork.Repository<TRepository>().Remove(item);
        }

        // TODO mybe move UnitOfWork.Complete() into function
        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            SaveUnitOfWork(UnitOfWork).Wait();
            UnitOfWork?.Dispose();
        }

        public virtual TItem Add(TItem item)
        {
            return UnitOfWork.Repository<TRepository>().Add(item);
        }

        public TItem Update(TItem item)
        {
            return UnitOfWork.Repository<TRepository>().Update(item);
        }

        protected async Task SaveUnitOfWork(IUnitOfWork unit)
        {
            if (SaveChanges)
            {
                await unit?.CompleteAsync();
            }
        }
    }
}