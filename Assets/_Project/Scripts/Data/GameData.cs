using System;
using System.Collections.Generic;
using UnityEngine;

namespace Superglazka.Data
{
    [Serializable]
    public class EpisodeData
    {
        public string id;
        public string bookId;
        public string titleKey;
        public string descriptionKey;
        public Sprite coverSprite;
        public List<FrameData> frames;
        public bool isLocked;
        public int requiredCoins;
    }

    [Serializable]
    public class FrameData
    {
        public string id;
        public Sprite bgImage;
        public Sprite bgImageMobile;
        public VideoClipRef videoClip;
        public VideoClipRef videoClipMobile;
        public AudioClip narrationAudio;
        public string narrationKey;
        public List<DialogueData> dialogues;
        public List<ChoiceData> choices;
        public string gameId;
        public string transitionTextKey;
        public string moodHint;
    }

    [Serializable]
    public class DialogueData
    {
        public string speakerKey;
        public string textKey;
        public float startTime;
    }

    [Serializable]
    public class ChoiceData
    {
        public string textKey;
        public int nextFrameIndex;
    }

    [Serializable]
    public class VideoClipRef
    {
        public string addressableKey;
    }

    [Serializable]
    public class PlayerProfileData
    {
        public string nickname;
        public int coins;
        public Dictionary<string, EpisodeProgress> episodes = new();
        public Dictionary<string, GameStats> games = new();
    }

    [Serializable]
    public class EpisodeProgress
    {
        public bool completed;
        public int framesSeen;
        public int maxFrame;
    }

    [Serializable]
    public class GameStats
    {
        public int played;
        public int bestScore;
    }

    [Serializable]
    public class AchievementData
    {
        public string key;
        public string titleKey;
        public string descKey;
        public Sprite icon;
        public AchievementType type;
        public int targetValue;
        public int rewardCoins;
    }

    public enum AchievementType
    {
        Frame, Game, GameCount, Episode, EpisodeAll, Coins
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string name;
        public int score;
        public string date;
    }
}
