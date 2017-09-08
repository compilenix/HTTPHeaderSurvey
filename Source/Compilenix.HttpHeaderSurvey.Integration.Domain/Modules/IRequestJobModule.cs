using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;


namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IRequestJobModule : IBaseModule<IRequestJobRepository, RequestJob>
    {
        
        Task<bool> ContainsRequestJobAsync( string method,  string uri);
    }
}