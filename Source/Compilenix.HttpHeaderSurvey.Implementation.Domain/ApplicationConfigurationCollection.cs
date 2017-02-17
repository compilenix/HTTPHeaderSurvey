using System;
using System.Collections;
using System.Configuration;
using Compilenix.HttpHeaderSurvey.Integration.Domain;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain
{
    public class ApplicationConfigurationCollection : IApplicationConfigurationCollection
    {
        /// <summary>
        /// Gets / Sets / Updates a given configuration value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>the current value (after the process)</returns>
        public string this[string key]
        {
            get { return Get(key); }
            set { SetOrAdd(key, value); }
        }

        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection" />.</returns>
        public int Count => ConfigurationManager.AppSettings.Count;

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
        public object SyncRoot => new NotImplementedException();

        /// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
        public bool IsSynchronized => false;

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        /// <filterpriority>2</filterpriority>
        public IEnumerator GetEnumerator()
        {
            yield return ConfigurationManager.AppSettings;
        }

        /// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
        public void CopyTo(Array array, int index)
        {
            ConfigurationManager.AppSettings.CopyTo(array, index);
        }

        /// <summary>
        /// Checks if a given key exists and is not null or empty.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(Get(key))) return true;
            }
            catch
            {
                // ignored
            }
            return false;
        }

        /// <summary>
        /// Gets a given configuration value.
        /// This does NOT check if the key exists, is null or empty.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>value</returns>
        public string Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Removes a given key and it's value permanently from the configuration.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>if the operation succeded, the key and the current value (before the process)</returns>
        public Tuple<bool, string, string> Remove(string key)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            var value = string.Empty;
            var removed = false;
            try
            {
                value = Get(key);
                settings.Remove(key);
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                removed = true;
            }
            catch
            {
                // ignored
            }
            return new Tuple<bool, string, string>(removed, key, value);
        }

        /// <summary>
        /// Sets / Updates a given configuration value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>the key and the current value (before the process)</returns>
        public Tuple<string, string> SetOrAdd(string key, string value)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if (settings[key] == null)
            {
                settings.Add(key, value);
            }
            else
            {
                settings[key].Value = value;
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            return new Tuple<string, string>(key, value);
        }
    }
}