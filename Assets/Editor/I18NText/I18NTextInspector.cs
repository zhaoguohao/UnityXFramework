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

    [MenuItem("GameObject/UI/I18NText (多语言文本)", false)]
    static void CreateI18NText()
    {
        if (null == Selection.activeObject || Selection.activeObject.GetType() != typeof(GameObject))
        {
            GameLogger.LogError("请在Hierarchy视图的某个GameObject节点上右键鼠标");
            return;
        }
        I18N.instance.Init();
        var parent = ((GameObject)(Selection.activeObject)).transform;
        var go = new GameObject("I18NText");

        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<I18NText>();
        text.color = Color.black;
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.i18NId = 1;

        text.Refresh();
    }
}
