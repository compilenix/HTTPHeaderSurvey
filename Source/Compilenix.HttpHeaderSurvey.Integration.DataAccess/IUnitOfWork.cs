using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IUnitOfWork : IDisposable
    {
        bool SaveChanges { get; set; }

        [NotNull]
        Task<int> CompleteAsync();

        [NotNull]
        TRepository Resolve<TRepository>()
            where TRepository : class;
    }
}