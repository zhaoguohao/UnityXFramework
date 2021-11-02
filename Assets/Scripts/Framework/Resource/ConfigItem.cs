using UnityEngine;
using System.Xml;
using System.Collections.Generic;
using System;

/// <summary>
/// 配置Item类
/// </summary>
public interface IConfigItem
{
    string GetKey();
    bool Parse(XmlNode node);
}

public abstract class ConfigItem
{
    public const string KeySpan = "_";
    public abstract string GetKey();

    public virtual void OnItemParsed()
    {

    }

    public virtual bool Parse(XmlNode node)
    {
        object self = this;
        var result = ObjectParser.Parse(node, ref self, this.GetType());

        OnItemParsed();

        return result == ObjectParser.Result.OK;
    }
}

public class ConfigFile<T> where T : ConfigItem
{
    protected Dictionary<string, T> m_items = new Dictionary<string, T>();

    public ConfigFile(string filePath)
    {
        XmlDocument doc = null;
        doc = XMLUtil.GetEssentialXmlCfg(filePath);
        if(null == doc)
        {
            GameLogger.LogError("Read Cfg Error, null == doc, path: " + filePath);
        }

        if (!InitFile(doc))
        {
            GameLogger.LogError("Init Config File Error filePath = " + filePath);
        }
    }

    public bool InitFile(XmlDocument doc)
    {
        XmlNodeList nodeList = doc.GetElementsByTagName("item");
        string errorStr = "";
        int count = nodeList.Count;
        for (int i = 0; i < count; i++)
        {

            XmlNode node = nodeList[i];
            T obj = (T)Activator.CreateInstance(typeof(T));//ReflectionUtils.CreateInstance<T>(typeof(T));
            if (!obj.Parse(node))
            {
                errorStr += ObjectParser.lastError;
            }
            var key = obj.GetKey();
            if (!m_items.ContainsKey(key))
                m_items.Add(key, obj);
            else
                GameLogger.LogError(string.Format("cfg has contains the same key,key:{0}", key));

        }

        if (!string.IsNullOrEmpty(errorStr))
        {
            Debug.LogError("InitFile error " + errorStr);
            return false;
        }
        else if (count == 0)
        {
            string baseUrl = doc.BaseURI;
            int index = baseUrl.LastIndexOf('/');
            baseUrl = baseUrl.Substring(index + 1);
            Debug.LogError("Cannot find Elements item, file name = " + baseUrl);
            return false;
        }
        else
        {
            return true;
        }
    }

    public T GetItem(string key)
    {
        T item = null;
        m_items.TryGetValue(key, out item);

        return item;
    }

    public Dictionary<string, T> GetAllItems()
    {
        return m_items;
    }
}
