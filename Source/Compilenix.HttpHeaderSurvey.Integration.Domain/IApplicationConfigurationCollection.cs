using System;
using System.Collections;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    public interface IApplicationConfigurationCollection : ICollection
    {
        bool Exists(string key);
        string Get(string key);
        Tuple<bool, string, string> Remove(string key);
        Tuple<string, string> SetOrAdd(string key, string value);
    }
}