using System.Collections.Generic;
using System;
using UnityEngine;

public class SpObject
{

    public enum ArgType
    {
        Table,
        Array,
        Boolean,
        String,
        Int,
        Long,
        Bytes,
        Null,
    }

    private object mValue;
    private ArgType mType;

    public static SpObject CreateWithKeys(string[] keys)
    {
        var obj = new SpObject();
        obj.mType = ArgType.Table;
        obj.mValue = new SpDict(keys);
        return obj;
    }
    public static SpObject CreateArrayWithCount(int count)
    {
        var obj = new SpObject();
        obj.mType = ArgType.Array;
        obj.mValue = new List<SpObject>(count);
        return obj;
    }

    public SpObject()
    {
        mValue = null;
        mType = ArgType.Null;
    }

    public SpObject(object arg)
    {
        mValue = arg;
        mType = ArgType.Null;

        if (mValue != null)
        {
            Type t = mValue.GetType();
            if (t == typeof(long))
            {
                mType = ArgType.Long;
            }
            else if (t == typeof(int))
            {
                mType = ArgType.Int;
            }
            else if (t == typeof(string))
            {
                mType = ArgType.String;
            }
            else if (t == typeof(bool))
            {
                mType = ArgType.Boolean;
            }
            else if (t == typeof(byte[]))
            {
                mType = ArgType.Bytes;
            }
        }
    }

    public SpObject(ArgType type, params object[] args)
    {
        mType = ArgType.Null;

        switch (type)
        {
            case ArgType.Array:
                foreach (object arg in args)
                {
                    Append(arg);
                }
                break;
            case ArgType.Table:
                for (int i = 0; i < args.Length; i += 2)
                {
                    Insert((string)args[i], args[i + 1]);
                }
                break;
        }
    }

    public bool IsTable()
    {
        return (mType == ArgType.Table);
    }

    public Dictionary<string, SpObject> AsTable()
    {
        return mValue as Dictionary<string, SpObject>;
    }
    public SpDict AsSpDict()
    {
        return mValue as SpDict;
    }

    public void Insert(string key, SpObject obj)
    {
        if (IsTable() == false)
        {
            mType = ArgType.Table;
            mValue = new Dictionary<string, SpObject>();
        }
        var d = AsSpDict();
        if (d != null)
        {
            d[key] = obj;
            return;
        }
        AsTable()[key] = obj;
    }

    public void Insert(string key, object value)
    {
        if (value.GetType() == typeof(SpObject))
            Insert(key, (SpObject)value);
        else
            Insert(key, new SpObject(value));
    }

    public bool IsArray()
    {
        return (mType == ArgType.Array);
    }

    public List<SpObject> AsArray()
    {
        return mValue as List<SpObject>;
    }

    public void Append(SpObject obj)
    {
        if (IsArray() == false)
        {
            mType = ArgType.Array;
            mValue = new List<SpObject>();
        }
        AsArray().Add(obj);
    }

    public void Append(object value)
    {
        if (value.GetType() == typeof(SpObject))
            Append((SpObject)value);
        else
            Append(new SpObject(value));
    }

    public bool IsLong()
    {
        return (mType == ArgType.Long);
    }

    public long AsLong()
    {
        if (IsLong())
            return (long)mValue;
        return Convert.ToInt64(mValue);
    }

    public bool IsInt()
    {
        return (mType == ArgType.Int);
    }

    public int AsInt()
    {
        if (mType == ArgType.Int)
        {
            return Convert.ToInt32(mValue);
        }
        else
        {
            GameLogger.LogError("SpObject AsInt is Error");
            return 0;
        }
        //return (int)mValue;
    }

    public bool IsBoolean()
    {
        return (mType == ArgType.Boolean);
    }

    public bool AsBoolean()
    {
        if (mType == ArgType.Boolean)
        {
            return Convert.ToBoolean(mValue);
        }
        else
        {
            GameLogger.LogError("SpObject AsBoolean is Error");
            return false;
        }

        //return (bool)mValue;
    }

    public bool IsString()
    {
        return (mType == ArgType.String);
    }

    public string AsString()
    {
        return mValue as string;
    }

    public bool IsBytes()
    {
        return (mType == ArgType.Bytes);
    }

    public byte[] AsBytes()
    {
        return mValue as byte[];
    }

    public object Value
    {
        get { return mValue; }
        //set { mValue = value; }
    }

    public void SetValue(object v)
    {
        mValue = v;
    }

    public SpObject this[string key]
    {
        get
        {
            if (IsTable() == false)
                return null;
            var d = AsSpDict();
            if (d != null)
                return d[key];
            Dictionary<string, SpObject> t = AsTable();
            if (t.ContainsKey(key) == false)
                return null;
            return t[key];
        }
        set
        {
            if (IsTable() == false)
                return;
            var d = AsSpDict();
            if (d != null)
            {
                d[key] = value;
                return;
            }
            Dictionary<string, SpObject> t = AsTable();
            t[key] = value;
        }
    }

