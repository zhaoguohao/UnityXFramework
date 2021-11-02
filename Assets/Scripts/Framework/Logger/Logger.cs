using System.IO;
using UnityEngine;
using System.Text;

/// <summary>
/// 日志输出
/// </summary>
public class Logger
{
    public void Init()
    {
        m_needUpload = false;
#if UNITY_ANDROID || UNITY_IOS
		//var fullSavePath = "mnt/sdcard/output_log.txt";
        var fullSavePath = string.Format("{0}/{1}", Application.persistentDataPath, "output_log.txt");
#elif UNITY_STANDALONE_WIN
        var fullSavePath = string.Format("{0}/{1}", Application.dataPath, "output_log.txt");
#endif
        InitLogger(fullSavePath);
    }
    void InitLogger(string fullSavePath)
    {
        m_fullSavePath = fullSavePath;
        if (File.Exists(fullSavePath))
        {
            FileInfo file = new FileInfo(fullSavePath);
            if (file.Length > MaxFileSize_1)
            {
                File.Delete(fullSavePath);
            }
        }

        if (!Directory.Exists(fullSavePath.Replace("/output_log.txt", "")))
        {
            fullSavePath = string.Format("{0}/{1}", Application.persistentDataPath, "output_log.txt");

            if (File.Exists(fullSavePath))
            {
                FileInfo file = new FileInfo(fullSavePath);
                if (file.Length > MaxFileSize)
                {
                    File.Delete(m_fullSavePath);
                }
            }
        }

        if (Directory.Exists(fullSavePath.Replace("/output_log.txt", "")))
        {
            try
            {
                if (!File.Exists(fullSavePath))
                {
                    FileStream fs = File.Create(fullSavePath);
                    fs.Close();
                }

                Application.logMessageReceivedThreaded += logCallBack;
                GameLogger.Log("Logger Start");
            }
            catch (System.Exception ex)
            {
                GameLogger.Log("error can not open." + ex.StackTrace.ToString() + ex.Message);
            }
        }
        else
        {
            GameLogger.Log("error not found the log output path.");
        }
    }

    public void Flush()
    {
        if (m_logBuilder.Length > 0)
        {
            if (enable && File.Exists(m_fullSavePath))
            {
                using (StreamWriter sw = File.AppendText(m_fullSavePath))
                {
                    sw.WriteLine(m_logBuilder.ToString());
                    m_logBuilder.Remove(0, m_logBuilder.Length);
                }
            }
        }
    }

    private void logCallBack(string condition, string stackTrace, LogType type)
    {
        lock (m_locker)
        {
            if ((type == LogType.Error || type == LogType.Exception))
            {
                m_needUpload = true;
            }

            string log = string.Format("{0} {1}\n", System.DateTime.Now.ToString("MM-dd HH:mm:ss"), condition);
            m_logBuilder.Append(log);
            m_logBuilder.Append(stackTrace);
            m_logBuilder.Append("\n");
            Flush();
        }
    }

    /// <summary>
    ///复制一份log，用来查找bug
    /// </summary>
    public void CopyOutPutLog()
    {
        try
        {
            //Debug.LogError("CopyOutPutLog");
            if (File.Exists(m_fullSavePath))
            {
                string file = File.ReadAllText(m_fullSavePath);
                if (file.Length > 0)
                {
                    string copyFile = m_fullSavePath;//m_fullSavePath.Replace("/output_log.txt", "")

                    copyFile = copyFile.Replace("/output_log.txt", "/output_log_1.txt");
                    if (File.Exists(copyFile))
                    {
                        FileInfo targetFile = new FileInfo(copyFile);
                        if (targetFile.Length > MaxFileSize_1)
                        {
                            File.Delete(copyFile);
                        }
                    }
                    File.AppendAllText(copyFile, file);
                }
            }
        }
        catch (System.Exception e)
        {
            GameLogger.LogError(e);
        }
    }

    private Logger()
    {
        m_logBuilder = new StringBuilder();
        m_needUpload = false;
        enable = true;
        m_locker = new System.Object();
    }

    public static Logger instance
    {
        get
        {
            if (s_instance == null) s_instance = new Logger();
            return s_instance;
        }
    }

    public bool needUpload
    {
        get { return m_needUpload; }
    }

    public bool enable;
    private bool m_needUpload;
    private string m_fullSavePath = string.Empty;
    private StringBuilder m_logBuilder;
    private System.Object m_locker;
    private static Logger s_instance;
    private const long MaxFileSize = 0;
    private const long MaxFileSize_1 = 5 * 1024 * 1024;
}
