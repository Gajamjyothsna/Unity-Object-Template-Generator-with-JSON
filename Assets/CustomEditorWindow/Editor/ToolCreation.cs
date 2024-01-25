using UnityEditor;
using UnityEngine;


public class ToolCreation : EditorWindow
{
    [MenuItem("Tools/Object Generation Tool")]
   public static void ShowWindow()
    {
        GetWindow(typeof(ToolCreation));
    }

    private void OnGUI()
    {
        GUILayout.Label("Create UI Object", EditorStyles.boldLabel);
    }
}
