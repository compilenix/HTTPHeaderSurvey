using System.Collections.Generic;
using System.Threading.Tasks;
using Integration.DataAccess.Entitys;

namespace Integration.Domain
{
    public interface IDataTransferObjectConverter
    {
        Task<IEnumerable<RequestJob>> RequestJobsFromCsv(string filePath, char seperator);
    }
}