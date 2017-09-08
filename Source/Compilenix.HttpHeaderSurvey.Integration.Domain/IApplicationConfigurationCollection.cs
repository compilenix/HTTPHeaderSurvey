using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public interface IApplicationConfigurationCollection : ICollection
    {
        bool Exists(string key);
        string Get(string key);

        Tuple<bool, string, string> Remove(string key);

        Tuple<string, string> SetOrAdd(string key, string value);
    }
}