using System.Net.Security;
using UnityEngine;
using LuaInterface;

public class BasePanel : MonoBehaviour
{
    public void Init(string moduleName)
    {
        var lua = LuaFramework.LuaManager.instance;
        luaFuncOnShow = lua.GetFunction(moduleName + ".OnShow", false);
        luaFuncOnHide = lua.GetFunction(moduleName + ".OnHide", false);
        luaFuncUpdate = lua.GetFunction(moduleName + ".Update", false);
        luaFuncRegistEvent = lua.GetFunction(moduleName + ".RegistEvent", false);
        luaFuncUnRegistEvent = lua.GetFunction(moduleName + ".UnRegistEvent", false);
    }

    protected virtual void Awake()
    {
        m_selfObj = gameObject;
        m_selfTransform = transform;
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (null != luaFuncUpdate)
            luaFuncUpdate.Call();
    }

    public virtual void Show()
    {
        if (1 == m_state)
            return;

        m_state = 1;
        m_selfObj.SetActive(true);


        OnShow(m_selfTransform);
        RegistEvent();
    }

    protected virtual void RegistEvent()
    {
        if (null != luaFuncRegistEvent)
            luaFuncRegistEvent.Call();
    }


    protected virtual void OnShow(Transform parent)
    {
        if (null != luaFuncOnShow)
            luaFuncOnShow.Call(parent);
    }

    public virtual void Hide()
    {
        if (0 == m_state)
            return;

        OnHide();
        UnRegistEvent();

        m_selfObj.SetActive(false);
        m_state = 0;
    }

    protected virtual void OnHide()
    {
        if (null != luaFuncOnHide)
            luaFuncOnHide.Call();
    }

    protected virtual void UnRegistEvent()
    {
        if (null != luaFuncUnRegistEvent)
            luaFuncUnRegistEvent.Call();
    }

    /// <summary>
    ///  状态，0：未显示，1：显示
    /// </summary>
    private int m_state = 0;

    private GameObject m_selfObj;
    private Transform m_selfTransform;
    private LuaFunction luaFuncOnShow;
    private LuaFunction luaFuncOnHide;
    private LuaFunction luaFuncUpdate;
    private LuaFunction luaFuncRegistEvent;
    private LuaFunction luaFuncUnRegistEvent;
}
