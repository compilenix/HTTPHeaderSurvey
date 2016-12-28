using Implementation.DataAccess.Repositories;
using Integration.DataAccess;
using Integration.DataAccess.Repositories;

namespace Implementation.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HttpHeaderDbContext _context;

        public UnitOfWork(HttpHeaderDbContext context)
        {
            _context = context;
            RequestJobs = new RequestJobRepository(_context);
            RequestHeaders = new RequestHeaderRepository(_context);
        }

        public IRequestJobRepository RequestJobs { get; private set; }
        public IRequestHeaderRepository RequestHeaders { get; private set; }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _context?.Dispose();
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }
    }
}
