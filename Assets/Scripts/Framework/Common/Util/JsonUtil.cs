using LitJson;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class JsonUtil
{
    /// <summary>
    /// 判断jison串是否包含相应的key
    /// </summary>
    /// <param name="data"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    public static bool IsDataContainkeys(JsonData data, params string[] keys)
    {
        string ret = string.Empty;
        return IsDataContainkeys(data, out ret, keys);
    }

    /// <summary>
    /// 判断jison串是否包含相应的key
    /// </summary>
    /// <param name="data"></param>
    /// <param name="ret"> 返回不存在的key字符串 </param>
    /// <param name="keys"></param>
    /// <returns></returns>
    public static bool IsDataContainkeys(JsonData data, out string ret, params string[] keys)
    {
        ret = string.Empty;
        if (keys == null || keys.Length == 0) return false;
        for (int i = 0; i < keys.Length; ++i)
        {
            if(!((IDictionary)data).Contains(keys[i]))
            {
                ret += keys[i];
                if (i != keys.Length - 1)
                    ret += ", ";
            }
        }

        return string.IsNullOrEmpty(ret);
    }

    /// <summary>
    /// 通过JsonData返回Vector3结构体
    /// 格式必须是这样子的{"x":4345,"y":45.34,"z":4432}
    /// </summary>
    public static Vector3 ConvertJsonToVector3(JsonData data)
    {
        Vector3 ret = Vector3.zero;

        if (data == null)
        {
            return ret;
        }

        if (IsDataContainkeys(data, "x"))
        {
            ret.x = float.Parse(data["x"].ToString());
        }

        if (IsDataContainkeys(data, "y"))
        {
            ret.y = float.Parse(data["y"].ToString());
        }

        if (IsDataContainkeys(data, "z"))
        {
            ret.z = float.Parse(data["z"].ToString());
        }

        return ret;
    }

    public static Vector3 ConvertJsonToVector3(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return Vector3.zero;
        } 

        JsonData data = JsonMapper.ToObject(json);
        return ConvertJsonToVector3(data);
    }

    /// <summary>
    /// 尝试获取key值对应的value
    /// </summary>
    public static string TryGetValue(JsonData data, string key, string defaultValue)
    {
        if (IsDataContainkeys(data, key))
        {
            return data[key].ToString();
        }
        return defaultValue;
    }

    public static List<T> ConvetJsonToList<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new List<T>();
        }

        return JsonMapper.ToObject<List<T>>(json);
    }

    public static string ConvertVector3ToJsonStr(Vector3 vec3)
    {
        var sbr = new StringBuilder();
        sbr.Append(@"{'x':");
        sbr.Append(vec3.x);
        sbr.Append(@",'y':");
        sbr.Append(vec3.y);
        sbr.Append(@",'z':");
        sbr.Append(vec3.z);
        sbr.Append("}");
        return sbr.ToString();
    }

    public static string ConvertVector2ToJsonStr(Vector2 vec2)
    {
        var sbr = new StringBuilder();
        sbr.Append(@"{'x':");
        sbr.Append(vec2.x);
        sbr.Append(@",'y':");
        sbr.Append(vec2.y);
        sbr.Append("}");
        return sbr.ToString();
    }
}
