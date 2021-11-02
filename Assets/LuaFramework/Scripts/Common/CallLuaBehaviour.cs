using UnityEngine;
using System.Collections;

//调用lua里面某个函数
public class CallLuaBehaviour : MonoBehaviour
{
    //lua函数名称
    public string luaFunName;
    //参数
    public string[] pamamList;

	// Use this for initialization
	void Start ()
    {
        if (pamamList != null && pamamList.Length > 0)
        {
            LuaCall.CallFunc(luaFunName, gameObject, pamamList);
        }
        else
        {
            LuaCall.CallFunc(luaFunName, gameObject);
        }
    }

}
