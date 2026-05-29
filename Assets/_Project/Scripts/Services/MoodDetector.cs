using System.Collections.Generic;
using Superglazka.Data;
using UnityEngine;

namespace Superglazka.Services
{
    public static class MoodDetector
    {
        private static readonly Dictionary<string, List<string>> Keywords = new()
        {
            { "cosmic", new() { "космос", "звезда", "планета", "галактика", "вселенная", "cosmic", "star", "planet" } },
            { "joyful", new() { "радость", "смех", "веселье", "счастье", "joy", "happy", "laugh" } },
            { "tension", new() { "напряжение", "страх", "опасность", "тревога", "tension", "fear", "danger" } },
            { "peaceful", new() { "спокойствие", "тишина", "мир", "peace", "calm", "quiet" } },
            { "magical", new() { "магия", "волшебство", "чудо", "magic", "wonder", "spell" } },
            { "triumphant", new() { "победа", "триумф", "восторг", "triumph", "victory", "cheer" } },
            { "warm", new() { "тепло", "дружба", "любовь", "warm", "friend", "love" } },
            { "mystery", new() { "тайна", "загадка", "неизвестность", "mystery", "secret", "riddle" } },
            { "epic", new() { "эпик", "легенда", "героизм", "epic", "legend", "hero" } },
            { "sad", new() { "грусть", "печаль", "слезы", "sad", "tear", "cry" } },
            { "action", new() { "действие", "бой", "погоня", "action", "fight", "chase" } },
        };

        public static MusicMood DetectMood(FrameData frame)
        {
            var text = $"{frame.narrationKey} {frame.moodHint}".ToLowerInvariant();
            var scores = new Dictionary<string, int>();

            foreach (var kv in Keywords)
            {
                int score = 0;
                foreach (var word in kv.Value)
                {
                    if (text.Contains(word.ToLowerInvariant()))
                        score++;
                }
                if (score > 0)
                    scores[kv.Key] = score;
            }

            if (scores.Count == 0) return MusicMood.Peaceful;

            string best = null;
            int bestScore = -1;
            foreach (var kv in scores)
            {
                if (kv.Value > bestScore)
                {
                    bestScore = kv.Value;
                    best = kv.Key;
                }
            }

            return best switch
            {
                "cosmic" => MusicMood.Cosmic,
                "joyful" => MusicMood.Joyful,
                "tension" => MusicMood.Tension,
                "peaceful" => MusicMood.Peaceful,
                "magical" => MusicMood.Magical,
                "triumphant" => MusicMood.Triumphant,
                "warm" => MusicMood.Warm,
                "mystery" => MusicMood.Mystery,
                "epic" => MusicMood.Epic,
                "sad" => MusicMood.Sad,
                "action" => MusicMood.Action,
                _ => MusicMood.Peaceful,
            };
        }
    }
}
