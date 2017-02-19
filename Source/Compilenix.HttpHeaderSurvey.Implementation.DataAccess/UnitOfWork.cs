using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.Shared.IoC;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IIoCScope _scope;
        internal readonly DataAccessContext Context;

        public UnitOfWork()
        {
            _scope = IoC.CurrentContainer.BeginScope();
            Context = _scope.Resolve<DataAccessContext>();
            Context.Configuration.AutoDetectChangesEnabled = false;
        }

        public TRepository Repository<TRepository>() where TRepository : class
        {
            return _scope.Resolve<TRepository>();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Context?.Dispose();
            _scope?.Dispose();
        }

        public async Task<int> CompleteAsync()
        {
            Context?.ChangeTracker?.DetectChanges();
            return await Context?.SaveChangesAsync();
        }
    }
}