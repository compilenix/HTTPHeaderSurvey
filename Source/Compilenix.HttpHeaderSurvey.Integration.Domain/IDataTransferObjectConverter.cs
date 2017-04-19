using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IDataTransferObjectConverter
    {
        [ItemNotNull]
        [NotNull]
        Task<IEnumerable<RequestJob>> RequestJobsFromCsv([NotNull] string filePath, char seperator);
    }
}