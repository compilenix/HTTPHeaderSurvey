using System.Collections.Generic;
using Integration.DataAccess.Entitys;

namespace Integration.Domain
{
    public interface IDataTransferObjectConverter
    {
        IEnumerable<RequestJob> RequestJobsFromCsv(string filePath, char seperator);
    }
}