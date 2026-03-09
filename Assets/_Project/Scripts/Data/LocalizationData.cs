using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gryd.Data
{
    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        [TextArea] public string english;
        [TextArea] public string spanish;
    }

    /// <summary>
    /// ScriptableObject con todas las cadenas de texto del juego.
    /// Crear con clic derecho → Gryd → Localization Data.
    /// Guardar en ScriptableObjects/Settings/
    ///
    /// Claves sugeridas: "btn_play", "btn_quit", "lbl_select_level", etc.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationData", menuName = "Gryd/Localization Data")]
    public class LocalizationData : ScriptableObject
    {
        public List<LocalizationEntry> entries = new();

        private Dictionary<string, LocalizationEntry> _cache;

        public void BuildCache()
        {
            _cache = new Dictionary<string, LocalizationEntry>(entries.Count);
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                    _cache[entry.key] = entry;
            }
        }

        public bool TryGet(string key, out LocalizationEntry entry)
        {
            if (_cache == null) BuildCache();
            return _cache.TryGetValue(key, out entry);
        }
    }
}
