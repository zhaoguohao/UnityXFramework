using UnityEngine;
using System.Collections;
using LuaFramework;
using System.Collections.Generic;

namespace LuaFramework
{
    public class Base : MonoBehaviour
    {
        private AppFacade m_Facade;

        protected void RegistGameWorldEvent(string eventType, MyEventHandler handler)
        {
            EventDispatcher.instance.Regist(eventType, handler);
        }

        protected void UnRegistGameWorldEvent(string eventType, MyEventHandler handler)
        {
            EventDispatcher.instance.UnRegist(eventType, handler);
        }

        protected void DispatchEvent(string eventType, params object[] objs)
        {
            EventDispatcher.instance.DispatchEvent(eventType, objs);
        }

        protected AppFacade facade
        {
            get
            {
                if (m_Facade == null)
                {
                    m_Facade = AppFacade.Instance;
                }
                return m_Facade;
            }
        }
    }
}


