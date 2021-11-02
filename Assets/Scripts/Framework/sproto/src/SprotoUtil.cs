using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Text;

public class SprotoUtil
{
    public static void DumpStream(SpStream stream)
    {
        StringBuilder str = new StringBuilder();
        byte[] buf = new byte[16];
        int count;
        while ((count = stream.Read(buf, 0, buf.Length)) > 0)
        {
            str.Append(DumpLine(buf, count));
        }
        Log(str.ToString());
    }

    private static string DumpLine(byte[] buf, int count)
    {
        StringBuilder str = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if (i < count)
                str.AppendFormat("{0:X2}", buf[i]);
            else
                str.Append("  ");

            str.Append((i > 0) && (i < count - 1) && ((i + 1) % 8 == 0) ? " - " : " ");
        }
        str.AppendLine();
        return str.ToString();
    }

    public static void LogErrorEx(string msg, Exception ex, SpObject obj)
    {

        var str = new StringBuilder();
        str.AppendLine(msg);
        str.AppendLine(ex.ToString());
        str.AppendLine(ex.StackTrace);
        SprotoUtil.DumpObject(obj, 0, str);
        GameLogger.LogError(str.ToString());

    }

    public static void DumpObjectWithMsg(string msg, SpObject obj)
    {
        var str = new StringBuilder();
        str.Append(msg);
        DumpObject(obj, 0, str);
        Log(str.ToString());
    }

    public static void DumpObject(SpObject obj)
    {
        var str = new StringBuilder();
        DumpObject(obj, 0, str);
        Log(str.ToString());

    }

    public static void DumpObject(SpObject obj, int tab, StringBuilder str = null)
    {
        if (obj != null)
        {
            if (str == null)
                str = new StringBuilder();

            if (obj.IsTable())
            {
                AddTab(str, tab);
                str.Append("<table>\n");

                SpDict d = obj.AsSpDict();
                if (d != null)
                {
                    var values = d.Values;
                    var ks = d.Keys;
                    for (int i = 0, count = ks.Length; i < count; i++)
                    {
                        string k = ks[i];
                        SpObject v = values[i];
                        if (v == null)
                            continue;
                        AddTab(str, tab + 1);
                        str.Append("<key : ");
                        str.Append(k);
                        str.Append(">\n");
                        DumpObject(v, tab + 1, str);
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, SpObject> entry in obj.AsTable())
                    {
                        AddTab(str, tab + 1);
                        str.Append("<key : ");
                        str.Append(entry.Key);
                        str.Append(">\n");
                        DumpObject(entry.Value, tab + 1, str);
                    }
                }
            }
            else
            {
                if (obj.IsArray())
                {
                    AddTab(str, tab);
                    str.Append("<array>\n");
                    foreach (SpObject o in obj.AsArray())
                    {
                        DumpObject(o, tab + 1, str);
                    }
                }
                else
                {
                    if (obj.Value == null)
                    {
                        AddTab(str, tab);
                        str.Append("<null>\n");
                    }
                    else
                    {
                        AddTab(str, tab);
                        str.Append(obj.Value.ToString());
                        str.Append("\n");
                    }
                }
            }
        }
        else
        {
            str.Append("SpObject obj == null");
        }
    }


    private static readonly string[] kAddTabArr = { "\t", "\t\t", "\t\t\t", "\t\t\t\t", "\t\t\t\t\t", "\t\t\t\t\t\t", "\t\t\t\t\t\t" };

    private static void AddTab(StringBuilder str, int n)
    {
        if (n > 0 && n <= kAddTabArr.Length)
        {
            str.Append(kAddTabArr[n - 1]);
            return;
        }

        for (int i = 0; i < n; i++)
            str.Append("\t");
    }

    public static void Log(object obj)
    {
        GameLogger.Log(obj.ToString());
    }

    public static void Assert(bool condition)
    {
        if (condition == false)
            throw new Exception();
    }

    public static string GetFullPath(string path)
    {
        return Application.dataPath + "/" + path;
    }
}
