#define DO_NOT_CHECK_ENVI
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Reflection;
using LuaInterface;
using System.Xml;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LuaFramework
{
    public class Util
    {
        public static int Int(object o)
        {
            return Convert.ToInt32(o);
        }

        public static float Float(object o)
        {
            return (float)Math.Round(Convert.ToSingle(o), 2);
        }

        public static long Long(object o)
        {
            return Convert.ToInt64(o);
        }

        public static int Random(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static float Random(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static string Uid(string uid)
        {
            int position = uid.LastIndexOf('_');
            return uid.Remove(0, position + 1);
        }

        public static long GetTime()
        {
            TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (long)ts.TotalMilliseconds;
        }


        /// <summary>
        /// 查找子对象
        /// </summary>
        public static GameObject Child(GameObject go, string subnode)
        {
            return Child(go.transform, subnode);
        }

        /// <summary>
        /// 查找子对象
        /// </summary>
        public static GameObject Child(Transform go, string subnode)
        {
            Transform tran = go.Find(subnode);
            if (tran == null) return null;
            return tran.gameObject;
        }

        /// <summary>
        /// 取平级对象
        /// </summary>
        public static GameObject Peer(GameObject go, string subnode)
        {
            return Peer(go.transform, subnode);
        }

        /// <summary>
        /// 取平级对象
        /// </summary>
        public static GameObject Peer(Transform go, string subnode)
        {
            Transform tran = go.parent.Find(subnode);
            if (tran == null) return null;
            return tran.gameObject;
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++)
            {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string md5file(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        /// <summary>
        /// 清除所有子节点
        /// </summary>
        public static void ClearChild(Transform go)
        {
            if (go == null) return;
            for (int i = go.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(go.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect(); Resources.UnloadUnusedAssets();
            LuaManager.GetInstance().LuaGC();
        }

        /// <summary>
        /// 取得数据存放目录
        /// </summary>
        public static string DataPath
        {
            get
            {
                string game = AppConst.AppName.ToLower();
                if (Application.isMobilePlatform)
                {
                    return Application.persistentDataPath + "/" + game + "/";
                }
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    return Application.streamingAssetsPath + "/";
                }
                if (AppConst.DebugMode && Application.isEditor)
                {
                    return Application.streamingAssetsPath + "/";
                }
                if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    //int i = Application.dataPath.LastIndexOf('/');
                    //return Application.dataPath.Substring(0, i + 1) + game + "/";
                    //return Application.dataPath + "/" + game + "/";
                    return Application.streamingAssetsPath + "/";
                }
                return "c:/" + game + "/";
            }
        }

        /// <summary>
        /// 应用程序内容路径
        /// </summary>
        public static string AppContentPath()
        {
            string path = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    path = "jar:file://" + Application.dataPath + "!/assets/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    path = Application.dataPath + "/Raw/";
                    break;
                default:
                    path = Application.dataPath + "/StreamingAssets/";
                    break;
            }
            return path;
        }


        public static void Log(string str)
        {
            GameLogger.Log(str);
        }

        public static void LogGreen(string str)
        {
            GameLogger.LogGreen(str);
        }

        public static void LogYellow(string str)
        {
            GameLogger.LogYellow(str);
        }


        public static void LogWarning(string str)
        {
            GameLogger.LogWarning(str);
        }

        public static void LogError(string str)
        {
            GameLogger.LogError(str);
        }

        public static Component AddComponent(GameObject go, string assembly, string classname)
        {
            Assembly asmb = Assembly.Load(assembly);
            Type t = asmb.GetType(assembly + "." + classname);
            return go.AddComponent(t);
        }

        public static LuaFunction GetLuaFunc(string module, string func)
        {
            return LuaManager.GetInstance().GetLuaFunc(module + "." + func);
        }

        public static LuaFunction GetFunction(string module, string func, bool beLogMiss = true)
        {
            return LuaManager.GetInstance().GetFunction(module + "." + func, beLogMiss);
        }

        /// <summary>
        /// 载入Prefab
        /// </summary>
        /// <param name="name"></param>
        public static GameObject LoadPrefab(string name)
        {
            return Resources.Load(name, typeof(GameObject)) as GameObject;
        }

        /// <summary>
        /// 执行Lua方法
        /// </summary>
        public static object[] CallMethod(string module, string func, params object[] args)
        {
            return LuaManager.GetInstance().CallFunction(module + "." + func, args);
        }

        public static string ReadFileFromPath(string fileName)
        {
            return LuaFileUtils.Instance.ReadOtherFile(fileName);

        }

        /// <summary>
        /// 防止初学者不按步骤来操作
        /// </summary>
        /// <returns></returns>
        static int CheckRuntimeFile()
        {
            if (!Application.isEditor) return 0;
            string streamDir = Application.dataPath + "/StreamingAssets/";
            if (!Directory.Exists(streamDir))
            {
                return -1;
            }
            else
            {
                string[] files = Directory.GetFiles(streamDir);
                if (files.Length == 0) return -1;

                if (!File.Exists(streamDir + "files.txt"))
                {
                    return -1;
                }
            }
            string sourceDir = AppConst.FrameworkRoot + "/ToLua/Source/Generate/";
            if (!Directory.Exists(sourceDir))
            {
                return -2;
            }
            else
            {
                string[] files = Directory.GetFiles(sourceDir);
                if (files.Length == 0) return -2;
            }
            return 0;
        }

        /// <summary>
        /// 检查运行环境
        /// </summary>
        public static bool CheckEnvironment()
        {
#if DO_NOT_CHECK_ENVI
            return true;
#else 
#if UNITY_EDITOR
            int resultId = Util.CheckRuntimeFile();
            if (resultId == -1) {
                GameLogger.LogError("没有找到框架所需要的资源，单击Game菜单下Build xxx Resource生成！！");
                EditorApplication.isPlaying = false;
                return false;
            } else if (resultId == -2) {
                GameLogger.LogError("没有找到Wrap脚本缓存，单击Lua菜单下Gen Lua Wrap Files生成脚本！！");
                EditorApplication.isPlaying = false;
                return false;
            }
            if (Application.loadedLevelName == "Test" && !AppConst.GameLoggerMode) {
                GameLogger.LogError("测试场景，必须打开调试模式，AppConst.GameLoggerMode = true！！");
                EditorApplication.isPlaying = false;
                return false;
            }
#endif
            return true;
#endif // DO_NOT_CHECK_ENVI
        }

        //Lua 里面读取配置表文件
        public static LuaTable ReadXMLConfigFile(string fileName)
        {
            XmlDocument doc = XMLUtil.GetEssentialXmlCfg(fileName);
            if (doc == null)
            {
                GameLogger.LogError("ReadXMLConfigFile doc is nil ... " + fileName);
                return null;
            }

            return ReadXMLDoc(doc);
        }

        public static LuaTable ReadXMLDoc(XmlDocument doc)
        {
            LuaTable tab = LuaManager.GetInstance().CreateLuaTable();

            if (tab == null)
            {
                GameLogger.LogError("Util ReadXMLDoc create lua table failed....");
                return null;
            }

            XmlNodeList nodeList = doc.GetElementsByTagName("item");
            for (int i = 0, count = nodeList.Count; i < count; i++)
            {

                XmlNode node = nodeList[i];
                int col = 0;
                LuaTable colTab = null;
                foreach (XmlNode attrNode in node.Attributes)
                {
                    string fieldName = attrNode.Name;
                    string fieldValueStr = attrNode.Value;

                    //默认第一列作为key
                    if (col == 0)
                    {
                        //用列号为key
                        string keyStr = (i + 1).ToString();
                        tab.AddTable(keyStr);
                        colTab = tab[keyStr] as LuaTable;
                    }

                    if (colTab != null)
                    {
                        colTab[fieldName] = fieldValueStr;
                    }

                    col++;

                }

            }

            return tab;
        }

    }
}
