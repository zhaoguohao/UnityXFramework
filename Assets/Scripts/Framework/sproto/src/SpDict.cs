using System;
using System.Collections;
using System.Collections.Generic;

public class SpDict 
{
    protected string[] keys;
    protected SpObject[] values;
    public string[] Keys { get { return keys; } }
    public SpObject[] Values { get { return values; } }

    public SpDict(string[] keys)
    {
        this.keys = keys;
        this.values = new SpObject[keys.Length];
    }

    public SpObject this[string key]
    {
        get
        {
            int idx = IndexOf(key);
            if (idx >= 0 && idx < values.Length)
                return values[idx];
            return null;
        }
        set
        {
            int idx = IndexOf(key);
            if (idx >= 0 && idx < values.Length)
                values[idx] = value;
        }
    }

    public void Add(string key, SpObject value)
    {
        int idx = IndexOf(key);
        if (idx >= 0 && idx < values.Length)
            values[idx] = value;
    }
    public bool ContainsKey(string key)
    {
        int idx = IndexOf(key);
        return idx >= 0;
    }

    public bool Remove(string key)
    {
        int idx = IndexOf(key);
        if (idx >= 0 && idx < values.Length)
            values[idx] = null;
        return idx >= 0;
    }

    public bool TryGetValue(string key, out SpObject value)
    {
        int idx = IndexOf(key);
        if (idx < 0)
        {
            value = null;
            return false;
        }
        value = values[idx];
        return true;
    }

    protected int IndexOf(string key)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i] == key)
                return i;
        }
        return -1;
    }

    class SpDictEnumerator:IEnumerator
    {
        int i = 0;
        SpDict dict;
        public SpDictEnumerator(SpDict dict)
        {
            this.dict = dict;
            i = 0;
        }
        public object Current { get { return dict.values[i]; } }
        public bool MoveNext()
        {
            i++;
            return (i < dict.keys.Length);
        }
        public void Reset()
        {
            i = 0;
        }
    }

    public IEnumerator GetEnumerator()
    {
        return new SpDictEnumerator(this);
    }
}

