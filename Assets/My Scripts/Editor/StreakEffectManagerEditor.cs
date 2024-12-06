using UnityEngine;
using UnityEditor;

namespace MyScripts
{
    [CustomEditor(typeof(StreakEffectsManager))]
    public class StreakEffectsManagerEditor : Editor
    {
        private bool showPerformanceSettings = true;
        private bool[] showStreakSettings = new bool[10];
        private bool showDefaultSettings = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            StreakEffectsManager manager = (StreakEffectsManager)target;

            // Performance Settings
            showPerformanceSettings = EditorGUILayout.Foldout(showPerformanceSettings, "Performance Settings", true);
            if (showPerformanceSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useObjectPool"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("poolSize"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSimultaneousEffects"));
                EditorGUI.indentLevel--;
            }

            // Component References
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Component References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("audioSource"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playableDirector"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectSpawnPoint"));

            // Streak Levels
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Streak Levels", EditorStyles.boldLabel);
            SerializedProperty streakLevels = serializedObject.FindProperty("streakLevels");

            for (int i = 0; i < 10; i++)
            {
                showStreakSettings[i] = EditorGUILayout.Foldout(showStreakSettings[i], $"Streak {i + 1}", true);
                if (showStreakSettings[i])
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty streakLevel = streakLevels.GetArrayElementAtIndex(i);
                    DrawStreakSettings(streakLevel);
                    EditorGUI.indentLevel--;
                }
            }

            // Default Settings
            EditorGUILayout.Space();
            showDefaultSettings = EditorGUILayout.Foldout(showDefaultSettings, "Default Settings (Streak > 10)", true);
            if (showDefaultSettings)
            {
                EditorGUI.indentLevel++;
                DrawStreakSettings(serializedObject.FindProperty("defaultSettings"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStreakSettings(SerializedProperty settings)
        {
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("meshEffect"));
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("particleEffect"));
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("streakSound"));
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("effectDuration"));
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("timelineAsset"));
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("useDefaultSettingsIfEmpty"));
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("soundVolume"));
            EditorGUILayout.Space();
        }
    }
}