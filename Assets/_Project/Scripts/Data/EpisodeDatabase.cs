using System.Collections.Generic;
using UnityEngine;

namespace Superglazka.Data
{
    [CreateAssetMenu(fileName = "EpisodeDatabase", menuName = "Superglazka/Episode Database")]
    public class EpisodeDatabase : ScriptableObject
    {
        [SerializeField] private List<BookData> _books;
        public IReadOnlyList<BookData> Books => _books;

        public EpisodeData GetEpisode(string id)
        {
            foreach (var book in _books)
            {
                foreach (var ep in book.episodes)
                {
                    if (ep.id == id) return ep;
                }
            }
            return null;
        }

        public BookData GetBook(string id)
        {
            foreach (var book in _books)
                if (book.id == id) return book;
            return null;
        }
    }

    [System.Serializable]
    public class BookData
    {
        public string id;
        public string titleKey;
        public string subtitleKey;
        public Sprite coverSprite;
        public List<EpisodeData> episodes;
    }
}
