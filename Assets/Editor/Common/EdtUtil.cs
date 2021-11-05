using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;

static class EdtUtil
{
    /// <summary>
    /// 运行shell命令
    /// </summary>
    /// <param name="cmd">命令(exe的文件名)</param>
    /// <param name="args">命令的参数</param>
    /// <returns>string[] res[0]命令的stdout输出, res[1]命令的stderr输出</returns>
    public static string[] RunCmd(string cmd, string args)
    {
        string[] res = new string[2];
        var p = CreateCmdProcess(cmd, args);
        res[0] = p.StandardOutput.ReadToEnd();
        res[1] = p.StandardError.ReadToEnd();
        p.Close();
        return res;
    }

    /// <summary>
    /// 运行shell命令,不返回stderr版本
    /// </summary>
    /// <param name="cmd">命令(exe的文件名)</param>
    /// <param name="args">命令的参数</param>
    /// <returns>命令的stdout输出</returns>
    public static string RunCmdNoErr(string cmd, string args)
    {
        var p = CreateCmdProcess(cmd, args);
        var res = p.StandardOutput.ReadToEnd();
        p.Close();
        return res;
    }

    public static string RunCmdNoErr(string cmd, string args, string[] input)
    {
        var p = CreateCmdProcess(cmd, args);
        //p.StandardOutput.ReadToEnd();
        if (input != null && input.Length > 0)
        {
            for(int i = 0; i < input.Length; i++)
                p.StandardInput.WriteLine(input[i]);
        }
        var res = p.StandardOutput.ReadToEnd();
        p.Close();
        return res;
    }
    /// <summary>
    /// 打开文件夹
    /// </summary>
    /// <param name="absPath">文件夹的绝对路径</param>
    public static void OpenFolderInExplorer(string absPath)
    {
#if UNITY_EDITOR
        if (Application.platform == RuntimePlatform.WindowsEditor)
            RunCmdNoErr("explorer.exe", absPath);
        else if (Application.platform == RuntimePlatform.OSXEditor)
			RunCmdNoErr("open", absPath.Replace("\\", "/"));
#endif
    }

