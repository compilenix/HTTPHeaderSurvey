using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IBaseModule<TRepository, TItem>
        where TItem : BaseEntity where TRepository : IRepository<TItem>
    {
        int Count { get; }

        /// <summary>
        /// Add item, without checking for existing
        /// </summary>
        TItem Add([NotNull] TItem item);

        /// <summary>
        /// Add item or update existing
        /// </summary>
        [NotNull]
        Task<TItem> AddOrUpdateAsync([NotNull] TItem item);

        [ItemNotNull]
        [NotNull]
        Task<IEnumerable<TItem>> FindAsync([NotNull] Expression<Func<TItem, bool>> predicate);

        /// <summary>
        /// Keep in mind that the result may be huge
        /// </summary>
        [ItemNotNull]
        [NotNull]
        Task<IEnumerable<TItem>> GetAllAsync();

        [NotNull]
        Task<TItem> GetAsync<TId>([NotNull] TId id);

        void Remove([NotNull] TItem item);

        TItem UpdateExisting([NotNull] TItem item);
    }
}