using System;
using System.Threading.Tasks;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CompleteAsync();
        TRepository Repository<TRepository>() where TRepository : class;
    }
}