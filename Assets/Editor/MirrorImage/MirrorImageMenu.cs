using UnityEngine;
using UnityEditor;


public class MirrorImageMenu 
{
    [MenuItem("GameObject/UI/MirrorImage (轴对称图片)", false)]
    static void CreateMirrorImage()
    {
        if (null == Selection.activeObject || Selection.activeObject.GetType() != typeof(GameObject))
        {
            GameLogger.LogError("请在Hierarchy视图的某个GameObject节点上右键鼠标");
            return;
        }
        var parent = ((GameObject)(Selection.activeObject)).transform;
        var go = new GameObject("MirrorImage");

        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, false);
        go.AddComponent<MirrorImage>();
    }
}
