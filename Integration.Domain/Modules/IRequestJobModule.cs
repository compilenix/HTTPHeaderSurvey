using System.Collections.Generic;
using System.Threading.Tasks;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Integration.Domain.Modules
{
    public interface IRequestJobModule : IBaseModule<IRequestJobRepository, RequestJob>
    {
        Task ImportFromCsv(string filePath, char delimiter = ',');
        Task ImportFromCsv(string filePath, IEnumerable<RequestHeader> requestHeaders, char delimiter = ',');
        Task ProcessPendingJobs(int countOfJobsToProcess);
    }
}
