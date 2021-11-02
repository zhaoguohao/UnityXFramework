/*******************************************************************
Description:  延迟调用
********************************************************************/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 延时调用
/// </summary>
public class DelayCallMgr : MonoBehaviour
{
    public void Call(float delayTime, Action cb)
    {
        Call(null, delayTime, cb);
    }

    public void Call(string tag, float delayTime, Action cb)
    {
        if (cb == null)
        {
            return;
        }

        var itr = CoFunc(delayTime, cb);
        if (!string.IsNullOrEmpty(tag))
        {
            if(m_coFuncDic.ContainsKey(tag))
            {
                var oldItr = m_coFuncDic[tag];
                if(null != oldItr)
                    StopCoroutine(oldItr);
                m_coFuncDic.Remove(tag);
            }
            m_coFuncDic.Add(tag, itr);
        }

        StartCoroutine(itr);
    }

    public void Call(string tag, float delayTime, LuaInterface.LuaFunction luaFunc)
    {
        Call(tag, delayTime, () =>
        {
            if (null != luaFunc)
                luaFunc.Call();
        });
    }

    public void Call(float delayTime, LuaInterface.LuaFunction luaFunc)
    {
        Call(null, delayTime, () =>
        {
            if (null != luaFunc)
                luaFunc.Call();
        });
    }


    public void Cancel(string tag)
    {
        if(m_coFuncDic.ContainsKey(tag))
        {
            var itr = m_coFuncDic[tag];
            if(null != itr)
            {
                StopCoroutine(itr);
            }
            m_coFuncDic.Remove(tag);
        }
    }

    public void Clear()
    {
        StopAllCoroutines();
        m_coFuncDic.Clear();
    }

    private IEnumerator CoFunc(float delayTime, Action cb)
    {
        yield return new WaitForSeconds(delayTime);
        cb();
    }

    private Dictionary<string, IEnumerator> m_coFuncDic = new Dictionary<string, IEnumerator>();

    private static DelayCallMgr s_instance = null;
    public static DelayCallMgr instance
    {
        get
        {
            if (null == s_instance)
            {
                GameObject obj = new GameObject("DelayCallMgr");
                s_instance = obj.AddComponent<DelayCallMgr>();
            }
            return s_instance;
        }
    }
}

