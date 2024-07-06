using Colossal;
using Colossal.IO.AssetDatabase.Internal;
using Colossal.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibShared.Localization
{
    public class AliasesLocaleSource: IDictionarySource
    {
        private readonly LocalizationManager _localizationManager;
        private readonly string _locale;
        private readonly Dictionary<string, string> _aliases = new();

        public AliasesLocaleSource(LocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
        }
        public AliasesLocaleSource(LocalizationManager localizationManager, IEnumerable<KeyValuePair<string, string>> aliases) : this(localizationManager)
        {
            Add(aliases);
        }


        public IReadOnlyDictionary<string, string> Aliases => _aliases;

        public void Add(string key, string target) => _aliases[key] = target;
        public void Add(KeyValuePair<string, string> entry) => Add(entry.Key, entry.Value);
        public void Add(IDictionary<string, string> entries) => Add(entries.AsEnumerable());
        public void Add(IEnumerable<KeyValuePair<string, string>> entries) => entries.ForEach((e) => Add(e));


        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts) =>
            Aliases.Select((a) => new KeyValuePair<string, string>(a.Key, _localizationManager.activeDictionary.TryGetValue(a.Value, out var text) ? text : a.Value));

        public void Unload() { }
    }
}
