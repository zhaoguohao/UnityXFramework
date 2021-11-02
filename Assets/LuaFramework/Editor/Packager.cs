// using UnityEditor;
// using UnityEngine;
// using System.IO;
// using System.Text;
// using System.Collections;
// using System.Collections.Generic;
// using System.Diagnostics;
// using LuaFramework;


// namespace LuaFramework
// {
//     public class Packager
//     {
//         public const bool LuaBundleMode = false;                   //Lua代码AssetBundle模式-默认关闭 
//         public static string platform = string.Empty;
//         public static bool LuaByteMode = false;
//         static List<string> paths = new List<string>();
//         static List<string> files = new List<string>();

//         ///-----------------------------------------------------------
//         static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle", ".json", ".sproto" };
//         static bool CanCopy(string ext)
//         {   //能不能复制
//             foreach (string e in exts)
//             {
//                 if (ext.Equals(e)) return true;
//             }
//             return false;
//         }

//         /// <summary>
//         /// 载入素材
//         /// </summary>
//         static UnityEngine.Object LoadAsset(string file)
//         {
//             if (file.EndsWith(".lua")) file += ".txt";
//             return AssetDatabase.LoadMainAssetAtPath("Assets/LuaFramework/Examples/Builds/" + file);
//         }





// #pragma warning disable 0618
//         static void BuildLuaBundle(string dir)
//         {
//             BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets |
//                                               BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle;
//             string path = "Assets/" + AppConst.LuaTempDir + dir;
//             string[] files = Directory.GetFiles(path, "*.lua.bytes");
//             List<Object> list = new List<Object>();
//             string bundleName = "lua.unity3d";
//             if (dir != null)
//             {
//                 dir = dir.Replace('\\', '_').Replace('/', '_');
//                 bundleName = "lua_" + dir.ToLower() + AppConst.ExtName;
//             }
//             for (int i = 0; i < files.Length; i++)
//             {
//                 Object obj = AssetDatabase.LoadMainAssetAtPath(files[i]);
//                 list.Add(obj);
//             }

//             if (files.Length > 0)
//             {
//                 string output = Application.streamingAssetsPath + "/lua/" + bundleName;
//                 if (File.Exists(output))
//                 {
//                     File.Delete(output);
//                 }
//                 BuildPipeline.BuildAssetBundle(null, list.ToArray(), output, options, EditorUserBuildSettings.activeBuildTarget);
//                 AssetDatabase.Refresh();
//             }
//         }

//         //lua 配置文件
//         static void HandleLuaCfgFile()
//         {
//             string luaCfgPath = AppDataPath + "/" + BuildRawAssets.AssetsDirPrefix + "/lua/res/";

//             //----------复制Lua文件----------------
//             if (!Directory.Exists(luaCfgPath))
//             {
//                 Directory.CreateDirectory(luaCfgPath);
//             }
//             string[] luaPaths = { AppDataPath + "/LuaFramework/lua/res"};

//             for (int i = 0; i < luaPaths.Length; i++)
//             {
//                 paths.Clear(); files.Clear();
//                 string luaDataPath = luaPaths[i];
//                 Recursive(luaDataPath);
//                 int n = 0;
//                 foreach (string f in files)
//                 {

//                     if (f.EndsWith(".meta")) continue;
//                     string strExt = Path.GetExtension(f);
//                     if (!CanCopy(strExt))
//                     {
//                         continue;
//                     }
//                     string newfile = f.Replace(luaDataPath, "");
//                     //因为Build AssetBundle包 不识别.lua文件 所以统一加上.bytes结尾
//                     string newpath = luaCfgPath + newfile + ".bytes";
//                     string path = Path.GetDirectoryName(newpath);
//                     if (!Directory.Exists(path)) Directory.CreateDirectory(path);

//                     if (File.Exists(newpath))
//                     {
//                         File.Delete(newpath);
//                     }
//                     if (LuaByteMode)
//                     {
//                         EncodeLuaFile(f, newpath);
//                     }
//                     else
//                     {
//                         byte[] allBytes = File.ReadAllBytes(f);
//                         if (allBytes == null || allBytes.Length <= 0)
//                         {
//                             GameLogger.LogError("Packager HandleLuaCfgFile allBytes == null || allBytes.Length <= 0) f = " + f);
//                             if (File.Exists(newpath))
//                             {
//                                 File.Delete(newpath);
//                             }
//                             File.WriteAllBytes(newpath, new byte[0]);
//                         }
//                         else
//                         {
//                             byte[] encryptBytes = CryptoUtil.AESEncrypt(allBytes, AppConst.LuaSecretKey);
//                             if (File.Exists(newpath))
//                             {
//                                 File.Delete(newpath);
//                             }
//                             File.WriteAllBytes(newpath, encryptBytes);
//                         }

//                         //File.Copy(f, newpath, true);
//                     }
//                     UpdateProgress(n++, files.Count, newpath);
//                 }
//             }
//             EditorUtility.ClearProgressBar();
//             AssetDatabase.Refresh();
//         }
//         /// <summary>
//         /// 处理Lua文件
//         /// </summary>
//         static void HandleLuaFile()
//         {
//             string luaPath = AppDataPath + "/" + BuildRawAssets.AssetsDirPrefix + "/lua/";

//             string luaCfgPath = AppDataPath + "/" + BuildRawAssets.AssetsDirPrefix + "/lua/res";
//             //----------复制Lua文件----------------
//             if (!Directory.Exists(luaPath))
//             {
//                 Directory.CreateDirectory(luaPath);
//             }
//             string[] luaPaths = { AppDataPath + "/LuaFramework/lua/",
//                               AppDataPath + "/LuaFramework/Tolua/Lua/" };

