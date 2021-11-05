using UnityEngine;
using UnityEditor;

public class I18NTextMenu
{
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
