using System;
using System.Collections.Generic;
using System.Reflection;
using EmberAI.Attributes;
using UnityEditor;
using UnityEngine;

namespace EmberAI.Editor.Windows
{
    public class PlayerPrefsReviewWindow : EditorWindow
    {
        private class Entry
        {
            public string Key;
            public Type FieldType;
            public object CurrentValue;
            public EmberBehaviour Target;
            public FieldInfo Field;
            public string DisplayValue;
        }
        
        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        private List<Entry> _entries = new List<Entry>();
        private Vector2 _scroll;
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        [MenuItem("Window/EmberAI/PlayerPrefs Review")]
        public static void ShowWindow()
        {
            var w = GetWindow<PlayerPrefsReviewWindow>();
            w.titleContent = new GUIContent("PlayerPrefs Review");
            w.Refresh();
        }
        
        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        #endregion

        #region Editor .................................................................................................
        
        private void OnEnable()
        {
            Refresh();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                    Refresh();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Clear All Prefs", GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Clear All PlayerPrefs?",
                        "This will delete ALL PlayerPrefs on disk.  Are you sure?", "Yes", "No"))
                    {
                        PlayerPrefs.DeleteAll();
                        PlayerPrefs.Save();
                        Refresh();
                    }
                }
            }

            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            // Table header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key", GUILayout.Width(200));
            EditorGUILayout.LabelField("Value", GUILayout.Width(200));
            EditorGUILayout.LabelField("Object → Field", GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            foreach (var e in _entries)
            {
                EditorGUILayout.BeginHorizontal();

                // Key
                EditorGUILayout.LabelField(e.Key, GUILayout.Width(200));

                // Editable Value
                string newDisplay = e.DisplayValue;
                if (e.FieldType == typeof(string))
                    newDisplay = EditorGUILayout.TextField(e.DisplayValue, GUILayout.Width(200));
                else if (e.FieldType == typeof(int))
                    newDisplay = EditorGUILayout.IntField(int.Parse(e.DisplayValue), GUILayout.Width(200)).ToString();
                else if (e.FieldType == typeof(float))
                    newDisplay = EditorGUILayout.FloatField(float.Parse(e.DisplayValue), GUILayout.Width(200)).ToString();
                else if (e.FieldType == typeof(bool))
                    newDisplay = EditorGUILayout.Toggle(bool.Parse(e.DisplayValue), GUILayout.Width(200)).ToString();
                else
                    EditorGUILayout.LabelField($"<unsupported:{e.FieldType.Name}>", GUILayout.Width(200));

                // Owner
                EditorGUILayout.LabelField(
                    $"{e.Target.name} → {e.Field.Name}",
                    GUILayout.Width(250));

                // Apply button
                if (newDisplay != e.DisplayValue)
                {
                    if (GUILayout.Button("Apply", GUILayout.Width(60)))
                    {
                        ApplyEntry(e, newDisplay);
                    }
                }
                else
                {
                    GUILayout.Space(64);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region General ................................................................................................

        private void Refresh()
    {
        _entries.Clear();

        foreach (var ember in UnityEngine.Object.FindObjectsOfType<EmberBehaviour>(true))
        {
            if (ember == null) continue;

            var fields = ember.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var f in fields)
            {
                var attr = f.GetCustomAttribute<PlayerPrefAttribute>();
                if (attr == null) continue;

                string key = string.IsNullOrEmpty(attr.Key) ? f.Name : attr.Key;
                Type ft = f.FieldType;
                object val = null;

                // load from PlayerPrefs (fallback to field value if no key)
                if (ft == typeof(string))
                    val = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : (object)f.GetValue(ember);
                else if (ft == typeof(int))
                    val = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key)    : (object)f.GetValue(ember);
                else if (ft == typeof(float))
                    val = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key)  : (object)f.GetValue(ember);
                else if (ft == typeof(bool))
                    val = PlayerPrefs.HasKey(key) ? (PlayerPrefs.GetInt(key)==1) 
                                                  : (object)f.GetValue(ember);
                else
                    continue; // unsupported

                _entries.Add(new Entry {
                    Key          = key,
                    FieldType    = ft,
                    CurrentValue = val,
                    Target       = ember,
                    Field        = f,
                    DisplayValue = val.ToString(),
                });
            }
        }
    }

    private void ApplyEntry(Entry e, string newDisplay)
    {
        // parse back to real type
        object parsed = e.CurrentValue;
        try
        {
            if (e.FieldType == typeof(string))
                parsed = newDisplay;
            else if (e.FieldType == typeof(int))
                parsed = int.Parse(newDisplay);
            else if (e.FieldType == typeof(float))
                parsed = float.Parse(newDisplay);
            else if (e.FieldType == typeof(bool))
                parsed = bool.Parse(newDisplay);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Cannot parse '{newDisplay}' as {e.FieldType.Name}: {ex.Message}");
            return;
        }

        // write to PlayerPrefs
        if (e.FieldType == typeof(string))
            PlayerPrefs.SetString(e.Key, (string)parsed);
        else if (e.FieldType == typeof(int))
            PlayerPrefs.SetInt   (e.Key, (int)parsed);
        else if (e.FieldType == typeof(float))
            PlayerPrefs.SetFloat (e.Key, (float)parsed);
        else if (e.FieldType == typeof(bool))
            PlayerPrefs.SetInt   (e.Key, (bool)parsed ? 1 : 0);

        PlayerPrefs.Save();

        // write back onto the component
        e.Field.SetValue(e.Target, parsed);
        EditorUtility.SetDirty(e.Target);

        // update our display
        e.CurrentValue = parsed;
        e.DisplayValue = newDisplay;
    }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion
        
        #endregion
    }
}