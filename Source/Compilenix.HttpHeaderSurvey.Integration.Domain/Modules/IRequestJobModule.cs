using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IRequestJobModule : IBaseModule<IRequestJobRepository, RequestJob>
    {
        [NotNull]
        Task<bool> ContainsRequestJobAsync([NotNull] string method, [NotNull] string uri);
    }
}