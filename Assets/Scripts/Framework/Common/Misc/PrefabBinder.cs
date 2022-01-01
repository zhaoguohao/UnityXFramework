
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System;
using UObject = UnityEngine.Object;

/// <summary>
/// 预设物体绑定器
/// </summary>
public class PrefabBinder : MonoBehaviour
{

    [Serializable]

    public class Item
    {
        public string name;
        public UObject obj;

    }

    private Dictionary<string, UObject> _itemDic =new Dictionary<string, UObject>();

    public Item[] items = new Item[0];

    private void Awake() {
        for (int i = 0, cnt = items.Length; i < cnt; i++)
        {
            _itemDic.Add(items[i].name, items[i].obj);
        }
        items = null;
    }

    public UObject GetObj(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if(_itemDic.ContainsKey(name))
            return _itemDic[name];
        return null;
    }

    public T GetObj<T>(string name) where T : UObject
    {
        try
        {
            return (T)GetObj(name);
        }
        catch (Exception e)
        {
            Debug.LogError("PrefabBinder GetObj name = " + name);
            return default(T);
        }
    }
}
