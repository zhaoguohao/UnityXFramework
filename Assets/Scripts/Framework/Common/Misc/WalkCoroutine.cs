using System.Collections;
using UnityEngine;


/// <summary>
/// 按一定的时间间隔遍历
/// </summary>
public class WalkCoroutine : MonoBehaviour
{
    public static void DoWalk(float interval, int step, System.Predicate<int> cb)
    {
        var go = new GameObject("WalkCoroutine");
        var bhv = go.AddComponent<WalkCoroutine>();
        bhv.Init(interval, step, cb);
    }

    public void Init(float interval, int step, System.Predicate<int> cb)
    {
        StartCoroutine(CoFunc(interval, step, cb));
    }

    IEnumerator CoFunc(float interval, int step, System.Predicate<int> cb)
    {
        int index = 0;
        while(true)
        {
            bool res = cb(index++);
            if(!res) break;
            if(index % step == 0 && 0 != interval)
                yield return new WaitForSeconds(interval);
        }
        Destroy(gameObject);
    }
}
