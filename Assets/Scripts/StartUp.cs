using UnityEngine;
using LuaFramework;

/// <summary>
/// 启动脚本
/// </summary>
public class StartUp : MonoBehaviour
{
    void Awake()
    {
        // 热更新之前初始化一些模块
        InitBeforeHotUpdate();
        // 热更新
        HotUpdatePanel.Show(() =>
        {
            // 热更新后初始化一些模块
            InitAfterHotUpdate();
            // 启动游戏
            StartGame();
        });
    }

    private void StartGame()
    {
        //启动lua框架//
        LuaFramework.AppFacade.Instance.StartUp(() =>
        {
            LuaCall.CallFunc("Main.Init");
            LuaCall.CallFunc("Main.Start");

            // 监听关闭游戏事件
            AppQuitDefend.Init();
        });
    }

    /// <summary>
    /// 热更新之前初始化一些模块
    /// </summary>
    private void InitBeforeHotUpdate()
    {
        m_networkMsgEventRegister = new NetworkMsgEventRegister();
        // 限制游戏帧数
        Application.targetFrameRate = AppConst.GameFrameRate;
        // 手机常亮//
        Screen.sleepTimeout = -1;
        // 后台运行//
        Application.runInBackground = true;

        // 日志
        GameLogger.Init();
        LogCat.Init();
        // 网络消息注册
        m_networkMsgEventRegister.RegistNetworkMsgEvent();
        // 界面管理器
        PanelMgr.instance.Init();

        // 版本号
        VersionMgr.instance.Init();

        // 预加载AssetBundle
        AssetBundleMgr.instance.PreloadAssetBundles();
        // TODO 加载必要的资源AssetBundle


        TimerThread.instance.Init();
        ClientNet.instance.Init();
        ScreenCapture.Init();
    }

    /// <summary>
    /// 热更新后初始化一些模块
    /// </summary>
    private void InitAfterHotUpdate()
    {
        // 资源管理器
        ResourceManager.instance.Init();
        // 音效管理器
        AudioMgr.instance.Init();
        // 多语言
        LanguageMgr.instance.Init();
        I18N.instance.Init();
        // 图集管理器
        SpriteManager.instance.Init();
    }

    private void Update()
    {
        AppFacade.Instance.UpdateEx();
    }

    private void LateUpdate()
    {
        AppFacade.Instance.LateUpdateEx();
    }

    private void FixedUpdate()
    {
        AppFacade.Instance.FixedUpdateEx();
    }

    private NetworkMsgEventRegister m_networkMsgEventRegister;
}
