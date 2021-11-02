#define LUA_VERSION_5_3

using UnityEngine;
using LuaInterface;
using System;

namespace LuaFramework
{
    public class LuaManager
    {
        private static LuaManager _instance;
        public static LuaManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new LuaManager();
            }

            return _instance;
        }

        public static LuaManager instance
        {
            get { return GetInstance(); }
        }

        private LuaState lua;
        //private LuaLooper loop = null;

        private LuaManager()
        {
            lua = new LuaState();
#if LUA_VERSION_5_3

#else
            LuaDLL.tolua_setflag(ToLuaFlags.USE_INT64, true);
            LuaDLL.tolua_setflag(ToLuaFlags.FLAG_UINT64, true);
#endif

            this.OpenLibs();
            lua.LuaSetTop(0);

            LuaBinder.Bind(lua);
            OpenBindLib();
        }


        public void InitStart(Action okCb)
        {
            this.lua.Start();    //启动LUAVM
            this.StartMain(() =>
            {
                LuaLooper.GetInstance().OnInit(lua);
                if(null != okCb)
                    okCb();
            });

        }


        // cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
        protected void OpenCJson()
        {
            lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            lua.OpenLibs(LuaDLL.luaopen_cjson);
            lua.LuaSetField(-2, "cjson");

            lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
            lua.LuaSetField(-2, "cjson.safe");
        }

        void StartMain(Action okCb)
        {
            if (AppConst.UseFileList)
            {
                //string outPutStr = "";
                string fileListStr = LuaFileUtils.Instance.ReadStringFromFile(AppConst.LuaFileListName);
                if (fileListStr != null)
                {
                    char[] sperateChars = { '\r', '\n' };
                    string[] files = fileListStr.Split(sperateChars);
                    int totalCnt = files.Length;
                    WalkCoroutine.DoWalk(0.03f, 10, (index) =>
                    {
                        if (index >= totalCnt)
                        {   
                            if (null != okCb)
                                okCb();
                            return false;
                        }
                        string fileName = files[index];
                        if (!string.IsNullOrEmpty(fileName) && !fileName.StartsWith("--"))
                        {
                            lua.DoFile(fileName);
                        }
                        return true;
                    });
                }
                else
                {
                    Debug.LogError("LuaManager StartMian Load filelist file not Exist ");
                }
            }
        }

        /// <summary>
        /// 初始化加载第三方库
        /// </summary>
        void OpenLibs()
        {
            lua.OpenLibs(LuaDLL.luaopen_pb);
            lua.OpenLibs(LuaDLL.luaopen_sproto_core);
            lua.OpenLibs(LuaDLL.luaopen_protobuf_c);
            lua.OpenLibs(LuaDLL.luaopen_lpeg);
#if LUA_VERSION_5_3
            lua.OpenLibs(LuaDLL.luaopen_bit32);
#else
            lua.OpenLibs(LuaDLL.luaopen_bit);
#endif

            lua.OpenLibs(LuaDLL.luaopen_socket_core);

            this.OpenCJson();
        }

        void OpenBindLib()
        {
            lua.BeginModule(null);
            lua.BeginModule("LuaFramework");
            WrapCSObj.Register(lua);
            lua.EndModule();
            lua.EndModule();
        }


        public LuaTable CreateLuaTable(int narr = 0, int nec = 0)
        {
            lua.LuaCreateTable(narr, nec);
            LuaTable tab = lua.CheckLuaTable(-1);

            return tab;
        }

        public LuaTable CheckLuaTable(int stackPos)
        {
            return lua.CheckLuaTable(stackPos);
        }

        public object[] DoFile(string filename)
        {
            return lua.DoFile(filename);
        }

        // Update is called once per frame
        public object[] CallFunction(string funcName, params object[] args)
        {
            LuaFunction func = lua.GetFunction(funcName);
            if (func != null)
            {
                return func.Call(args);
            }
            return null;
        }

        public LuaFunction GetLuaFunc(string funcName)
        {
            return lua.GetFunction(funcName);
        }

        public LuaFunction GetFunction(string funcName, bool beLogMiss = true)
        {
            return lua.GetFunction(funcName, beLogMiss);
        }

        public string GetLuaFunDebugStr()
        {
            return lua.GetLuaFunDebugStr();
        }


        public void LuaGC()
        {
            lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }

        public void Close()
        {
            if (lua != null)
            {
                lua.Dispose();
                lua = null;
            }
        }

        public LuaState GetState()
        {
            return lua;
        }

        /// <summary>
        /// 重载脚本
        /// </summary>
        /// <param name="luafilenames"></param>
        public void ReloadLuaFiles(string[] luafilenames)
        {
            for (int i = 0; i < luafilenames.Length; i++)
            {
                string fileName = luafilenames[i];
                if (!string.IsNullOrEmpty(fileName) && !fileName.StartsWith("--"))
                {
                    lua.DoFile(fileName);
                    GameLogger.LogGreen("Reload: " + fileName);
                }
            }
        }
    }
}

/// <summary>
/// 封装一个调用Lua的简短写法的接口
/// </summary>
public class LuaCall
{
    public static object[] CallFunc(string funcName, params object[] args)
    {
        return LuaFramework.LuaManager.instance.CallFunction(funcName, args);
    }
}