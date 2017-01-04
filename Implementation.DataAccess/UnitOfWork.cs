﻿using System;
using Implementation.DataAccess.Repositories;
using Implementation.Shared;
using Integration.DataAccess;
using Integration.DataAccess.Repositories;

namespace Implementation.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        internal readonly DataAccessContext Context;

        public IRequestJobRepository RequestJobs { get; private set; }
        public IRequestHeaderRepository RequestHeaders { get; private set; }

        public UnitOfWork()
        {
            Context = new DataAccessContext();
            Context.Configuration.AutoDetectChangesEnabled = false;
            RequestJobs = new RequestJobRepository(Context, scheduleDays: 10);
            RequestHeaders = new RequestHeaderRepository(Context);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Context?.Dispose();
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