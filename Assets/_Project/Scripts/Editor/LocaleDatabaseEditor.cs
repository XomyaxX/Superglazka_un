#if UNITY_EDITOR
using Superglazka.Data;
using UnityEditor;
using UnityEngine;

namespace Superglazka.Editor
{
    [CustomEditor(typeof(LocaleDatabase))]
    public class LocaleDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Import from JSON"))
            {
                var db = (LocaleDatabase)target;
                string path = EditorUtility.OpenFilePanel("Import Locale JSON", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    // Parse and populate entries
                }
            }
        }
    }
}
#endif