    /// <summary>
    /// 打开文件夹 并定位文件
    /// </summary>
    /// <param name="fileAbsPath">文件的绝对路径</param>
    public static void OpenFolderInExplorerReveals(string fileAbsPath)
    {
#if UNITY_EDITOR
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            var args = string.Format("/select,\"{0}\"", fileAbsPath);
            RunCmdNoErr("explorer.exe", args);
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
			fileAbsPath = fileAbsPath.Replace("\\", "/");
            var args = string.Format("-R \"{0}\"", fileAbsPath);
            RunCmdNoErr("open", args);
        }
#endif
    }

    /// <summary>
    /// 一个路径的上层文件夹
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>路径的上层</returns>
    public static string PathParent(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";
        int lastStartIndex = path.Length - 1;
        char lastchar = path[lastStartIndex];
        if (lastchar == '\\' || lastchar == '/')
            lastStartIndex--;
        if (lastStartIndex < 0)
            return path;
  
        int idx = path.LastIndexOf('/', lastStartIndex);
        int idx2 = path.LastIndexOf('\\', lastStartIndex);
        if (idx < idx2)
            idx = idx2;
        if (idx == -1)
        {
            if (lastchar == '\\' || lastchar == '/')
                path = path.Substring(0, lastStartIndex+1);
            return path;
        }
        if (idx == 0)
            return path.Substring(0, 1);
        return path.Substring(0, idx);
    }

    /// <summary>
    /// 取得Application.dataPath的上层文件夹
    /// </summary>
    public static string GetDataPathParent()
    {
        const string kAssets = "Assets";
        var dataPath = Application.dataPath;
        if (!dataPath.EndsWith(kAssets))
            throw new System.Exception("Application.dataPath is not EndsWith \"Assets\" ");
        return dataPath.Substring(0, dataPath.Length - kAssets.Length - 1);
    }

    /// <summary>
    /// 计算文件的md5值
    /// </summary>
    /// <param name="filename">文件名</param>
    /// <returns>文件的md5</returns>
    public static string GetFileMd5(string filename)
    {
        var file = new System.IO.FileStream(filename, System.IO.FileMode.Open);
        var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hash = md5.ComputeHash(file);
        file.Close();
        return BitConverter.ToString(hash).Replace("-", "");
    }

    private static readonly char[] kNewLineSplit = new char[] { '\n' };

    public static Dictionary<string, string> ParseKeyValCfg(string txt, string split = ":", bool needTrim = false)
    {
        if (string.IsNullOrEmpty(txt))
            return null;
        txt = txt.Replace("\r\n", "\n");
        string[] lines = txt.Split(kNewLineSplit, StringSplitOptions.RemoveEmptyEntries);
        return ParseKeyValCfg(lines, split, needTrim);
    }

    public static Dictionary<string, string> ParseKeyValCfg(string[] lines, string split=":", bool needTrim = false)
    {
        if (lines == null || lines.Length == 0)
            return null;
        if (string.IsNullOrEmpty(split))
            return null;
        Dictionary<string, string> tab = new Dictionary<string, string>();
        for(int i = 0,count = lines.Length; i < count; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line))
                continue;
            int idx = line.IndexOf(split);
            if (idx < 0)
                continue;
            string k = idx == 0 ? "" : line.Substring(0, idx);
            string v = line.Substring(idx + split.Length);
            if (needTrim)
            {
                k = k.Trim();
                v = v.Trim();
            }
            tab.Add(k, v); 
        }
        return tab;
    }

    private static System.Diagnostics.Process CreateCmdProcess(string cmd, string args)
    {
        var pStartInfo = new System.Diagnostics.ProcessStartInfo(cmd);
        pStartInfo.Arguments = args;
        pStartInfo.CreateNoWindow = false;
        pStartInfo.UseShellExecute = false;
        pStartInfo.RedirectStandardError = true;
        pStartInfo.RedirectStandardInput = true;
        pStartInfo.RedirectStandardOutput = true;
        pStartInfo.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        pStartInfo.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
        return System.Diagnostics.Process.Start(pStartInfo);
    }

    /// <summary>
    /// 拿文件夹里指定后缀的全部文件
    /// </summary>
    /// <param name="absPath">文件夹绝对路径</param>
    /// <param name="extensions">后缀名</param>
    /// <returns></returns>
    public static List<string> GetFiles(string absPath, string extensions)
    {
        List<string> list = new List<string>();
        GetFiles(absPath, extensions, list);
        return list;
    }

    private static void GetFiles(string absPath, string extensions, List<string> list)
    {
        string[] folders = Directory.GetDirectories(absPath);
        for (int i = 0, len = folders.Length; i < len; i++)
        {
            GetFiles(folders[i], extensions, list);
        }

        string[] files = Directory.GetFiles(absPath);
        for (int i = 0, len = files.Length; i < len; i++)
        {
            string file = files[i];
            if (file.EndsWith(extensions))
            {
                list.Add(file);
            }
        }
    }

    public static string GetFileName(string fileAbsPath)
    {
        int idx = fileAbsPath.LastIndexOf('\\') + 1;
        if (idx <= 0)
        {
            idx = fileAbsPath.LastIndexOf('/') + 1;
        }
        if (idx >= 0)
        {
            return fileAbsPath.Substring(idx, fileAbsPath.Length - idx);
        }
        return fileAbsPath;
    }

    /// <summary>
    /// 获取文件夹上层路径
    /// </summary>
    /// <returns></returns>
    public static string GetFolderParent(string folderPath)
    {
        int idx = folderPath.LastIndexOf('\\') + 1;
        if (idx <= 0)
        {
            idx = folderPath.LastIndexOf('/') + 1;
        }
        if (idx >= 0)
        {
            return folderPath.Substring(0, idx);
        }
        return folderPath;
    }

    public static void Xml2Excel(string xmlPath, string excelPath)
    {
        // 转换成Excel
        string projectDir = Directory.GetCurrentDirectory();
        int lastIndex1 = projectDir.LastIndexOf('\\');
        int lastIndex2 = projectDir.LastIndexOf('\\', lastIndex1 - 1, lastIndex1);
        string rootDir = projectDir.Substring(0, lastIndex2);
        excelPath = rootDir + "/doc/" + excelPath;
        //xmlPath = Path.Combine(projectDir, xmlPath);
        string programPath = rootDir + @"\tool\xml2excel\Xml2Excel.exe";
        if (!File.Exists(programPath))
        {
            EditorUtility.DisplayDialog("错误", "找不到Xml2Excel.exe！", "确定");
            return;
        }

        if(!File.Exists(xmlPath) || !File.Exists(excelPath))
        {
            GameLogger.LogError(xmlPath + "\n" + excelPath);
            return;
        }
        Thread thread = new Thread(() => Xml2Excel(programPath, xmlPath, excelPath));
        thread.Start();
    }

    private static void Xml2Excel(string programPath, string xmlPath, string excelPath)
    {
        try
        {
            string cmd = string.Format("{0} {1} {2} {3}", programPath, xmlPath, excelPath, true);
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c" + cmd;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(936);
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();

            if (!string.IsNullOrEmpty(output))
            {
                GameLogger.LogError("Xml2Excel Console:\n" + output);
            }
        }
        catch (Exception e)
        {
            GameLogger.LogError(e.ToString());
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    private static void GenXml2ExcelScript(string xmlPath, string excelPath, Dictionary<string, string> colNames, string tempPythonFile)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#-*- coding: UTF-8 -*- ");
        sb.AppendLine("from excelpy.xml2excel import xml2excel");
        if (colNames == null || colNames.Count == 0)
        {
            sb.AppendFormat("xml2excel(u\"{0}\", u\"{1}\")", xmlPath, excelPath);
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("col_names = {}");
            foreach (KeyValuePair<string, string> pair in colNames)
            {
                sb.AppendFormat("col_names[\"{0}\"] = u\"{1}\"", pair.Key, pair.Value);
                sb.AppendLine();
            }
            sb.AppendFormat("xml2excel(u\"{0}\", u\"{1}\", col_names)", xmlPath, excelPath);
            sb.AppendLine();
        }
        var txt = sb.ToString();
        File.WriteAllText(tempPythonFile, txt);
    }

}// end class EdtUtil

