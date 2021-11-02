using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework
{
    public class AppFacade : Facade
    {
        private static AppFacade _instance;

        private AppFacade() : base()
        {
        }

        public static AppFacade Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppFacade();
                }
                return _instance;
            }
        }

        private bool m_isStartUp = false;
        private string m_shareKey;
        private string m_shareData;

        /// <summary>
        /// 启动框架
        /// </summary>
        public void StartUp(Action okCb = null)
        {
            if (!Util.CheckEnvironment())
            {
                return;
            }

            m_isStartUp = true;

            LuaInterface.LuaFileUtils.Instance.Init();
            NetworkManager.GetInstance();
            
            //-----------------初始化管理器-----------------------
            LuaManager.GetInstance().InitStart(()=>
            {
                NetworkManager.GetInstance().OnInit();
                if(null != okCb)
                    okCb();
            });
        }

        public void UpdateEx()
        {
            LuaLooper.GetInstance().UpdateEx();
        }

        public void LateUpdateEx()
        {
            LuaLooper.GetInstance().LateUpdateEx();
        }

        public void FixedUpdateEx()
        {
            LuaLooper.GetInstance().FixedUpdateEx();
        }

        public void Close()
        {
            LuaLooper.GetInstance().Destroy();
            LuaManager.GetInstance().Close();
        }
    }
}


