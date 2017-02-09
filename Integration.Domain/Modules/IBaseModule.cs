using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Integration.Domain.Modules
{
    public interface IBaseModule<TRepository, TItem> : IDisposable
        where TItem : BaseEntity where TRepository : IRepository<TItem>
    {
        bool SaveChanges { get; set; }

        int Count { get; }

        /// <summary>
        /// Add item, without checking for existing
        /// </summary>
        TItem Add(TItem item);

        /// <summary>
        /// Add item or update existing
        /// </summary>
        TItem AddOrUpdate(TItem item);

        TItem Update(TItem item);

        IEnumerable<TItem> Find(Expression<Func<TItem, bool>> predicate);

        TItem Get<TId>(TId id);

        /// <summary>
        /// Keep in mind that the result may be huge
        /// </summary>
        IEnumerable<TItem> GetAll();

        void Remove(TItem item);
    }
}