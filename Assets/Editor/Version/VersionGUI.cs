using UnityEngine;
using UnityEditor;
using LitJson;
using System.IO;

public class VersionGUI 
{
    public void Awake()
    {
        VersionMgr.instance.Init();
        m_appVersion = VersionMgr.instance.appVersion;
    }

    public void DrawVersion()
    {
        GUILayout.BeginHorizontal();
        m_appVersion = EditorGUILayout.TextField("version", m_appVersion);
        JsonData jd = new JsonData();
        jd["app_version"] = m_appVersion;
        jd["res_version"] = m_appVersion;
        if (GUILayout.Button("Save"))
        {
            using (StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/version.bytes"))
            {
                sw.Write(jd.ToJson());
            }
            AssetDatabase.Refresh();
            Debug.Log("Save Version OK: " + m_appVersion);
            VersionMgr.instance.DeleteCacheResVersion();
            VersionMgr.instance.Init();
        }
        GUILayout.EndHorizontal();
    }


    private string m_appVersion;
}