    public SpObject this[int index]
    {
        get
        {
            if (IsArray() == false)
                return null;

            if (index < 0)
                return null;

            List<SpObject> a = AsArray();
            if (a.Count <= index)
                return null;
            return a[index];
        }
        set
        {
            if (IsArray() == false)
                return;

            if (index < 0)
                return;

            List<SpObject> a = AsArray();
            if (a.Count <= index)
                return;
            a[index] = value;
        }
    }

    public SpObject DeepClone()
    {
        SpObject spObj = new SpObject();
        if (mValue == null)
            return spObj;

        Type t = mValue.GetType();
        if (t == typeof(string))
        {
            string sValue = (string)mValue;
            string dValue = (sValue == null) ? null : (string)sValue.Clone();
            spObj.SetValue(dValue);
            spObj.mType = ArgType.String;
        }
        else if (t == typeof(byte[]))
        {
            Byte[] sValue = this.AsBytes();
            Byte[] dValue = null;
            if (sValue != null && sValue.Length > 0)
            {
                dValue = new byte[sValue.Length];
                for (int i = 0; i < sValue.Length; ++i)
                    dValue[i] = sValue[i];
            }
            spObj.SetValue(dValue);
            spObj.mType = ArgType.Bytes;
        }
        else if (t == typeof(List<SpObject>))
        {
            List<SpObject> dValues = new List<SpObject>();
            List<SpObject> sValues = this.AsArray();
            foreach (SpObject sp in sValues)
            {
                if (sp != null)
                    dValues.Add(sp.DeepClone());
                else
                    dValues.Add(null);
            }
            spObj.SetValue(dValues);
            spObj.mType = ArgType.Array;
        }
        else if (t == typeof(Dictionary<string, SpObject>))
        {
            Dictionary<string, SpObject> dValues = new Dictionary<string, SpObject>();
            Dictionary<string, SpObject> sValues = this.AsTable();
            foreach (KeyValuePair<string, SpObject> p in sValues)
            {
                if (p.Value != null)
                    dValues.Add(p.Key, p.Value.DeepClone());
                else
                    dValues.Add(p.Key, null);
            }
            spObj.SetValue(dValues);
            spObj.mType = ArgType.Table;
        }
        else if (t == typeof(SpDict))
        {
            SpDict sValues = this.AsSpDict();
            SpDict dValues = new SpDict(sValues.Keys);

            for (int i = 0; i < sValues.Keys.Length; ++i)
            {
                dValues.Keys[i] = sValues.Keys[i];
                dValues.Values[i] = sValues.Values[i] == null ? null : sValues.Values[i].DeepClone();
            }
            spObj.SetValue(dValues);
            spObj.mType = ArgType.Table;
        }
        else if (t == typeof(int))
        {
            spObj.mType = ArgType.Int;
            spObj.SetValue(mValue);
        }
        else if (t == typeof(bool))
        {
            spObj.mType = ArgType.Boolean;
            spObj.SetValue(mValue);
        }
        else if (t == typeof(long))
        {
            spObj.mType = ArgType.Long;
            spObj.SetValue(mValue);
        }

        return spObj;
    }

    #region 解析安全接口

    public static string AsString(SpObject spObj, string key, string defaultValue = "", bool logNull = false)
    {
        if (null != spObj[key]) return spObj[key].AsString();
        else
        {
            if (logNull) GameLogger.LogError("SpObject AsString Error, key: " + key);
            return defaultValue;
        }
    }

    public static int AsInt(SpObject spObj, string key, int defaultValue = 0, bool logNull = false)
    {
        if (null != spObj[key]) return spObj[key].AsInt();
        else
        {
            if (logNull) GameLogger.LogError("SpObject AsInt Error, key: " + key);
            return defaultValue;
        }
    }

    public static bool AsBoolean(SpObject spObj, string key, bool defaultValue = false, bool logNull = false)
    {
        if (null != spObj[key]) return spObj[key].AsBoolean();
        else
        {
            if (logNull) GameLogger.LogError("SpObject AsBoolean Error, key: " + key);
            return defaultValue;
        }
    }

    public static long AsLong(SpObject spObj, string key, long defaultValue = 0, bool logNull = false)
    {
        if (null != spObj[key]) return spObj[key].AsLong();
        else
        {
            if (logNull) GameLogger.LogError("SpObject AsBoolean Error, key: " + key);
            return defaultValue;
        }
    }

    public static List<SpObject> AsArray(SpObject spObj, string key, bool logNull = false)
    {
        if (null != spObj[key]) return spObj[key].AsArray();
        else
        {
            if (logNull) GameLogger.LogError("SpObject AsBoolean Error, key: " + key);
            return new List<SpObject>();
        }
    }
    #endregion 解析安全接口

}
