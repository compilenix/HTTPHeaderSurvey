using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    [UsedImplicitly]
    public class RequestJobModule : BaseModule<IRequestJobRepository, RequestJob>, IRequestJobModule
    {
        [NotNull]
        private readonly IRequestJobRepository _repository;

        public RequestJobModule([NotNull] IRequestJobRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<bool> ContainsRequestJobAsync(string method, string uri)
        {
            return await _repository.ContainsRequestJobAsync(method, uri);
        }
    }
}