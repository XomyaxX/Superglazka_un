using System.Collections.Generic;
using UnityEngine;

namespace Superglazka.Data
{
    [CreateAssetMenu(fileName = "LocaleDatabase", menuName = "Superglazka/Locale Database")]
    public class LocaleDatabase : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string key;
            [TextArea] public string ru;
            [TextArea] public string en;
            [TextArea] public string kz;
            [TextArea] public string zh;
        }

        [SerializeField] private List<Entry> _entries;

        public List<Entry> Entries => _entries;
    }
}
