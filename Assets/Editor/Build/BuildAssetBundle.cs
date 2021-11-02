using UnityEngine;
using UnityEditor;


public class BuildAssetBundle
{
    public static void Build()
    {
        string targetPath = Application.streamingAssetsPath + "/res";
        BuildUtils.BuildLuaBundle(targetPath);
        GameLogger.LogGreen("BuildLuaBundle Done");
        BuildUtils.BuildNormalCfgBundle(targetPath);
        GameLogger.LogGreen("BuildNormalCfgBundle Done");
        BuildUtils.BuildGameResBundle(targetPath);
        GameLogger.LogGreen("BuildGameResBundle Done");
    }
}
