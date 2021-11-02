using System.Collections.Generic;
using System.Text;

/// <summary>
/// I18国际化文字
/// </summary>
public class I18N
{
    public void Init()
    {
        var reader = XMLUtil.GetEssentialXmlCfgByReader("i18nAppStrings");

        while (reader.Read())
        {
            if (XMLUtil.filterElement(reader)) continue;

            var key = (uint)XMLUtil.tryGetInt32(reader, "key", -1);
            string strValue = XMLUtil.tryGetString(reader, "value", "");
            m_stringMap.Add(key, strValue);
        }
        return;
    }

    public static string GetStr(uint key)
    {
        return s_instance.GetString(key);
    }

    public static string GetStr(uint key, params object[] args)
    {
        return s_instance.getResString(key, args);
    }

    public string GetString(uint key)
    {
        if (m_stringMap.ContainsKey(key))
            return m_stringMap[key];
        else
            return string.Empty;
    }

    public string getSeparateResString(uint key, int key2, char sp1 = ';', char sp2 = ':', bool default_output = false)
    {
        string str = null;
        if (m_stringMap.TryGetValue(key, out str))
        {
            string[] strarray = str.Split(sp1);
            for (int i = 0; i < strarray.Length; ++i)
            {
                string[] substrarray = strarray[i].Trim().Split(sp2);
                if (substrarray.Length.Equals(2))
                {
                    int first = int.Parse(substrarray[0].Trim());
                    if (first.Equals(key2))
                    {
                        return substrarray[1].Trim();
                    }
                }
            }
            if (default_output)
                return string.Format(I18N.instance.getResString(14701), key2);
            return str;
        }
        else
        {
            return string.Empty;
        }
    }

    public string getResString(uint key, params object[] args)
    {
        return string.Format(getResString(key), args);
    }

    public string getResString(bool[] indices, params object[] values)
    {
        if (indices.Length != values.Length) return string.Empty;

        StringBuilder sb = new StringBuilder();
        for (int i = 0, len = indices.Length; i < len; ++i)
        {
            sb.Append(indices[i] ? getResString((uint)(values[i])) : values[i]);
        }
        return sb.ToString();
    }


    private Dictionary<uint, string> m_stringMap= new Dictionary<uint, string>();

    private static I18N s_instance;
    public static I18N instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new I18N();
            return s_instance;
        }
    }

}
