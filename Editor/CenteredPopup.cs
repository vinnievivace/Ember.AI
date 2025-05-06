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
        var popup = CreateInstance<CenteredPopup>();
        popup.titleContent = new GUIContent(title);
        popup._message = message;

        // centre over main editor window
        Rect main = EditorGUIUtility.GetMainWindowPosition();
        popup.position = new Rect(
            main.x + (main.width  - WIDTH)  * 0.5f,
            main.y + (main.height - HEIGHT) * 0.5f,
            WIDTH, HEIGHT
        );

        popup.ShowPopup();
    }

    void OnGUI()
    {
        // — Header Bar —
        var headerRect = new Rect(0, 0, position.width, 24);
        EditorGUI.DrawRect(headerRect, HeaderColor);
        // draw centered title
        var titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = HeaderTextColor }
        };
        GUI.Label(headerRect, titleContent.text, titleStyle);

        // — Body —
        GUILayout.Space(28);  // leave room for header
        GUILayout.Label(_message, EditorStyles.wordWrappedLabel);

        // — Footer (Close) —
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close", GUILayout.Height(24)))
            Close();
    }
}