using System.Data.Common;
using UnityEngine;

/// <summary>
/// 多语言管理器
/// </summary>
public class LanguageMgr
{
    public void Init()
    {
        var languageIndex = int.Parse(Cache.Get("LANGUAGE_TYPE", "0"));
        language = (LanguageType)languageIndex;
    }

    /// <summary>
    /// 切换语言
    /// </summary>
    /// <param name="index">语言索引，参见LanguageType枚举</param>
    public void ChangeLanguageType(int index)
    {
        language = (LanguageType)index;
        Cache.Set("LANGUAGE_TYPE", index.ToString());
        EventDispatcher.instance.DispatchEvent(EventNameDef.LANGUAGE_TYPE_CHANGED);
    }

    public LanguageType language = LanguageType.ZH_CN;
    public int languageIndex
    {
        get
        {
            return (int)language;
        }
    }
    private static LanguageMgr s_instance;
    public static LanguageMgr instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new LanguageMgr();
            return s_instance;
        }
    }
}

public enum LanguageType
{
    // 中文简体
    ZH_CN = 0,
    // 中文繁体
    ZH_TW,
    // 英语
    English,
}
