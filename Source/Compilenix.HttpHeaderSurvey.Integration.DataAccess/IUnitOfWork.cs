using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IUnitOfWork : IDisposable
    {
        bool SaveChanges { get; set; }

        Task<int> CompleteAsync();

        TRepository Resolve<TRepository>()
            where TRepository : class;
    }
}