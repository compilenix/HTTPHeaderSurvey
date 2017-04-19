using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    public interface IErrorCodeRepository : IRepository<ErrorCode>
    {
        [NotNull]
        Task<ErrorCode> AddIfNotExistingAsync([NotNull] ErrorCode code);
    }
}