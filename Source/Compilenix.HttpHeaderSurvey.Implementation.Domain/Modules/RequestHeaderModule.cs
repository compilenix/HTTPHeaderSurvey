using System.Collections.Generic;
using System.Linq;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    public class RequestHeaderModule : BaseModule<IRequestHeaderRepository, RequestHeader>, IRequestHeaderModule
    {
        public RequestHeaderModule(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>Add item or update existing</summary>
        public override RequestHeader AddOrUpdate(RequestHeader item)
        {
            return UnitOfWork.Repository<IRequestHeaderRepository>().AddIfNotExisting(item);
        }

        public List<ResponseHeader> GetResponseHeadersFromList(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerList, IUnitOfWork unit)
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
                    addedHeaders.Add(unit.Repository<IResponseHeaderRepository>().AddIfNotExisting(tmpHeader));
                }
            }

            addedHeaders = addedHeaders.Where(h => h != null).ToList();
            headersFromResponse.RemoveAll(header => addedHeaders.Contains(header));

            foreach (var responseHeader in headersFromResponse)
            {
                if (responseHeader != null)
                {
                    headers.Add(unit.Repository<IResponseHeaderRepository>().GetByHeaderAndValue(responseHeader.Key, responseHeader.Value));
                }
            }

            headers.AddRange(addedHeaders);
            headers = headers.Where(h => h != null).Distinct().ToList();
            return headers;
        }

        public IEnumerable<RequestHeader> GetDefaultRequestHeaders()
        {
            var headers = new List<RequestHeader>();
            var headerRepository = UnitOfWork.Repository<IRequestHeaderRepository>();

            headers.Add(headerRepository.GetByHeader("User-Agent").First());
            headers.Add(headerRepository.GetByHeader("accept-encoding").First());
            headers.Add(headerRepository.GetByHeader("accept-language").First());
            headers.Add(headerRepository.GetByHeader("upgrade-insecure-requests").First());

            return headers;
        }
    }
}