using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    public class ResponseHeaderModule : BaseModule<IResponseHeaderRepository, ResponseHeader>, IResponseHeaderModule
    {
        private IResponseHeaderRepository _repository;

        public ResponseHeaderModule(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _repository = unitOfWork.Repository<IResponseHeaderRepository>();
        }

        public async Task<List<ResponseHeader>> GetResponseHeadersFromListAsync(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerList,
                                                                                IUnitOfWork unit)
        {
            var headersFromResponse = new List<ResponseHeader>();
            var addedHeaders = new List<ResponseHeader>();
            var headers = new List<ResponseHeader>();

            foreach (var header in headerList)
            {
                // "fixing" bug in .net httpclient lib, where server values get split into multiple strings by whitespaces.
                var headerValues = new List<string>();
                if (header.Key.ToLower() == "server")
                {
                    headerValues.Add(string.Join(" ", header.Value));
                }
                else
                {
                    headerValues.AddRange(header.Value);
                }

                foreach (var headerValue in headerValues)
                {
                    var tmpHeader = new ResponseHeader { Key = header.Key, Value = headerValue };
                    headersFromResponse.Add(tmpHeader);
                    addedHeaders.Add(await _repository.AddIfNotExistingAsync(tmpHeader));
                }
            }

            addedHeaders = addedHeaders.Where(h => h != null).ToList();
            headersFromResponse.RemoveAll(header => addedHeaders.Contains(header));

            foreach (var responseHeader in headersFromResponse)
            {
                if (responseHeader != null)
                {
                    headers.Add(await _repository.GetByHeaderAndValueAsync(responseHeader.Key, responseHeader.Value));
                }
            }

            headers.AddRange(addedHeaders);
            headers = headers.Where(h => h != null).Distinct().ToList();
            return headers;
        }
    }
}