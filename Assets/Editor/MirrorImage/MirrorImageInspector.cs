using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(MirrorImage))]
public class MirrorImageInspector : ImageEditor
{
    SerializedProperty m_mirrorType;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        EditorGUILayout.PropertyField(m_mirrorType);
        serializedObject.ApplyModifiedProperties();
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        m_mirrorType = serializedObject.FindProperty("m_mirrorType");

    }

}