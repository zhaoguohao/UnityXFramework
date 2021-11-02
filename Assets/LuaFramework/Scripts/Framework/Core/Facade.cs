/* 
    LuaFramework Code By Jarjin lee
*/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace LuaFramework
{

    public class Facade
    {
        //protected IController m_controller;
        static GameObject m_GameManager;
        static Dictionary<string, object> m_Managers = new Dictionary<string, object>();

        public GameObject AppGameManager
        {
            get
            {
                if (m_GameManager == null)
                {
                    m_GameManager = GameObject.Find("GameManager");
                    if(m_GameManager == null)
                    {
                        m_GameManager = new GameObject("GameManager");
                    }
                }
                return m_GameManager;
            }
        }

        protected Facade()
        {
        }


        /// <summary>
        /// 添加管理器
        /// </summary>
        public void AddManager(string typeName, object obj)
        {
            if (!m_Managers.ContainsKey(typeName))
            {
                m_Managers.Add(typeName, obj);
            }
        }

        /// <summary>
        /// 添加Unity对象
        /// </summary>
        public T AddManager<T>(string typeName) where T : Component
        {
            object result = null;
            m_Managers.TryGetValue(typeName, out result);
            if (result != null)
            {
                return (T)result;
            }
            Component c = AppGameManager.AddComponent<T>();
            m_Managers.Add(typeName, c);
            return default(T);
        }

        /// <summary>
        /// 获取系统管理器
        /// </summary>
        public T GetManager<T>(string typeName) where T : class
        {
            if (!m_Managers.ContainsKey(typeName))
            {
                return default(T);
            }
            object manager = null;
            m_Managers.TryGetValue(typeName, out manager);
            return (T)manager;
        }

        //For Lua
        public object GetManager(string typeName)
        {
            object obj = null;
            if (m_Managers.TryGetValue(typeName, out obj))
            {
                return obj;
            }

            return null;
        }

        /// <summary>
        /// 删除管理器
        /// </summary>
        public void RemoveManager(string typeName)
        {
            if (!m_Managers.ContainsKey(typeName))
            {
                return;
            }
            object manager = null;
            m_Managers.TryGetValue(typeName, out manager);
            Type type = manager.GetType();
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                GameObject.Destroy((Component)manager);
            }
            m_Managers.Remove(typeName);
        }
    }
}

