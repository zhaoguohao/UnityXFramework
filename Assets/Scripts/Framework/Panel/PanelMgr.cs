using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Panel管理器
/// </summary>
public class PanelMgr
{
    public void Init()
    {
        GlobalObjs.s_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        GlobalObjs.s_topPanel = GameObject.Find("TopPanel").transform;
        GlobalObjs.s_windowPanel = GameObject.Find("WindowPanel").transform;
        GlobalObjs.s_gamePanel = GameObject.Find("GamePanel").transform;
        GlobalObjs.s_bgPanel = GameObject.Find("BgPanel").transform;
    }

    public BasePanel GetPanelById(int panelId)
    {
        BasePanel panel = null;
        m_panelMap.TryGetValue(panelId, out panel);
        return panel;
    }

    /// <summary>
    /// 显示界面（供lua层调用）
    /// </summary>
    /// <param name="panelId">界面ID</param>
    /// <param name="luaObj">lua界面脚本对象，是一个lua table</param>
    /// <param name="parent">父节点</param>
    /// <returns></returns>
    public BasePanel ShowPanel(int panelId, LuaInterface.LuaTable luaObj, Transform parent)
    {
        var panel = GetPanelById(panelId);
        if (null == panel)
        {
            var go = new GameObject(luaObj.GetStringField("panelName"));
            go.transform.SetParent(parent, false);
            var rectTrans = go.AddComponent<RectTransform>();
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.one;
            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
            panel = go.AddComponent<BasePanel>();
            panel.LuaBind(luaObj);
            m_panelMap.Add(panelId, panel);
        }
        panel.Show();
        return panel;
    }

    /// <summary>
    /// 显示界面（供C#层调用）
    /// </summary>
    /// <param name="panelId">界面ID</param>
    /// <param name="parent">父节点</param>
    /// <typeparam name="T">界面类</typeparam>
    /// <returns></returns>
    public T ShowPanel<T>(int panelId, Transform parent) where T : BasePanel
    {
        var panel = GetPanelById(panelId);
        if (null == panel)
        {
            var go = new GameObject(typeof(T).ToString());
            go.transform.SetParent(parent, false);
            var rectTrans = go.AddComponent<RectTransform>();
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.one;
            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
            panel = go.AddComponent<T>();
            m_panelMap.Add(panelId, panel);
        }
        panel.Show();
        return (T)panel;
    }

    /// <summary>
    /// 关闭界面
    /// </summary>
    /// <param name="panelId">界面ID</param>
    public void HidePanel(int panelId)
    {
        var panel = GetPanelById(panelId);
        if (null != panel)
            panel.Hide();
    }

    public void HideAllPanels()
    {
        foreach (var panel in m_panelMap.Values)
        {
            panel.Hide();
        }
    }

    private Dictionary<int, BasePanel> m_panelMap = new Dictionary<int, BasePanel>();

    private static PanelMgr s_instance;
    public static PanelMgr instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new PanelMgr();
            return s_instance;
        }
    }
}
