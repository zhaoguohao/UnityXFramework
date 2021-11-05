using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(I18NText), false)]
public class I18NTextInspector : Editor
{
    SerializedProperty m_i18NId;
    Text m_self;

    private void OnEnable()
    {
        I18N.instance.Init();
        m_self = target as Text;
        m_i18NId = serializedObject.FindProperty("i18NId");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var str = I18N.GetStr(m_i18NId.intValue);
        EditorGUILayout.LabelField("I18N文本:");
        if (-1 != m_i18NId.intValue)
            m_self.text = null != str ? str : "";
        EditorGUILayout.TextArea(null != str ? str : "");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("打开I18N配置"))
        {
            FastOpenTools.OpenI18NCfg();
        }
        if (GUILayout.Button("重新加载 I18N配置"))
        {
            I18N.instance.Reload();
        }
        GUILayout.EndHorizontal();
    }

  
}
