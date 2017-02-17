using System.Collections.Generic;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    public interface IDataTransferObjectConverter
    {
        Task<IEnumerable<RequestJob>> RequestJobsFromCsv(string filePath, char seperator);
    }
}