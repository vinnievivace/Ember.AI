using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EmberAI.Core;
using EmberAI.Attributes;
using EmberAI.Attributes.EmberAI.Attributes;
using UnityEditor;
using UnityEngine;

namespace EmberAI.Editor
{
    /// <summary>
    /// Custom Editor for <see cref="Block"/>. Inspired by Odin Inspector!
    /// </summary>
    [CustomEditor(typeof(EmberBehaviour), true), CanEditMultipleObjects]
    [InitializeOnLoad]
    public class EmberEditor : UnityEditor.Editor
    {
        // used to determine path to 'logo.png'
        public const string PackageName = "com.emberai";
        
        public static Color HeaderColor = Color.gray1;
        
        private EmberBehaviour ember;
        
        // to facilitate the BoxGroup functionality
        private Dictionary<string, List<SerializedProperty>> _groupedProperties;
        private List<SerializedProperty> _ungroupedProperties;

        #region Methods ................................................................................................
        
        static EmberEditor()
        {
            //
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void ValidateBlocks()
        {
            // only want this logic to run when in edit mode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            
            ValidateSealedMethods();

            foreach (EmberBehaviour ember in FindObjectsByType<EmberBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                ember.InitializeDependencies();
            }
            
        }
        
        private static void ValidateSealedMethods()
        {
            var sealedMethods = typeof(EmberBehaviour).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            sealedMethods = sealedMethods.Where(m => m.GetCustomAttributes(typeof(SealedMethodAttribute)).Any()).ToArray();
        
            var subClasses = (
                from assembly in AppDomain.CurrentDomain.GetAssemblies() 
                from type in assembly.GetTypes() 
                where type.IsSubclassOf(typeof(EmberBehaviour))
                select type).ToList();
        
            foreach (var info in sealedMethods)
            {
                foreach (var subClass in subClasses)
                {
                    var methods = subClass.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(methodInfo => methodInfo.Name == info.Name).ToArray();

                    foreach (var method in methods)
                    {
                        if (method.MethodHandle != info.MethodHandle)
                        {
                            Debug.LogError($"{subClass}, is overriding/hiding sealed method <{method.Name}>.");
                        }
                    }
                }
            }
        }
        
        #endregion
        
        private void OnEnable()
        {
            CacheProperties();
        }

        public override void OnInspectorGUI()
        {
             ember = (EmberBehaviour)target;
             
             DrawHeaderRow();
             DrawProperties();
             DrawDefaultButtons();
             
             EditorLayoutUtils.DrawButtonGroups(ember.GetType(), targets);;
        }

        protected override void OnHeaderGUI()
        {
            //
        }


        public static string GetLogoPath()
        {
            return "Packages/" + PackageName + "/Logo.png";
        }

        private void DrawHeaderRow()
        {
            int oldIndent = EditorGUI.indentLevel;
            int headerHeight = 50;
            
            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-15);

            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();
            EditorGUI.DrawRect(headerRect, HeaderColor);

            float padding = 0f;
    
            Rect logoRect = new Rect(x: headerRect.x + padding, y: headerRect.y + (headerRect.height - 50) * 0.5f, width: 50f, height: 50f);
            Texture2D logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GetLogoPath());;
            
            // this attempts to load the logo in any Project that includes this source code, when not installed as a Package.
            if (logoTexture == null)
            {
                logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/" + PackageName + "/Logo.png");   
            }
            
            if (logoTexture != null)
            {
                GUI.DrawTexture(logoRect, logoTexture, ScaleMode.ScaleToFit);
            }

            float headingX = logoRect.xMax + 10f; 
            float headingWidth = headerRect.xMax - headingX - padding; Rect headingRect = new Rect(headingX, headerRect.y, headingWidth, headerRect.height);

            GUIStyle headingStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                normal = { textColor = Color.white }
            };
            
            GUI.Label(headingRect, "EmberAI: " + ember.GetType().Name, headingStyle);

