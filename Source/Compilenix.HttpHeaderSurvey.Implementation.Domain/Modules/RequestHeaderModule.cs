using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    public class RequestHeaderModule : BaseModule<IRequestHeaderRepository, RequestHeader>, IRequestHeaderModule
    {
        private readonly IRequestHeaderRepository _repository;

        public RequestHeaderModule(IRequestHeaderRepository repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>Add item or update existing</summary>
        public override async Task<RequestHeader> AddOrUpdateAsync(RequestHeader item)
        {
            var result = await _repository.AddIfNotExistingAsync(item);

            if (result == null)
            {
                return await _repository.GetAsync(item.Id);
            }

            return result;
        }

        public async Task<IEnumerable<RequestHeader>> GetDefaultRequestHeadersAsync()
        {
            var headers = new List<RequestHeader> { (await _repository.GetByHeaderAsync("User-Agent")).First(), (await _repository.GetByHeaderAsync("accept-encoding")).First(), (await _repository.GetByHeaderAsync("accept-language")).First(), (await _repository.GetByHeaderAsync("upgrade-insecure-requests")).First() };

            return headers;
        }
    }
}