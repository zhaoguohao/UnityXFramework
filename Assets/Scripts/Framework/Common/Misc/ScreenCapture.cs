using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 截屏
/// </summary>
public class ScreenCapture 
{
    public static void Init()
    {
        for (int i = 0, len = s_camNameList.Length; i < len; ++i)
        {
            var camName = s_camNameList[i];
            var camObj = GameObject.Find(camName);
            if (null != camObj)
            {
                var cam = camObj.GetComponent<Camera>();
                s_camList.Add(cam);
            }
            else
            {
                GameLogger.LogError("Camera does not exist, name: " + camName);
            }
        }
    }

    /// <summary>
    /// 截屏接口
    /// </summary>
    /// <returns></returns>
    public static Texture2D StartScreenCapture()
    {
        int width = Screen.width;
        int height = Screen.height;
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        for (int i = 0, cnt = s_camList.Count; i < cnt; ++i)
        {
            var cam = s_camList[i];
            cam.targetTexture = rt;
            cam.Render();
        }
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = null;
        for (int i = 0, cnt = s_camList.Count; i < cnt; ++i)
        {
            var cam = s_camList[i];
            cam.targetTexture = null;
        }
        Object.Destroy(rt);
        tex.Apply();
        return tex;
    }

    public static List<Camera> s_camList = new List<Camera>();

    /// <summary>
    /// 摄像机列表，注意顺序会影响渲染顺序，如果新增相机，要把相机添加到数组中
    /// </summary>
    private static string[] s_camNameList = new string[] { "Main Camera", "UICamera" };
}
