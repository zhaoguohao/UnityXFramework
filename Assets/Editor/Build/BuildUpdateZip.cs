using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Ionic.Zip;
using System.IO;
using LitJson;
using UObject = UnityEngine.Object;
using System.Text.RegularExpressions;

public class BuildUpdateZip : EditorWindow
{
    [MenuItem("Build/打热更包")]
    public static void ShowWindow()
    {
        var win = GetWindow<BuildUpdateZip>();
        win.Show();
    }

    private void Awake()
    {
        m_versionGUI = new VersionGUI();
        m_versionGUI.Awake();
        m_luaFrameworkFilesVersion = TryGetLuaFrameworkFilesVersion();
    }

    private void OnGUI()
    {
        m_versionGUI.DrawVersion();
        GUILayout.BeginHorizontal();
        GUILayout.Label("LuaFrameworkFiles.json");
        m_luaFrameworkFilesVersion = EditorGUILayout.TextField(m_luaFrameworkFilesVersion);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("打热更包"))
        {
            var zipDir = BuildUtils.CreateTmpDir("updateZip");
            BuildUtils.BuildNormalCfgBundle(zipDir);
            BuildLuaUpdateBundle(zipDir);
            var contionsObj = BuildObjBundle(zipDir);
            ZipUpdateDir(zipDir, contionsObj);
            BuildUtils.DeleteDir(zipDir);
            AssetDatabase.Refresh();
            // 自动打开Bin目录
            FastOpenTools.OpenFileOrDirectory("/../Bin");
        }
        DrawObjList();
    }

    private void DrawObjList()
    {
        m_scrollViewPos = GUILayout.BeginScrollView(m_scrollViewPos);
        if (GUILayout.Button("+"))
        {
            m_objList.Add(null);
        }
        for (int i = 0, cnt = m_objList.Count; i < cnt; ++i)
        {
            GUILayout.BeginHorizontal();
            m_objList[i] = EditorGUILayout.ObjectField(m_objList[i], typeof(UObject), false);
            if (GUILayout.Button("-"))
            {
                m_objList.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    private string TryGetLuaFrameworkFilesVersion()
    {
        if (!Directory.Exists(BuildUtils.BIN_PATH))
            return null;
        var fs = Directory.GetFiles(BuildUtils.BIN_PATH);
        foreach (var f in fs)
        {
            if (f.Contains("LuaFrameworkFiles_"))
            {
                var result = Regex.Match(f, ".*LuaFrameworkFiles_(.*).json");
                if (null != result)
                {
                    return result.Groups[1].Value;
                }
            }
        }
        return null;
    }

    private void ZipUpdateDir(string zipDir, bool contionsObj)
    {
        var zipFilePath = BuildUtils.BIN_PATH + "/" + (contionsObj ? "res_" : "script_") + VersionMgr.instance.resVersion + ".zip";
        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }
        ZipFile zipFile = new ZipFile();
        var fs = Directory.GetFiles(zipDir);
        foreach (var f in fs)
        {
            if (f.EndsWith("meta") || f.EndsWith(".manifest"))
                continue;
            zipFile.AddFile(f, "");
        }
        zipFile.Save(zipFilePath);
        zipFile.Dispose();
    }



    private void BuildLuaUpdateBundle(string zipDir)
    {
        var needUpdateLuaFiles = GetNeedUpdateLuaList();
        GameLogger.Log("needUpdateLuaFiles.Count:" + needUpdateLuaFiles.Count);
        var luaBundleDir = BuildUtils.CreateTmpDir("luabundle");

        BuildUtils.CopyLuaToBundleDir(needUpdateLuaFiles, luaBundleDir);
        // 打包AssetBundle
        Hashtable tb = new Hashtable();
        tb["lua_update.bundle"] = "luabundle";
        AssetBundleBuild[] buildArray = BuildUtils.MakeAssetBundleBuildArray(tb);
        BuildUtils.BuildBundles(buildArray, zipDir);
        BuildUtils.DeleteDir(luaBundleDir);
        AssetDatabase.Refresh();
    }

    private bool BuildObjBundle(string zipDir)
    {
        if (0 == m_objList.Count) return false;
        AssetBundleBuild[] bundleBuilds = new AssetBundleBuild[m_objList.Count];
        bool contionsObj = false;
        for (int i = 0, cnt = m_objList.Count; i < cnt; ++i)
        {
            var obj = m_objList[i];
            if (null == obj) continue;
            var assetPath = AssetDatabase.GetAssetPath(obj);
            var fileName = Path.GetFileName(assetPath);

            bundleBuilds[i].assetBundleName = fileName;
            bundleBuilds[i].assetNames = new string[] { assetPath };
            contionsObj = true;
        }
        BuildUtils.BuildBundles(bundleBuilds, zipDir);
        return contionsObj;
    }

    private List<string> GetNeedUpdateLuaList()
    {
        var localLuaMD5 = BuildUtils.GetOriginalLuaframeworkMD5Json();
        var originalLuaMD5FilePath = BuildUtils.BIN_PATH + "/LuaFrameworkFiles_" + m_luaFrameworkFilesVersion + ".json";
        var originalLuaMD5File = File.OpenRead(originalLuaMD5FilePath);
        StreamReader sr = new StreamReader(originalLuaMD5File);
        var jsonStr = sr.ReadToEnd();
        sr.Close();
        var originalLuaMD5 = JsonMapper.ToObject(jsonStr);
        List<string> needUpdateLuaFiles = new List<string>();
        foreach (var key in localLuaMD5.Keys)
        {
            if (!originalLuaMD5.ContainsKey(key) || originalLuaMD5[key].ToString() != localLuaMD5[key].ToString())
            {
                needUpdateLuaFiles.Add(key);
            }
        }
        return needUpdateLuaFiles;
    }

    private VersionGUI m_versionGUI;
    private string m_luaFrameworkFilesVersion;
    private List<UObject> m_objList = new List<UObject>();
    private Vector2 m_scrollViewPos;
}
