using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;


namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    
    public class RequestJobModule : BaseModule<IRequestJobRepository, RequestJob>, IRequestJobModule
    {
        
        private readonly IRequestJobRepository _repository;

        public RequestJobModule( IRequestJobRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<bool> ContainsRequestJobAsync(string method, string uri)
        {
            return await _repository.ContainsRequestJobAsync(method, uri);
        }
    }
}