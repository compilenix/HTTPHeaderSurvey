using System.Collections.Generic;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    public interface IRequestJobModule : IBaseModule<IRequestJobRepository, RequestJob>
    {
        Task ImportFromCsvAsync(string filePath, char delimiter = ',');
        Task ImportFromCsvAsync(string filePath, IEnumerable<RequestHeader> requestHeaders, char delimiter = ',');
    }
}