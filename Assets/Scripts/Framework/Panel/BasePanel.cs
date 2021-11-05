using UnityEngine;
using LuaInterface;

public class BasePanel : MonoBehaviour
{
    public void LuaBind(LuaInterface.LuaTable luaObj)
    {
        m_luaObj = luaObj;
        luaFuncOnShow = luaObj.GetLuaFunction("OnShow");
        luaFuncOnHide = luaObj.GetLuaFunction("OnHide");
        luaFuncUpdate = luaObj.GetLuaFunction("Update");
        luaFuncRegistEvent = luaObj.GetLuaFunction("RegistEvent");
        luaFuncUnRegistEvent = luaObj.GetLuaFunction("UnRegistEvent");
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
            luaFuncUpdate.Call(m_luaObj);
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
            luaFuncRegistEvent.Call(m_luaObj);
    }


    protected virtual void OnShow(Transform parent)
    {
        if (null != luaFuncOnShow)
            luaFuncOnShow.Call(m_luaObj, parent);
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
            luaFuncOnHide.Call(m_luaObj);
    }

    protected virtual void UnRegistEvent()
    {
        if (null != luaFuncUnRegistEvent)
            luaFuncUnRegistEvent.Call(m_luaObj);
    }

    /// <summary>
    ///  状态，0：未显示，1：显示
    /// </summary>
    private int m_state = 0;

    private LuaInterface.LuaTable m_luaObj;
    private GameObject m_selfObj;
    private Transform m_selfTransform;
    private LuaFunction luaFuncOnShow;
    private LuaFunction luaFuncOnHide;
    private LuaFunction luaFuncUpdate;
    private LuaFunction luaFuncRegistEvent;
    private LuaFunction luaFuncUnRegistEvent;
}
