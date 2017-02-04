﻿using System;
using System.Threading.Tasks;

namespace Integration.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        int Complete();
        Task<int> CompleteAsync();
        TRepository Repository<TRepository>() where TRepository : class;
    }
}