// #define ENABLE_HOT_UPDATE

using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;

public class HotUpdatePanel : BasePanel
{
    /// <summary>
    /// 版本号
    /// </summary>
    private Text m_versionText;
    /// <summary>
    /// 更新进度条
    /// </summary>
    private Slider m_progressSlider;
    /// <summary>
    /// 更新进度文本
    /// </summary>
    private Text m_progressText;
    /// <summary>
    /// 更新提示文本
    /// </summary>
    private Text m_tipsText;
    /// <summary>
    /// 提示框
    /// </summary>
    private GameObject m_appUpdateDlg;
    /// <summary>
    /// 下一次按钮
    /// </summary>
    private Button m_nextBtn;
    /// <summary>
    /// 整包更新按钮
    /// </summary>
    private Button m_fullAppUpdateBtn;
    /// <summary>
    /// 错误提示框
    /// </summary>
    private GameObject m_errorTipsDlg;
    /// <summary>
    /// 错误提示文本
    /// </summary>
    private Text m_errorText;
    /// <summary>
    /// 重试按钮
    /// </summary>
    private Button m_retryBtn;

    /// <summary>
    /// 热更新器
    /// </summary>
    private HotUpdater m_hotUpdater;
    /// <summary>
    /// 回调，更新完毕后回调
    /// </summary>
    private Action m_cb;

    public static void Show(Action cb)
    {
#if ENABLE_HOT_UPDATE
        var panel = PanelMgr.instance.ShowPanel<HotUpdatePanel>(1, GlobalObjs.s_topPanel);
        panel.m_cb = cb;
#else
        if (null != cb) cb();
#endif
    }

    protected override void OnShow(Transform parent)
    {
        base.OnShow(parent);
        var panelObj = ResourceManager.instance.Instantiate<GameObject>("BaseRes/HotUpdatePanel.prefab");
        panelObj.transform.SetParent(parent, false);
        var binder = panelObj.GetComponent<PrefabBinder>();
        SetUi(binder);
        StartUpdate();
    }

    void SetUi(PrefabBinder binder)
    {
        m_versionText = binder.GetObj<Text>("versionText");
        m_progressSlider = binder.GetObj<Slider>("progressSlider");
        m_progressText = binder.GetObj<Text>("progressText");
        m_tipsText = binder.GetObj<Text>("tipsText");
        m_appUpdateDlg = binder.GetObj<GameObject>("appUpdateDlg");
        m_nextBtn = binder.GetObj<Button>("nextBtn");
        m_fullAppUpdateBtn = binder.GetObj<Button>("updateBtn");
        m_errorTipsDlg = binder.GetObj<GameObject>("errorTipsDlg");
        m_errorText = binder.GetObj<Text>("errorText");
        m_retryBtn = binder.GetObj<Button>("retryBtn");

        m_nextBtn.onClick.AddListener(OnNextBtnClick);
        m_fullAppUpdateBtn.onClick.AddListener(OnFullAppUpdateBtnClick);
        m_retryBtn.onClick.AddListener(OnRetryBtnClick);
    }

    void StartUpdate()
    {
        // 请求热更新
        m_versionText.text = string.Format("app: {0} res: {1}", VersionMgr.instance.appVersion, VersionMgr.instance.resVersion);
        m_progressSlider.value = 0;
        m_progressText.text = "0%";
        m_tipsText.text = "正在请求更新，请稍等...";

        m_hotUpdater = new HotUpdater();
        m_hotUpdater.actionForceFullAppUpdate = ShowForceAppUpdateDlg;
        m_hotUpdater.actionWeakFullAppUpdate = ShowUnForceAppUpdateDlg;
        m_hotUpdater.actionShowErrorTips = ShowErrorTips;
        m_hotUpdater.actionUpdateTipsText = UpdateTipsText;
        m_hotUpdater.actionNothongUpdate = NothingUpdate;
        m_hotUpdater.actionUpdateProgress = UpdateProgress;
        m_hotUpdater.actionAllDownloadDone = OnAllDownloadDone;
        m_hotUpdater.Init();
        m_hotUpdater.Start();
    }

    protected override void Update()
    {
        m_hotUpdater.Update();
    }

    private void ShowForceAppUpdateDlg()
    {
        m_appUpdateDlg.SetActive(true);
        m_nextBtn.gameObject.SetActive(false);
    }

    private void ShowUnForceAppUpdateDlg()
    {
        m_appUpdateDlg.SetActive(true);
        m_nextBtn.gameObject.SetActive(true);
    }

    private void ShowErrorTips(UnityWebRequest.Result error)
    {
        m_errorTipsDlg.SetActive(true);
        switch (error)
        {
            case UnityWebRequest.Result.ConnectionError:
                {
                    m_errorText.text = "连击更新服务器失败，请重试！";
                }
                break;
            case UnityWebRequest.Result.DataProcessingError:
                {
                    m_errorText.text = "下载异常，请检查网络并重试！";
                }
                break;
            default:
                {
                    m_errorText.text = "请求更新服务器失败，请重试！";
                }
                break;
        }
    }

    private void UpdateTipsText(string tipsStr)
    {
        m_tipsText.text = tipsStr;
    }

    private void NothingUpdate()
    {
        Finish();
    }

    private void OnAllDownloadDone()
    {
        Finish();
    }

    private void OnNextBtnClick()
    {
        m_appUpdateDlg.SetActive(false);
        m_hotUpdater.DoNextTime();
    }

    private void OnFullAppUpdateBtnClick()
    {
        m_hotUpdater.DoFullAppUpdate();
    }

    private void OnRetryBtnClick()
    {
        m_errorTipsDlg.SetActive(false);
        m_hotUpdater.Start();
    }



    /// <summary>
    /// 更新进度条
    /// </summary>
    /// <param name="value"></param>
    private void UpdateProgress(float value)
    {
        m_progressSlider.value = value < 0.05f ? 0.05f : value;
        m_progressText.text = (100 * value).ToString("0.00") + "%";
    }

    private void Finish()
    {
        PanelMgr.instance.HidePanel(1);
        m_cb?.Invoke();
    }

    private void OnApplicationQuit()
    {
        m_hotUpdater.Dispose();
    }
}
