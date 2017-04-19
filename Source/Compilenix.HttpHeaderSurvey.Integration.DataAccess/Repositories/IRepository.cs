using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IRepository<TEntity>
        where TEntity : BaseEntity
    {
        [NotNull]
        IQueryable<TEntity> EntitiesAsQueryable { get; }

        TEntity Add([NotNull] TEntity entity);

        [NotNull]
        Task<int> CountAsync();

        [NotNull]
        Task<IEnumerable<TEntity>> FindAsync([NotNull] Expression<Func<TEntity, bool>> predicate);

        [ItemNotNull]
        [NotNull]
        Task<IEnumerable<TEntity>> GetAllAsync();

        [NotNull]
        Task<TEntity> GetAsync<TTarget>([NotNull] TTarget id);

        void Remove([NotNull] TEntity entity);

        Task<TEntity> SingleOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate);

        TEntity UpdateExisting([NotNull] TEntity entity);
    }
}