using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Superglazka.Services
{
    public class SaveSystem : MonoBehaviour
    {
        private string SavePath => Path.Combine(Application.persistentDataPath, "superglazka_save.json");
        private Dictionary<string, string> _data = new();
        private bool _dirty;

        public static SaveSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        private void Update()
        {
            if (_dirty)
            {
                _dirty = false;
                Flush();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && _dirty) Flush();
        }

        private void OnApplicationQuit()
        {
            if (_dirty) Flush();
        }

        public void SetString(string key, string value)
        {
            _data[key] = value;
            _dirty = true;
        }

        public string GetString(string key, string defaultValue = "")
        {
            return _data.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void SetInt(string key, int value)
        {
            _data[key] = value.ToString();
            _dirty = true;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (_data.TryGetValue(key, out var str) && int.TryParse(str, out var val))
                return val;
            return defaultValue;
        }

        public void SetFloat(string key, float value)
        {
            _data[key] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _dirty = true;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (_data.TryGetValue(key, out var str) && float.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var val))
                return val;
            return defaultValue;
        }

        public void SetBool(string key, bool value)
        {
            _data[key] = value ? "1" : "0";
            _dirty = true;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_data.TryGetValue(key, out var str))
                return str == "1";
            return defaultValue;
        }

        public void SetJson(string key, object value)
        {
            _data[key] = JsonUtility.ToJson(value);
            _dirty = true;
        }

        public T GetJson<T>(string key, T defaultValue = default) where T : class
        {
            if (_data.TryGetValue(key, out var json))
            {
                try { return JsonUtility.FromJson<T>(json); }
                catch { return defaultValue; }
            }
            return defaultValue;
        }

        public bool HasKey(string key) => _data.ContainsKey(key);
        public void DeleteKey(string key) { _data.Remove(key); _dirty = true; }

        public void Flush()
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("{");
                bool first = true;
                foreach (var kv in _data)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append($"\"{Escape(kv.Key)}\":\"{Escape(kv.Value)}\"");
                }
                sb.Append("}");
                File.WriteAllText(SavePath, sb.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveSystem flush failed: {e.Message}");
            }
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return;
                var json = File.ReadAllText(SavePath);
                var parsed = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
                if (parsed != null)
                {
                    _data.Clear();
                    foreach (var kv in parsed)
                        _data[kv.Key] = kv.Value?.ToString() ?? "";
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveSystem load failed: {e.Message}");
            }
        }

        private static string Escape(string s)
        {
            return s?.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") ?? "";
        }
    }
}
