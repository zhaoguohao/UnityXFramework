using UnityEngine;
using UnityEditor;

public class FastOpenTools 
{
    [MenuItem("Tools/命令/打开I18配置 &i")]
    public static void OpenI18NCfg()
    {
        OpenFileOrDirectory("/GameRes/Config/i18nAppStrings.bytes");
    }

    [MenuItem("Tools/命令/打开资源配置 &r")]
    public static void OpenResCfg()
    {
        OpenFileOrDirectory("/GameRes/Config/resources.bytes");
    }

    

    public static void OpenFileOrDirectory(string uri, string workingDir = "")
    {
        var path = Application.dataPath + uri;
        path = path.Replace("/", "\\");
        EdtUtil.OpenFolderInExplorer(path);
    }
}
