using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

/// <summary>
/// 资源管理器
/// </summary>
public class ResourceManager
{
    public ResourceManager()
    {
        m_urlDic = new Dictionary<string, ResourceConfigItem>();
    }

    public void Init()
    {
        m_resourceConfigFile = new ConfigFile<ResourceConfigItem>("resources");
        Dictionary<string, ResourceConfigItem> allItems = m_resourceConfigFile.GetAllItems();
        foreach (KeyValuePair<string, ResourceConfigItem> p in allItems)
        {
            if (!m_urlDic.ContainsKey(p.Value.editorPath))
            {
                m_urlDic.Add(p.Value.editorPath, p.Value);
            }
            else
            {
                GameLogger.Log("name not be same, " + p.Value.editorPath);
            }
        }
    }

    public ResourceConfigItem GetResourceItem(int resID)
    {
        ResourceConfigItem item = m_resourceConfigFile.GetItem(resID.ToString());
        if (item == null)
        {
            GameLogger.LogError("getResourceItem error, not contain id: " + resID);
        }

        return item;
    }


    public ResourceConfigItem GetResourceItem(string uri)
    {
        if (m_urlDic.ContainsKey(uri))
            return m_urlDic[uri];

        return null;
    }

    public GameObject InstantiateGameObject(int id)
    {
        return Instantiate<GameObject>(id);
    }

    public T Instantiate<T>(int id) where T : Object
    {
        return Instantiate<T>(Id2Uri(id));
    }

    public T Instantiate<T>(string uri) where T : Object
    {
        var obj = AssetBundleMgr.instance.LoadAsset(uri);
        if (null == obj)
        {
            GameLogger.LogError("Instantiate Error, uri:" + uri);
            return default(T);
        }

        if (typeof(T) == typeof(GameObject))
        {
            T go = Object.Instantiate(obj) as T;
            return go;
        }
        else
        {
            return obj as T;
        }
    }

    public void FreeResource(string uri)
    {
        // TODO
    }

    public void FreeResource(int resourceID, bool unloadAllLoadedObjects)
    {
        FreeResource(Id2Uri(resourceID), unloadAllLoadedObjects);
    }

    public void FreeResource(string uri, bool unloadAllLoadedObjects)
    {
        // TODO
    }

    public void FreeResource(int resourceID)
    {
        FreeResource(Id2Uri(resourceID));
    }

    private string Id2Uri(int id)
    {
        ResourceConfigItem item = GetResourceItem(id);

        if (item == null)
        {
            return string.Empty;
        }

        return item.editorPath;
    }

    public int Uri2Id(string uri)
    {
        if(m_urlDic.ContainsKey(uri))
        {
            return m_urlDic[uri].id;
        }
        return -1;
    }

    private ConfigFile<ResourceConfigItem> m_resourceConfigFile;
    private Dictionary<string, ResourceConfigItem> m_urlDic;

    private static ResourceManager s_instance;
    public static ResourceManager instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new ResourceManager();
            return s_instance;
        }
    }
}

