using UnityEditor;
using UnityEngine;

public class CenteredPopup : EditorWindow
{
    private string _message;
    private const float WIDTH = 600;
    private const float HEIGHT = 240;
    private static readonly Color HeaderColor = new Color(0.12f, 0.12f, 0.12f, 1f);
    private static readonly Color HeaderTextColor = Color.white;

    public static void Show(string title, string message)
    {
        CenteredPopup popup = CreateInstance<CenteredPopup>();
        
        popup.titleContent = new GUIContent(title);
        popup._message = message;

        Rect main = EditorGUIUtility.GetMainWindowPosition();
        popup.position = new Rect(
            main.x + (main.width  - WIDTH)  * 0.5f,
            main.y + (main.height - HEIGHT) * 0.5f,
            WIDTH, HEIGHT
        );

        popup.ShowPopup();
    }

    private void OnGUI()
    {
        Rect headerRect = new Rect(0, 0, position.width, 24);
        
        EditorGUI.DrawRect(headerRect, HeaderColor);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = HeaderTextColor } };
        
        GUI.Label(headerRect, titleContent.text, titleStyle);
        GUILayout.Space(28);  
        GUILayout.Label(_message, EditorStyles.wordWrappedLabel);
       
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Close", GUILayout.Height(24))) Close();
    }
}