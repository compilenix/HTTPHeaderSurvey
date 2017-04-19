using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public interface IApplicationConfigurationCollection : ICollection
    {
        bool Exists([NotNull] string key);
        string Get([NotNull] string key);

        [NotNull]
        Tuple<bool, string, string> Remove([NotNull] string key);

        [NotNull]
        Tuple<string, string> SetOrAdd([NotNull] string key, [NotNull] string value);
    }
}