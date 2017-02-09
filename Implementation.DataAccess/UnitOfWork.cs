using System.Threading.Tasks;
using Implementation.Shared.IoC;
using Integration.DataAccess;
using Integration.Shared.IoC;

namespace Implementation.DataAccess
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

        public int Complete()
        {
            return CompleteAsync().Result;
        }
    }
}