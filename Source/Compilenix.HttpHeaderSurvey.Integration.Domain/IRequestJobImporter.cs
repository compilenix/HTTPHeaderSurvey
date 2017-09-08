using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IRequestJobImporter : IDisposable
    {
        
        Task FromCsvAsync( string filePath, char delimiter = ',');

        
        Task FromCsvAsync( string filePath,   IEnumerable<RequestHeader> requestHeaders, char delimiter = ',');

        
        Task ImportAsync( RequestJob requestJob,  IEnumerable<RequestHeader> headers);
    }
}