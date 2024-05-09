/* Copyright 2024 zakuro <z@kuro.red> (https://x.com/zakuro9715)
* 
* This file is part of 
* 
* LocaleHelper is free software: you can redistribute it and/or modify it under the
* terms of the GNU General Public License as published by the Free Software Foundation, either
* version 3 of the License, or (at your option) any later version.
* 
* LocaleHelper is distributed in the hope that it will be useful, but WITHOUT ANY
* WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
* PURPOSE. See the GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License along with
* LocaleHelper. If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Text;
using Colossal;
using Colossal.Json;
using System.IO;
using System.Linq;
using System.Reflection;
using Colossal.Localization;
using Colossal.Logging;

namespace LibShared.Localization
{

    public class LocaleLoadException : Exception
    {
        public LocaleLoadException() : base("Locale loading error") { }
        public LocaleLoadException(string message) : base(message) { }
    }

    public class LocaleNotFoundException : LocaleLoadException
    {
        public LocaleNotFoundException(string name) : base($"Locale resource `{name}` not found") { }
    }

    public class LocaleLoader
    {
        private readonly Assembly _assembly;
        private readonly ILog _log;
        public Dictionary<string, LocaleDictionarySource> Locales { get; private set; }
        public LocaleLoader(ILog log) : this(log, Assembly.GetExecutingAssembly())
        {
        }

        public LocaleLoader(ILog log, Assembly assembly)
        {
            _assembly = assembly;
            _log = log;
        }

        private string Message(string message) => $"LocaleLoader: {message}";

        private IEnumerable<string> getLocaleResourceNames() =>
            _assembly.GetManifestResourceNames().Where((name) => name.Contains("Locales") && name.EndsWith(".json"));

        public Dictionary<string, LocaleDictionarySource> LoadAll()
        {
            Locales = new Dictionary<string, LocaleDictionarySource>();
            foreach (var localeResourceName in getLocaleResourceNames())
            {
                var localeName = Path.GetFileNameWithoutExtension(localeResourceName);
                localeName = localeName.Substring(localeName.LastIndexOf('.') + 1);

                using var resourceStream = _assembly.GetManifestResourceStream(localeResourceName) ?? throw new LocaleNotFoundException(localeResourceName);
                using var reader = new StreamReader(resourceStream, Encoding.UTF8);
                JSON.MakeInto<Dictionary<string, string>>(JSON.Load(reader.ReadToEnd()), out var dictionary);
                Locales.Add(localeName, new LocaleDictionarySource(localeName, dictionary));

                _log.Info(Message($"Load {localeName}"));
            }
            return Locales;
        }

        public void LoadToManager(LocalizationManager manager)
        {
            foreach (var locale in LoadAll())
            {
                manager.AddSource(locale.Key, locale.Value);
                _log.Info(Message($"Added {locale.Key}"));
            }
        }

        public static void Load(ILog log, LocalizationManager manager)
        {
            new Loader(log).LoadToManager(manager);
        }
    }

    public class LocaleDictionarySource : IDictionarySource
    {
        private readonly Dictionary<string, string> _dictionary;

        public LocaleDictionarySource(string localeId, Dictionary<string, string> dictionary)
        {
            LocaleId = localeId;
            _dictionary = dictionary;
        }

        public string LocaleId { get; }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return _dictionary;
        }

        public void Unload() { }
    }
}
