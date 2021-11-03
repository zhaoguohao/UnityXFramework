using UnityEngine;
using System;

/// <summary>
/// 动画事件触发器
/// </summary>
public class AnimationEventTrigger : MonoBehaviour
{
    public Action<string> aniEvent;

    public void AniMsgStr(string msg)
    {
        aniEvent?.Invoke(msg);
    }
}
