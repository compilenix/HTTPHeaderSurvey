using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    public interface IErrorCodeRepository : IRepository<ErrorCode>
    {
        Task<ErrorCode> AddIfNotExistingAsync(ErrorCode code);
    }
}