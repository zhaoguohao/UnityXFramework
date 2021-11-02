using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System.Collections.Generic;

/// <summary>
/// Unity工具类
/// </summary>
public class UnityUtil
{

    /// <summary>
    /// uint转换为Color
    /// </summary>
	public static Color ParseColor(uint input)
    {
        return new Color(
            ((input >> 16) & 0xff) / 255.0f,
            ((input >> 8) & 0xff) / 255.0f,
            (input & 0xff) / 255.0f,
            ((input >> 24) & 0xff) / 255.0f
        );
    }

    /// <summary>
    /// uint转换为Color, 但alpha值为1
    /// </summary>
    public static Color ParseSolidColor(uint input)
    {
        return new Color(
            ((input >> 16) & 0xff) / 255.0f,
            ((input >> 8) & 0xff) / 255.0f,
            (input & 0xff) / 255.0f,
            1.0f
        );
    }

    /// <summary>
    /// uint(为BGR格式)转换为Color, 但alpha值为1
    /// </summary>
    public static Color ParseSolidColorBGR(uint input)
    {
        return new Color(
            (input & 0xff) / 255.0f,
            ((input >> 8) & 0xff) / 255.0f,
            ((input >> 16) & 0xff) / 255.0f,
            1.0f
        );
    }

    public static void PrintSystemInfo()
    {
        string systemInfo = SystemInfo.operatingSystem + " "
                            + SystemInfo.processorType + " " + SystemInfo.processorCount + " "
                            + "memorySize:" + SystemInfo.systemMemorySize + " "
                            + "Graphics: " + SystemInfo.graphicsDeviceName + " vendor: " + SystemInfo.graphicsDeviceVendor
                            + " memorySize: " + SystemInfo.graphicsMemorySize + " " + SystemInfo.graphicsDeviceVersion;
        GameLogger.Log(systemInfo);
    }

    /// <summary>
    /// 曲线插值计算
    /// </summary>
    public static Vector3 Interp(Vector3[] pts, float t)
    {
        int numSections = pts.Length - 3;
        int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
        float u = t * (float)numSections - (float)currPt;

        Vector3 a = pts[currPt];
        Vector3 b = pts[currPt + 1];
        Vector3 c = pts[currPt + 2];
        Vector3 d = pts[currPt + 3];

        return .5f * (
            (-a + 3f * b - 3f * c + d) * (u * u * u)
            + (2f * a - 5f * b + 4f * c - d) * (u * u)
            + (-a + c) * u
            + 2f * b
            );
    }

    /// <summary>
    /// MD5加密
    /// </summary>
    public static string MD5Encrypt(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        string cl = str;
        string md5str = "";
        MD5 myMd5 = MD5.Create();
        byte[] bytes = myMd5.ComputeHash(Encoding.UTF8.GetBytes(cl));
        for (int i = 0; i < bytes.Length; ++i)
        {
            md5str += bytes[i].ToString("x2");
        }
        return md5str;
    }

    /// <summary>
    /// 将Unix时间戳转为DateTime
    /// </summary>
    public static System.DateTime ConverToDateTime(double gmt)
    {
        System.DateTime time = System.DateTime.MinValue;
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        time = startTime.AddSeconds(gmt);

        return time;
    }

    /// <summary>
    /// dateTime转utc
    /// </summary>
    public static double DateTimeConvertToUTC(System.DateTime time)
    {
        double intResult = 0;
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        intResult = (time - startTime).TotalSeconds;
        return intResult;
    }

    /// <summary>
    /// 总秒数 转成 H:M:S 
    /// </summary>
    public static string Second2Time(int second, string spl = ":")
    {
        second = Mathf.Max(second, 0);
        //int day = second / 86400;
        int hour = second / 3600;
        int min = (second % 3600) / 60;
        int sec = second % 60;

        return string.Format("{0:D2}{3}{1:D2}{3}{2:D2}", hour, min, sec, spl);
    }

    /// <summary>
    /// 交换数据
    /// <summary>
    public static void Swap<T>(ref T x, ref T y)
    {
        T temp = x;
        x = y;
        y = temp;
    }

