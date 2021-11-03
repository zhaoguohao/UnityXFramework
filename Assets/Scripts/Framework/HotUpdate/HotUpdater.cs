using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LuaFramework;
using System;
using LitJson;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.IO;
using Ionic.Zip;


/// <summary>
/// 热更新逻辑
/// </summary>
public class HotUpdater
{
    /// <summary>
    /// app整包强制更新
    /// </summary>
    public Action actionForceFullAppUpdate;
    /// <summary>
    /// app整包非强制更新
    /// </summary>
    public Action actionWeakFullAppUpdate;
    /// <summary>
    /// 没有任何更新
    /// </summary>
    public Action actionNothongUpdate;
    /// <summary>
    /// 全部热更包下载完毕
    /// </summary>
    public Action actionAllDownloadDone;
    /// <summary>
    /// 错误提示
    /// </summary>
    public Action<UnityWebRequest.Result> actionShowErrorTips;

    /// <summary>
    /// 更新提示语
    /// </summary>
    public Action<string> actionUpdateTipsText;

    public Action<float> actionUpdateProgress;
    private DownloadingInfo m_downloadingInfo;


    /// <summary>
    /// 需要整包更新
    /// </summary>
    private bool m_needFullAppUpdate;
    /// <summary>
    /// 是否强制更新
    /// </summary>
    private bool m_hasNextUpdateBtn;
    /// <summary>
    /// 整包更新的URL
    /// </summary>
    private string m_fullAppUpdateUrl;
    /// <summary>
    /// 相同app版本号的res更新（res版本号增加）
    /// </summary>
    private bool m_sameAppVerResUpdate;
    private List<PackInfo> m_packList = new List<PackInfo>();
    /// <summary>
    /// 下载器
    /// </summary>
    private Downloader m_downloader;

    private IEnumerator m_onDownloadItr;

    private int m_curPackIndex = 0;

    public void Init()
    {
        // 解决HTTPS证书问题
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
    }

    public void Start()
    {
        m_needFullAppUpdate = false;
        m_hasNextUpdateBtn = true;
        m_fullAppUpdateUrl = null;
        m_sameAppVerResUpdate = false;
        m_packList.Clear();
        m_downloadingInfo = new DownloadingInfo();

        // 请求更新列表
        var updateInfos = ReqUpdateInfo();
        if (null != updateInfos)
        {
            // 对更新列表进行排序
            SortUpdateInfo(ref updateInfos);
            // 计算实际需要下载的增量包列表
            CalculateUpdatePackList(updateInfos);
            // 需要整包更新
            if (m_needFullAppUpdate)
            {
                if (m_hasNextUpdateBtn)
                {
                    // 可以不强制更新整包，有【下次】按钮
                    actionWeakFullAppUpdate?.Invoke();
                }
                else
                {
                    // 必须整包更新，强制，没有【下次】按钮
                    actionForceFullAppUpdate?.Invoke();
                }
            }
            else if (m_sameAppVerResUpdate)
            {
                // 有同app版本的res增量包更新
                m_curPackIndex = 0;
                StartDownloadResPack();
            }
            else
            {
                // 没有任何更新
                actionNothongUpdate?.Invoke();
            }
        }
    }

    /// <summary>
    /// 请求更新信息
    /// </summary>
    private List<UpdateInfo> ReqUpdateInfo()
    {
        UnityWebRequest uwr = UnityWebRequest.Get(AppConst.WebUrl + "update_list.json");
        var request = uwr.SendWebRequest();
        while (!request.isDone) { }
        if (!string.IsNullOrEmpty(uwr.error))
        {
            // GameLogger.LogError(uwr.error);
            actionShowErrorTips.Invoke(uwr.result);
            return null;
        }

        GameLogger.Log(uwr.downloadHandler.text);
        return JsonMapper.ToObject<List<UpdateInfo>>(uwr.downloadHandler.text);
    }

    private void SortUpdateInfo(ref List<UpdateInfo> updateInfos)
    {
        // 逆序排序，版本号大的排前面
        updateInfos.Sort((a, b) =>
        {
            return VersionMgr.CompareVersion(b.appVersion, a.appVersion);
        });
    }

    /// <summary>
    /// 分析更新类型
    /// </summary>
    public void CalculateUpdatePackList(List<UpdateInfo> updateInfos)
    {
        string bigestAppVer = "0.0.0.0";
        m_hasNextUpdateBtn = false;
        foreach (var info in updateInfos)
        {
            if (VersionMgr.CompareVersion(info.appVersion, VersionMgr.instance.appVersion) > 0)
            {
                m_needFullAppUpdate = true;
                if (VersionMgr.CompareVersion(info.appVersion, bigestAppVer) > 0)
                {
                    bigestAppVer = info.appVersion;
                    m_fullAppUpdateUrl = info.appUrl;
                }
            }
            if (VersionMgr.CompareVersion(info.appVersion, VersionMgr.instance.appVersion) == 0)
            {
                m_hasNextUpdateBtn = true;
                foreach (var pack in info.updateList)
                {
                    if (VersionMgr.CompareVersion(pack.resVersion, VersionMgr.instance.resVersion) > 0)
                    {
                        m_sameAppVerResUpdate = true;
                        m_packList.Add(pack);
                    }
                }
            }
        }
        GameLogger.Log("m_packList.Count: " + m_packList.Count);
    }


    public void DoFullAppUpdate()
    {
        // 跳到应用商店
        Application.OpenURL(m_fullAppUpdateUrl);
    }

    /// <summary>
    /// 跳过大版本更新
    /// </summary>
    public void DoNextTime()
    {
        // 不更新整包，执行res热更
        if (m_sameAppVerResUpdate)
        {
            m_curPackIndex = 0;
            StartDownloadResPack();
        }
        else
        {
            actionNothongUpdate?.Invoke();
        }
    }

