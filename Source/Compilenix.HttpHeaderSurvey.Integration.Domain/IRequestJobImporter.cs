using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IRequestJobImporter : IDisposable
    {
        [NotNull]
        Task FromCsvAsync([NotNull] string filePath, char delimiter = ',');

        [NotNull]
        Task FromCsvAsync([NotNull] string filePath, [ItemNotNull] [NotNull] IEnumerable<RequestHeader> requestHeaders, char delimiter = ',');

        [NotNull]
        Task ImportAsync([NotNull] RequestJob requestJob, [NotNull] IEnumerable<RequestHeader> headers);
    }
}