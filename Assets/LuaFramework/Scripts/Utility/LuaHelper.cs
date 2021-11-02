using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using LuaInterface;
using System;

namespace LuaFramework {
    public static class LuaHelper
    {
        public static System.Type GetType(string classname)
        {
            Assembly assb = Assembly.GetExecutingAssembly(); 
            System.Type t = null;
            t = assb.GetType(classname); ;
            if (t == null)
            {
                t = assb.GetType(classname);
            }
            return t;
        }

        /// <summary>
        /// 网络管理器
        /// </summary>
        public static NetworkManager GetNetManager()
        {
            return NetworkManager.GetInstance();
        }
    }
}