    private void StartDownloadResPack(bool next = false)
    {
        actionUpdateTipsText?.Invoke("正在下载更新包，请稍等...");
        if (next)
            ++m_curPackIndex;
        if (m_curPackIndex > m_packList.Count - 1)
        {
            GameLogger.Log("全部热更包下载完毕");
            actionAllDownloadDone?.Invoke();
            return;
        }
        
        // 排序，确保版本号小的先下载
        m_packList.Sort((a, b) =>
        {
            return VersionMgr.CompareVersion(a.resVersion, b.resVersion);
        });
        var packInfo = m_packList[m_curPackIndex];
        GameLogger.Log("开始下载: " + packInfo.ToString());
        m_downloadingInfo.totalPackCount = m_packList.Count;
        m_downloadingInfo.packIndex = m_curPackIndex;
        m_downloadingInfo.targetDownloadSize = packInfo.size;
        m_downloader = new Downloader();
        m_downloader.Start(packInfo);
    }

    public void Update()
    {
        if (null != m_downloader)
        {
            switch (m_downloader.state)
            {
                case Downloader.DownloadState.End:
                    {
                        m_downloader = null;
                        m_onDownloadItr = OnDownloadEnd();
                    }
                    break;
                case Downloader.DownloadState.Ing:
                    {
                        OnDownloading();
                    }
                    break;
                case Downloader.DownloadState.ConnectionError:
                    {
                        m_downloader = null;
                        OnDownloadError(UnityWebRequest.Result.ConnectionError);
                    }
                    break;
                case Downloader.DownloadState.DataProcessingError:

                    {
                        m_downloader = null;
                        OnDownloadError(UnityWebRequest.Result.DataProcessingError);
                    }
                    break;
            }
        }
        RunCoroutine();
    }

    private void RunCoroutine()
    {
        if (null != m_onDownloadItr && !m_onDownloadItr.MoveNext())
        {
            m_onDownloadItr = null;
        }
    }

    private IEnumerator OnDownloadEnd()
    {
        var packInfo = m_packList[m_curPackIndex];
        GameLogger.Log("下载完毕: " + packInfo.url);
        actionUpdateTipsText?.Invoke("正在校验文件，请稍等...");
        yield return null;

        var filePath = Application.persistentDataPath + "/" + packInfo.md5;
        var md5Ok = CheckMd5(filePath, packInfo.md5);
        if (!md5Ok)
        {
            DeleteFile(filePath);
            GameLogger.LogError("MD5校验不通过，重新下载");
            m_downloader = new Downloader();
            m_downloader.Start(m_packList[m_curPackIndex]);
        }
        else
        {
            // 解压zip
            actionUpdateTipsText?.Invoke("正在解压文件，请稍等...");
            yield return null;
            GameLogger.LogGreen("解压文件：" + filePath);
            int index = 0;
            using (ZipFile zip = new ZipFile(filePath))
            {
                var cnt = zip.Count;
                foreach (var entity in zip)
                {
                    ++index;
                    entity.Extract(Application.persistentDataPath + "/update/", ExtractExistingFileAction.OverwriteSilently);
                    actionUpdateProgress?.Invoke((float)index / cnt);
                    yield return null;
                }
            }
            // 删除zip
            DeleteFile(filePath);
            // 更新版本号
            VersionMgr.instance.UpdateResVersion(packInfo.resVersion);
            // 请求下载下一个zip
            StartDownloadResPack(true);
        }
    }


    /// <summary>
    /// 下载中
    /// </summary>
    private void OnDownloading()
    {
        // 更新进度条
        m_downloadingInfo.curDownloadSize = m_downloader.curDownloadSize;
        var value = (float)m_downloadingInfo.curDownloadSize / m_downloadingInfo.targetDownloadSize;
        actionUpdateProgress?.Invoke(value);
    }

    /// <summary>
    /// 下载异常
    /// </summary>
    private void OnDownloadError(UnityWebRequest.Result result)
    {
        actionShowErrorTips?.Invoke(result);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    private void DeleteFile(string filePath)
    {
        GameLogger.LogGreen("删除文件：" + filePath);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    /// <summary>
    /// 检测MD5
    /// </summary>
    /// <param name="filePath">要检测的文件路径</param>
    /// <param name="md5">目标MD5</param>
    /// <returns></returns>
    private bool CheckMd5(string filePath, string md5)
    {
        var localMd5 = LuaFramework.Util.md5file(filePath);
        var ok = localMd5.Equals(md5);
        GameLogger.LogGreen("MD5校验，ok: " + ok);
        return ok;
    }

    /// <summary>
    /// 解决HTTPS证书问题
    /// </summary>
    private bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None) return true;
        bool isOk = true;
        for (int i = 0; i < chain.ChainStatus.Length; i++)
        {
            if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
            {
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build((X509Certificate2)certificate);
                if (!chainIsValid)
                {
                    isOk = false;
                    break;
                }
            }
        }
        return isOk;
    }

    public void Dispose()
    {
        if (null != m_downloader)
            m_downloader.Dispose();
    }

    public class UpdateInfo
    {
        public string appVersion;
        public string appUrl;
        public List<PackInfo> updateList;
    }

    public class PackInfo
    {
        public string resVersion;
        public string md5;
        public int size;
        public string url;

        public override string ToString()
        {
            return string.Format("resVersion: {0}, md5: {1}, size: {2}, url: {3}", resVersion, md5, size, url);
        }
    }

    public struct DownloadingInfo
    {
        public long curDownloadSize;
        public long targetDownloadSize;
        public int packIndex;
        public int totalPackCount;
    }
}


