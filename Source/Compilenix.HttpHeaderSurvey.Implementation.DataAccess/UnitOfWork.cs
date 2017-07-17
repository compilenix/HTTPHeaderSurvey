using System.Diagnostics;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.Shared.IoC;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess
{
    [UsedImplicitly]
    public class UnitOfWork : IUnitOfWork
    {
        [NotNull]
        private readonly IIoCScope _scope;

        [NotNull]
        internal readonly DataAccessContext Context;

        public bool SaveChanges { get; set; } = true;

        [DebuggerStepThrough]
        public UnitOfWork()
        {
            _scope = IoC.BeginLifetimeScope();
            Context = _scope.Resolve<DataAccessContext>();
            // ReSharper disable once PossibleNullReferenceException
            Context.Configuration.AutoDetectChangesEnabled = false;
        }

        [DebuggerStepThrough]
        public T Resolve<T>()
            where T : class
        {
            return _scope.Resolve<T>();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (SaveChanges)
            {
                CompleteAsync().Wait();
            }

            Context.Dispose();
            _scope.Dispose();
        }

        public async Task<int> CompleteAsync()
        {
            Context.ChangeTracker?.DetectChanges();
            // ReSharper disable once PossibleNullReferenceException
            return await Context.SaveChangesAsync();
        }
    }
}