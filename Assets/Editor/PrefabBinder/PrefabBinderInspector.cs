
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PrefabBinder), true)]
public class PrefabBinderInspector : Editor
{
    void Awake()
    {
        btnStyle = new GUIStyle(EditorStyles.miniButton);
        btnStyle.fontSize = 12;
        btnStyle.normal.textColor = Color.green;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("打开PrefabBinder编辑器", btnStyle))
        {
            PrefabBinderEditor.ShowWindow();
        }
        base.OnInspectorGUI();


    }

    private GUIStyle btnStyle;
}
