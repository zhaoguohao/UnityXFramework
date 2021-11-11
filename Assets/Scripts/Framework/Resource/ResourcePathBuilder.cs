using UnityEngine;

/// <summary>
/// 资源路径管理
/// </summary>
public class ResourcePathBuilder
{
    static ResourcePathBuilder()
    {
    }

    public static string BuildConfigPath(string name)
    {
#if UNITY_EDITOR
        string path = "Assets/GameRes/Config/" + name;
        if (!path.EndsWith(".bytes")) path += ".bytes";
        return path;
#else
      return name;
#endif
    }


    public static string buildUnityEditorPath(string path)
    {
        return "Res/" + path;
    }

    public static string GetStreamingUrl(string name)
    {
#if UNITY_IPHONE || UNITY_EDITOR
        string url = string.Format("file://{0}/{1}", Application.streamingAssetsPath, name);
#else
        string url = Application.streamingAssetsPath + "/"+ name;
#endif

        return url;
    }
}
