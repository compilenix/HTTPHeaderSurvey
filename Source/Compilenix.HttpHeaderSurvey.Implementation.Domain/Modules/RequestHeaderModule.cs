using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    public class RequestHeaderModule : BaseModule<IRequestHeaderRepository, RequestHeader>, IRequestHeaderModule
    {
        private readonly IRequestHeaderRepository _repository;

        public RequestHeaderModule(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _repository = unitOfWork.Repository<IRequestHeaderRepository>();
        }

        /// <summary>Add item or update existing</summary>
        public override async Task<RequestHeader> AddOrUpdateAsync(RequestHeader item)
        {
            return await _repository.AddIfNotExistingAsync(item);
        }

        public async Task<IEnumerable<RequestHeader>> GetDefaultRequestHeadersAsync()
        {
            var headers = new List<RequestHeader>();
            var headerRepository = UnitOfWork.Repository<IRequestHeaderRepository>();

            headers.Add((await _repository.GetByHeaderAsync("User-Agent")).First());
            headers.Add((await _repository.GetByHeaderAsync("accept-encoding")).First());
            headers.Add((await _repository.GetByHeaderAsync("accept-language")).First());
            headers.Add((await _repository.GetByHeaderAsync("upgrade-insecure-requests")).First());

            return headers;
        }
    }
}