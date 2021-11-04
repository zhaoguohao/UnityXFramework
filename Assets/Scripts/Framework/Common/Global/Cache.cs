
using System.Collections;
using UnityEngine;
using LitJson;

public class Cache
{
    public static string Get(string cacheKey, string defaultValue)
    {
        return PlayerPrefs.GetString(cacheKey, defaultValue);
    }

    public static void Set(string cacheKey, string value)
    {
        PlayerPrefs.SetString(cacheKey, value);
    }

    public static string JsonGet(string cacheKey, string jsonKey, string defaultValue)
    {
        var old = Get(cacheKey, "[]");
        if (old.Length > 0 && !old.StartsWith("["))
        {
            JsonClear(cacheKey);
            return defaultValue;
        }
        var jsonObj = JsonMapper.ToObject(old);
        for (int i = 0, cnt = jsonObj.Count; i < cnt; ++i)
        {
            if (((IDictionary)jsonObj[i]).Contains(jsonKey))
            {
                return jsonObj[i][jsonKey].ToString();
            }
        }
        return defaultValue;
    }

    public static void JsonSet(string cacheKey, string jsonKey, string value)
    {
        var old = Get(cacheKey, "[]");
        if (old.Length > 0 && !old.StartsWith("["))
            old = "[]";
        var jsonObj = JsonMapper.ToObject(old);
        var keyInJson = false;
        for (int i = 0, cnt = jsonObj.Count; i < cnt; ++i)
        {
            if (((IDictionary)jsonObj[i]).Contains(jsonKey))
            { 
                jsonObj[i][jsonKey] = value;
                keyInJson = true;
                break;
            }
        }
        if(!keyInJson)
        {
            var jsonItem = new JsonData();
             jsonItem[jsonKey] = value;
             jsonObj.Add(jsonItem);
        }
        Set(cacheKey, jsonObj.ToJson());
    }

    public static void JsonClear(string cacheKey)
    {
        Set(cacheKey, "[]");
    }
}