//             for (int i = 0; i < luaPaths.Length; i++)
//             {
//                 paths.Clear(); files.Clear();
//                 string luaDataPath = luaPaths[i];
//                 Recursive(luaDataPath);
//                 int n = 0;
//                 foreach (string f in files)
//                 {
                    
//                     if (f.EndsWith(".meta")) continue;
//                     string strExt = Path.GetExtension(f);
//                     if(!CanCopy(strExt))
//                     {
//                         continue;
//                     }

//                     //是lua配置 不打包
//                     if (f.Contains(luaCfgPath))
//                     {
//                         continue;
//                     }

//                     string newfile = f.Replace(luaDataPath, "");
//                     //因为Build AssetBundle包 不识别.lua文件 所以统一加上.bytes结尾
//                     string newpath = luaPath + newfile + ".bytes";
//                     string path = Path.GetDirectoryName(newpath);
//                     if (!Directory.Exists(path)) Directory.CreateDirectory(path);

//                     if (File.Exists(newpath))
//                     {
//                         File.Delete(newpath);
//                     }
//                     if (LuaByteMode)
//                     {
//                         EncodeLuaFile(f, newpath);
//                     }
//                     else
//                     {
//                         byte[] allBytes = File.ReadAllBytes(f);
//                         if(allBytes == null || allBytes.Length <= 0)
//                         {
//                             GameLogger.LogError("Packager HandleLuaFile allBytes == null || allBytes.Length <= 0) f = " + f);
//                             if (File.Exists(newpath))
//                             {
//                                 File.Delete(newpath);
//                             }
//                             File.WriteAllBytes(newpath, new byte[0]);
//                         }
//                         else
//                         {
//                             byte[] encryptBytes = CryptoUtil.AESEncrypt(allBytes, AppConst.LuaSecretKey);
//                             if (File.Exists(newpath))
//                             {
//                                 File.Delete(newpath);
//                             }
//                             File.WriteAllBytes(newpath, encryptBytes);
//                         }

//                         //File.Copy(f, newpath, true);
//                     }
//                     UpdateProgress(n++, files.Count, newpath);
//                 }
//             }
//             EditorUtility.ClearProgressBar();
//             AssetDatabase.Refresh();
//         }


//         /// <summary>
//         /// 数据目录
//         /// </summary>
//         static string AppDataPath
//         {
// 			get { return Application.dataPath; }
//         }

//         /// <summary>
//         /// 遍历目录及其子目录
//         /// </summary>
//         static void Recursive(string path)
//         {
//             string[] names = Directory.GetFiles(path);
//             string[] dirs = Directory.GetDirectories(path);
//             foreach (string filename in names)
//             {
//                 string ext = Path.GetExtension(filename);
//                 if (ext.Equals(".meta")) continue;
//                 files.Add(filename.Replace('\\', '/'));
//             }
//             foreach (string dir in dirs)
//             {
//                 paths.Add(dir.Replace('\\', '/'));
//                 Recursive(dir);
//             }
//         }

//         static void UpdateProgress(int progress, int progressMax, string desc)
//         {
//             string title = "Processing...[" + progress + " - " + progressMax + "]";
//             float value = (float)progress / (float)progressMax;
//             EditorUtility.DisplayProgressBar(title, desc, value);
//         }

//         public static void EncodeLuaFile(string srcFile, string outFile)
//         {
//             if (!srcFile.ToLower().EndsWith(".lua") || srcFile.Contains("LuaFileList.lua"))
//             {
//                 File.Copy(srcFile, outFile, true);
//                 return;
//             }
//             bool isWin = true;
//             string luaexe = string.Empty;
//             string args = string.Empty;
//             string exedir = string.Empty;
//             string currDir = Directory.GetCurrentDirectory();
//             if (Application.platform == RuntimePlatform.WindowsEditor)
//             {
//                 isWin = true;
//                 luaexe = "luac.exe";
//                 args = "-o " + outFile + " " + srcFile;
// 				exedir = Path.Combine(EdtUtil.GetDataPathParent(), "LuaEncoder/luavm/");
// 				Directory.SetCurrentDirectory(exedir);
//             }
//             else if (Application.platform == RuntimePlatform.OSXEditor)
//             {
//                 isWin = false;
// 				luaexe = Path.Combine(EdtUtil.GetDataPathParent(), "LuaEncoder/luavm/luac");
//                 args = "-o " + outFile + " " + srcFile;
//             }
            
//             ProcessStartInfo info = new ProcessStartInfo();
//             info.FileName = luaexe;
//             info.Arguments = args;
//             info.WindowStyle = ProcessWindowStyle.Hidden;
//             info.UseShellExecute = isWin;
//             info.ErrorDialog = true;
//             LuaFramework.Util.Log(info.FileName + " " + info.Arguments);

//             Process pro = Process.Start(info);
//             pro.WaitForExit();
//             Directory.SetCurrentDirectory(currDir);
//         }

//         [MenuItem("LuaFramework/Sync Server proto")]
//         public static void SyncServerProto()
//         {
//             string srcPath = Application.dataPath + "/../../../server-skynet/proto";
//             string dstPath = Application.dataPath + "/LuaFramework/Lua/proto";
//             string[] fileList = Directory.GetFiles(srcPath);
//             if (fileList != null)
//             {
//                 for(int i = 0; i < fileList.Length; i++)
//                 {
//                     string fileName = fileList[i].Substring(fileList[i].LastIndexOf('\\'));
//                     string srcFile = srcPath + fileName;
//                     string dstFile = dstPath + fileName;

//                     File.Copy(srcFile, dstFile, true);
//                 }
//             }
            
//         }
//     }
// }