            EditorGUI.indentLevel = oldIndent;
        }
        
        private void DrawProperties()
        {
            serializedObject.Update();
            
            // Draw each group in a box
            foreach (var kvp in _groupedProperties)
            {
                string groupName = kvp.Key;
                List<SerializedProperty> props = kvp.Value;

                EditorGUILayout.BeginVertical("box");

                if (!groupName.IsEmptyString())
                {
                    EditorGUILayout.LabelField(" - " + groupName, EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                }
                
                EditorGUI.indentLevel++;

                foreach (SerializedProperty prop in props)
                {
                    EditorGUILayout.PropertyField(prop, true);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);
            }

            // Draw ungrouped properties normally
            foreach (SerializedProperty prop in _ungroupedProperties)
            {
                EditorGUILayout.PropertyField(prop, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDefaultButtons()
        {
            EditorLayoutUtils.BeginRows();
           
            GUIContent loadButtonContent = new GUIContent("Documentation", "Opens documentation for the " + nameof(EmberBehaviour) + " component");
            GUIContent showDebug = new GUIContent("Debug", "Runtime Logs for this instance of " + nameof(EmberBehaviour));
            
            if(GUI.Button(EditorLayoutUtils.GetButtonRect(0,0.5f), loadButtonContent, EditorLayoutUtils.GetButtonStyle()))
            {
                CenteredPopup.Show(ember.GetType().ToString(), ember.GetDocumentation());
            }
            
            GUI.enabled = Application.isPlaying;
            
            if (GUI.Button(EditorLayoutUtils.GetButtonRect(0, 0.5f), showDebug, EditorLayoutUtils.GetButtonStyle()))
            {
                CenteredPopup.Show("Debug: " + ember.name, "....");
            }

            GUI.enabled = true;
            
            /*if(GUI.Button(EditorLayoutUtils.GetButtonRect(1,0.5f), connectButtonContent, EditorLayoutUtils.GetButtonStyle()))
            {
                
            }

            GUI.enabled = false;
            
            if (GUI.Button(EditorLayoutUtils.GetButtonRect(1, 0.5f), disconnectButtonContent, EditorLayoutUtils.GetButtonStyle()))
            {
                
            }
            
            GUI.enabled = true;*/
        }
        
        /// <summary>
        /// Draws buttons for methods tagged with [ButtonGroup].
        /// </summary>
        private void DrawButtonGroups()
        {
            // Find all methods on this component with the attribute
            var methods = ember.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<ButtonGroupAttribute>() != null)
                .ToArray();

            if (methods.Length == 0)
                return;

            EditorGUILayout.Space(8);

            // Group by GroupID
            var groups = methods.GroupBy(m => m.GetCustomAttribute<ButtonGroupAttribute>().GroupID);

            foreach (var group in groups)
            {
                // Label for this group
                EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                EditorLayoutUtils.BeginRows();

                int count = group.Count();
                int col = 0;

                foreach (var method in group)
                {
                    var attr = method.GetCustomAttribute<ButtonGroupAttribute>();
                    float width = 1f / count;

                    var content = new GUIContent(attr.Label, attr.Tooltip);
                    if (GUI.Button(EditorLayoutUtils.GetButtonRect(0, width), content, EditorLayoutUtils.GetButtonStyle()))
                    {
                        // Invoke on all selected objects
                        foreach (var obj in targets)
                        {
                            var beh = obj as EmberBehaviour;
                            method.Invoke(beh, null);
                        }
                    }

                    col++;
                }

                EditorGUILayout.Space(6);
            }
        }

        
        private void CacheProperties()
        {
            _groupedProperties = new Dictionary<string, List<SerializedProperty>>();
            _ungroupedProperties = new List<SerializedProperty>();

            SerializedProperty iterator = serializedObject.GetIterator();

            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.propertyPath == "m_Script") continue;

                    BoxGroupAttribute boxAttr = GetBoxGroupAttribute(iterator);
                    
                    if (boxAttr != null)
                    {
                        string groupName = boxAttr.GroupName;

                        if (!_groupedProperties.ContainsKey(groupName))
                        {
                            _groupedProperties[groupName] = new List<SerializedProperty>();
                        }
                        _groupedProperties[groupName].Add(iterator.Copy());
                    }
                    else
                    {
                        _ungroupedProperties.Add(iterator.Copy());
                    }

                } while (iterator.NextVisible(false));
            }
        }
        
        private BoxGroupAttribute GetBoxGroupAttribute(SerializedProperty property)
        {
            FieldInfo field = GetFieldInfoFromProperty(property);
            
            if (field == null) return null;

            BoxGroupAttribute attr = (BoxGroupAttribute)Attribute.GetCustomAttribute(field, typeof(BoxGroupAttribute));
            
            return attr;
        }

        private FieldInfo GetFieldInfoFromProperty(SerializedProperty prop)
        {
            Type scriptType = target.GetType();

            return scriptType.GetField(prop.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }
    }
}