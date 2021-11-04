
using UnityEngine;
using System;

public class PrefabBinder : MonoBehaviour
{

    [Serializable]

    public class Item
    {
        public string name;
        public UnityEngine.Object obj;

    }

    public Item[] items = new Item[0];

    public UnityEngine.Object GetObj(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        for (int i = 0, cnt = items.Length; i < cnt; i++)
        {
            Item item = items[i];

            if (item.name.Equals(name))
            {
                return item.obj;
            }
        }
        return null;
    }

    public T GetObj<T>(string name) where T : UnityEngine.Object
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
