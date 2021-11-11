using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// I18国际化文字
/// </summary>
public class I18N
{
    public void Init()
    {
        if (m_isInited) return;
        m_i18nCfg.Clear();
        var reader = XMLUtil.GetXmlCfgByReader("i18nAppStrings");

        while (reader.Read())
        {
            if (XMLUtil.filterElement(reader)) continue;

            var key = (int)XMLUtil.tryGetInt32(reader, "key", -1);
            var item = new I18NCfgItem();
            item.id = key;
            item.zhCn = XMLUtil.tryGetString(reader, "value_zh", "");
            item.zhTw = XMLUtil.tryGetString(reader, "value_zh_tw", "");
            item.english = XMLUtil.tryGetString(reader, "english", "");
            m_i18nCfg.Add(key, item);
        }
        m_isInited = true;
    }

    public void Reload()
    {
        m_isInited = false;
        Init();
    }

    public static string GetStr(int key)
    {
        return s_instance.GetString(key);
    }

    public static string GetStr(int key, params object[] args)
    {
        return s_instance.getResString(key, args);
    }

    public string GetString(int key)
    {
        if (m_i18nCfg.ContainsKey(key))
        {
            if(!Application.isPlaying)
            {
                return m_i18nCfg[key].zhCn;
            }
            // 根据语言设置，返回对应的语言文本
            switch (LanguageMgr.instance.language)
            {
                case LanguageType.ZH_CN: return m_i18nCfg[key].zhCn;
                case LanguageType.ZH_TW: return m_i18nCfg[key].zhTw;
                case LanguageType.English: return m_i18nCfg[key].english;
                default: return m_i18nCfg[key].zhCn;
            }
        }
        else
            return string.Empty;
    }

    public string getSeparateResString(int key, int key2, char sp1 = ';', char sp2 = ':', bool default_output = false)
    {
        string str = GetString(key);
        if (string.IsNullOrEmpty(str)) return string.Empty;

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

    public string getResString(int key, params object[] args)
    {
        return string.Format(getResString(key), args);
    }

    public string getResString(bool[] indices, params object[] values)
    {
        if (indices.Length != values.Length) return string.Empty;

        StringBuilder sb = new StringBuilder();
        for (int i = 0, len = indices.Length; i < len; ++i)
        {
            sb.Append(indices[i] ? getResString((int)(values[i])) : values[i]);
        }
        return sb.ToString();
    }


    private Dictionary<int, I18NCfgItem> m_i18nCfg = new Dictionary<int, I18NCfgItem>();
    private bool m_isInited = false;

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
