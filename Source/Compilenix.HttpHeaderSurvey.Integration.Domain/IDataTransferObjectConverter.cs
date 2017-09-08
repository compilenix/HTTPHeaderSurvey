using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IDataTransferObjectConverter
    {
        Task<IEnumerable<RequestJob>> RequestJobsFromCsv(string filePath, char seperator);
    }
}