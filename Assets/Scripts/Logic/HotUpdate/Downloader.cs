using System.IO;
using System.Net;
using UnityEngine;
using System.Threading;


/// <summary>
/// 下载器
/// </summary>
public class Downloader
{
    private Stream m_fs, m_ns;
    private int m_readSize;
    private byte[] m_buff;
    private HotUpdater.PackInfo m_packInfo;


    /// <summary>
    /// 下载状态
    /// </summary>
    public DownloadState state { get; private set; }
    /// <summary>
    /// 当前已下载的大小
    /// </summary>
    public long curDownloadSize { get; private set; }

    /// <summary>
    /// 停止线程
    /// </summary>
    private bool m_stopThread = false;
    private Thread m_thread;


    public void Start(HotUpdater.PackInfo packInfo)
    {
        m_buff = new byte[1024*4];
        state = DownloadState.Ready;
        m_packInfo = packInfo;
        var httpReq = HttpWebRequest.Create(m_packInfo.url) as HttpWebRequest;
        httpReq.Timeout = 5000;
        // 以md5作为文件名保存文件
        var savePath = Application.persistentDataPath + "/" + m_packInfo.md5;
        GameLogger.LogGreen("Downloader Start, savePath: " + savePath);
        m_fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);
        curDownloadSize = m_fs.Length;
        if (curDownloadSize == m_packInfo.size)
        {
            state = DownloadState.End;
            Dispose();
            return;
        }
        else if (curDownloadSize > m_packInfo.size)
        {
            // 下载的文件超过了实际大小，删掉重新下载
            curDownloadSize = 0;
            m_fs.Close();
            File.Delete(savePath);
            m_fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        }
        else if (curDownloadSize > 0)
        {
            GameLogger.LogGreen("检测到上次文件下载未结束，断点续传，上次已下载大小: " + curDownloadSize);
            // 设置本地文件流的起始位置
            m_fs.Seek(curDownloadSize, SeekOrigin.Current);
            // 设置远程访问文件流的起始位置
            httpReq.AddRange(curDownloadSize);
        }

        HttpWebResponse response;
        try
        {
            response = (HttpWebResponse)httpReq.GetResponse();
        }
        catch (System.Exception e)
        {
            GameLogger.LogError(e);
            state = DownloadState.ConnectionError;
            return;
        }

        GameLogger.Log("response.StatusCode: " + response.StatusCode);
        if (response.StatusCode != HttpStatusCode.PartialContent)
        {
            if (File.Exists(savePath))
            {
                m_fs.Close();
                m_fs = null;
                curDownloadSize = 0;
                File.Delete(savePath);
            }
            m_fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        }

        m_ns = response.GetResponseStream();

        // 开启一个独立的写文件线程
        if (null == m_thread)
        {
            m_stopThread = false;
            m_thread = new Thread(WriteThread);
            m_thread.Start();
        }
    }


    /// <summary>
    /// 写文件线程
    /// </summary>
    private void WriteThread()
    {
        state = DownloadState.Ing;
        while (!m_stopThread)
        {
            try
            {
                var readSize = m_ns.Read(m_buff, 0, m_buff.Length);
                if (readSize > 0)
                {
                    m_fs.Write(m_buff, 0, readSize);
                    curDownloadSize += readSize;
                    Thread.Sleep(0);
                }
                else
                {
                    // 完毕
                    m_stopThread = true;
                    state = DownloadState.End;
                    Dispose();
                }
            }
            catch (System.Exception e)
            {
                // 下载出错
                state = DownloadState.DataProcessingError;
                Dispose();
            }
        }
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Dispose()
    {
        m_stopThread = true;
        if (null != m_fs)
        {
            m_fs.Close();
            m_fs = null;
        }
        if (null != m_ns)
        {
            m_ns.Close();
            m_ns = null;
        }
        m_packInfo = null;
        m_buff = null;
    }


    public enum DownloadState
    {
        Ready,
        Ing,
        ConnectionError,
        DataProcessingError,
        End,
    }
}