    /// <summary>
    /// 将列表随机（洗牌）
    /// </summary>
    public static void ShuffleList<T>(ref T[] dataList)
    {
        int len = dataList.Length;

        for (int i = len - 1; i >= 1; --i)
        {
            Swap<T>(ref dataList[i], ref dataList[UnityEngine.Random.Range(0, i)]);
        }
    }


    /// <summary>
    /// 获取本地时间(时分秒)
    /// </summary>
    public static string GetCurTime()
    {
        return DateTime.Now.ToString("MM-dd HH:mm:ss");
    }

    public static string GetMD5FromFile(string fullPath)
    {
        try
        {
            FileStream fs = new FileStream(fullPath, FileMode.Open);
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(fs);
            fs.Close();
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
        catch (System.Exception ex)
        {
            GameLogger.LogError("GetMD5FromFile error, exp: " + ex);
        }

        return string.Empty;
    }

    /// <summary>
    /// 自动转屏设置
    /// </summary>
    public static void ActiveScreenAutoRotate(bool actv)
    {
        Screen.autorotateToLandscapeLeft = actv;
        Screen.autorotateToLandscapeRight = actv;
        //Screen.autorotateToPortrait = false;
        //Screen.autorotateToPortraitUpsideDown = false;
    }

   

    /// <summary>
    /// 获取Hierarchy视图中的对象的路劲
    /// </summary>
    public static void GetPathInHierarchy(Transform tran, ref string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = tran.name;
        }
        else
        {
            path = string.Format("{0}/{1}", tran.name, path);
        }

        if (tran.parent != null)
        {
            GetPathInHierarchy(tran.parent, ref path);
        }
    }

    /// <summary>
    /// 数组的拷贝Clone
    /// </summary>
    public static T[] CloneList<T>(T[] resArr) where T : ICloneable
    {
        T[] cloneArr = resArr == null ? null : new T[resArr.Length];
        if (cloneArr != null)
        {
            for (int i = 0, count = resArr.Length; i < count; ++i)
            {
                cloneArr[i] = (T)(resArr[i].Clone());
            }
        }

        return cloneArr;
    }

    /// <summary>
    /// 链表List的拷贝Clone
    /// </summary>
    public static List<T> CloneList<T>(List<T> resList) where T : ICloneable
    {
        List<T> cloneList = resList == null ? null : new List<T>();
        if (cloneList != null)
        {
            for (int i = 0, count = resList.Count; i < count; ++i)
            {
                T res = resList[i];
                T dest = (T)(res.Clone());
                cloneList.Add(dest);
            }
        }
        return cloneList;
    }

    /// <summary>
    /// 字典Dictionary的拷贝Clone
    /// </summary>
    public static Dictionary<T1, T2> CloneDictionary<T1, T2>(Dictionary<T1, T2> resDic) where T2 : ICloneable
    {
        Dictionary<T1, T2> cloneDic = resDic == null ? null : new Dictionary<T1, T2>();
        if (cloneDic != null)
        {
            foreach (KeyValuePair<T1, T2> keyValue in resDic)
            {
                T1 reskey = keyValue.Key;
                T1 deskey = reskey;
                ICloneable rescl = reskey as ICloneable;
                if (rescl != null)
                {
                    deskey = (T1)(rescl.Clone());
                }
                T2 desValue = (T2)(keyValue.Value.Clone());
                cloneDic.Add(deskey, desValue);
            }
        }
        return cloneDic;
    }

    /// <summary>
    /// 重置随机数种子
    /// </summary>
    public static void ResetRandomSeed()
    {
        int second = (int)(DateTimeConvertToUTC(System.DateTime.Now));
        string secondeStr = second.ToString();
        char[] cs = secondeStr.ToCharArray();
        Array.Reverse(cs);
        string newsecondeStr = new string(cs);
        newsecondeStr = newsecondeStr.Substring(0, newsecondeStr.Length - 1);
        second = int.Parse(newsecondeStr);

        // UnityEngine.Random.seed = second; // 该属性已弃用
        UnityEngine.Random.InitState(second);
    }
}

