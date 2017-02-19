﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
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
        Task<TItem> AddOrUpdateAsync(TItem item);

        Task<IEnumerable<TItem>> FindAsync(Expression<Func<TItem, bool>> predicate);

        /// <summary>
        /// Keep in mind that the result may be huge
        /// </summary>
        Task<IEnumerable<TItem>> GetAllAsync();

        Task<TItem> GetAsync<TId>(TId id);

        void Remove(TItem item);

        TItem Update(TItem item);
    }
}