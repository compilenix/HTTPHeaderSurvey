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
        public ResponseHeaderModule(IResponseHeaderRepository repository) : base(repository)
        {
        }

        public async Task<List<ResponseHeader>> GetResponseHeadersFromListAsync(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerList, IUnitOfWork unit)
        {
            var existingHeaders = new List<ResponseHeader>();
            var newHeaders = new List<ResponseHeader>();
            var responseHeaderRepository = unit.Resolve<IResponseHeaderRepository>();

            foreach (var header in headerList)
            {
                if (header.Key == null || header.Value == null)
                {
                    continue;
                }

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
                    var newAddedHeader = await responseHeaderRepository.AddIfNotExistingAsync(tmpHeader);

                    if (newAddedHeader == null)
                    {
                        existingHeaders.Add(tmpHeader);
                    }
                    else
                    {
                        newHeaders.Add(newAddedHeader);
                    }
                }
            }

            var headers = new List<ResponseHeader>();
            foreach (var existingHeader in existingHeaders)
            {
                if (existingHeader.Key == null || existingHeader.Value == null)
                {
                    continue;
                }

                headers.Add(await responseHeaderRepository.GetByHeaderAndValueAsync(existingHeader.Key, existingHeader.Value));
            }

            headers.AddRange(newHeaders);
            return headers.Distinct().Where(h => h != null).ToList();
        }
    }
}