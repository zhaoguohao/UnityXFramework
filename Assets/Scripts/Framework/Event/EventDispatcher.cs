/*******************************************************************
Description:  事件管理
********************************************************************/


using UnityEngine;
using System.Collections.Generic;

public delegate object MyEventHandler(params object[] objs);

public class EventDispatcher
{
    public void Regist(string type, MyEventHandler handler)
    {
        if (handler == null)
            return;

        if (listeners.ContainsKey(type))
        {
            //这里涉及到Dispath过程中反注册问题，必须使用listeners[type]+=..
            listeners[type] += handler;
        }
        else
        {
            listeners.Add(type, handler);
        }
    }

    public void UnRegist(string type, MyEventHandler handler)
    {
        if (handler == null)
            return;

        if (listeners.ContainsKey(type))
        {
            //这里涉及到Dispath过程中反注册问题，必须使用listeners[type]-=..
            listeners[type] -= handler;
            if (listeners[type] == null)
            {
                //已经没有监听者了，移除.
                listeners.Remove(type);
            }
        }
    }

    public void DispatchEvent(string evt, params object[] objs)
    {
        try
        {
            if (listeners.ContainsKey(evt))
            {
                MyEventHandler handler = listeners[evt];
                if (handler != null)
                    handler(objs);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat(szErrorMessage, evt, ex.Message, ex.StackTrace);
        }
    }


    public void ClearEvents(string key)
    {
        if (listeners.ContainsKey(key))
        {
            listeners.Remove(key);
        }
    }

    private Dictionary<string, MyEventHandler> listeners = new Dictionary<string, MyEventHandler>();
    private readonly string szErrorMessage = "DispatchEvent Error, Event:{0}, Error:{1}, {2}";

    private static EventDispatcher s_instance;
    public static EventDispatcher instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new EventDispatcher();
            return s_instance;
        }
    }
}

