using System;
using Implementation.Shared;
using Implementation.Shared.IoC;
using Integration.DataAccess;
using Integration.Shared.IoC;

namespace Implementation.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        internal readonly DataAccessContext Context;
        private readonly IIoCScope _scope;

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

        public int Complete()
        {
            try
            {
                Context.ChangeTracker.DetectChanges();
                return Context.SaveChanges();
            }
            catch (Exception exception)
            {
                this.Log()?.Error("Error completing unit", exception);
                throw;
            }
        }
    }
}