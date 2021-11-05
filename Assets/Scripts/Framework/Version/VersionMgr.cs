using UnityEngine;
using LitJson;
using System.IO;
public class VersionMgr
{

    public void Init()
    {
        // 从文件中读取版本号
        var versionText = Resources.Load<TextAsset>("version").text;
        var jsonData = JsonMapper.ToObject(versionText);
        appVersion = jsonData["app_version"].ToString();
        resVersion = jsonData["res_version"].ToString();

        // 从缓存中读取资源版本号
        var cacheResVersion = ReadCacheResVersion();

        // 如果缓存的版本号比文件中的版本号大，以缓存的为准（因为热更新会增加缓存的资源版本号）
        if (CompareVersion(cacheResVersion, resVersion) > 0)
        {
            resVersion = cacheResVersion;
        }
        GameLogger.Log("appVersion: " + appVersion + ", resVersion: " + resVersion);
    }

    /// <summary>
    /// 读取缓存的资源版本号
    /// </summary>
    private string ReadCacheResVersion()
    {
        if (File.Exists(cacheResVersionFile))
        {
            using (var f = File.OpenRead(cacheResVersionFile))
            {
                using (var sr = new StreamReader(f))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        return "0.0.0.0";
    }

    /// <summary>
    /// 更新资源版本号
    /// </summary>
    public void UpdateResVersion(string resVersion)
    {
        this.resVersion = resVersion;
        var dir = Path.GetDirectoryName(cacheResVersionFile);
        if (Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        using (var f = File.OpenWrite(cacheResVersionFile))
        {
            using(StreamWriter sw = new StreamWriter(f))
            {
                sw.Write(resVersion);
            }
        }
        GameLogger.LogGreen("更新本地缓存版本号：" + resVersion);
    }

    /// <summary>
    /// 删除缓存的资源版本号
    /// </summary>
    public void DeleteCacheResVersion()
    {
        if(File.Exists(cacheResVersionFile))
        {
            File.Delete(cacheResVersionFile);
        }
    }

    /// <summary>
    /// 对比两个版本号的大小
    /// </summary>
    /// <param name="v1">版本号1</param>
    /// <param name="v2">版本号2</param>
    /// <returns>1：v1比v2大，0：v1等于v2，-1：v1小于v2</returns>
    public static int CompareVersion(string v1, string v2)
    {
        if (v1 == v2) return 0;
        string[] v1Array = v1.Split('.');
        string[] v2Array = v2.Split('.');
        for (int i = 0, len = v1Array.Length; i < len; ++i)
        {
            if (int.Parse(v1Array[i]) < int.Parse(v2Array[i]))
                return -1;
            else if (int.Parse(v1Array[i]) > int.Parse(v2Array[i]))
                return 1;
        }
        return 0;
    }

    /// <summary>
    /// 版本号转版本数字，例：1.5.0.12转为1050012
    /// </summary>
    /// <returns></returns>
    public static int VersionCode(string version)
    {
        var s = version.Split('.');
        var result = "";
        for(int i=0,len=s.Length;i<len;++i)
        {
            var b = s[i];
            if(i > 0)
                b = s[i].PadLeft(2, '0');
            result += b;     
        }
        return int.Parse(result);
    }

    /// <summary>
    /// 游戏版本号
    /// </summary>
    public string appVersion { get; private set; }
    /// <summary>
    /// 资源版本号（热更新增加这个版本号）
    /// </summary>
    public string resVersion { get; private set; }

    /// <summary>
    /// 热更新版本号
    /// </summary>
    private string cacheResVersionFile
    {
        get
        {
            return Application.persistentDataPath + "/update/version.txt";
        }
    }

    private static VersionMgr s_instance;
    public static VersionMgr instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new VersionMgr();
            return s_instance;
        }
    }
}
