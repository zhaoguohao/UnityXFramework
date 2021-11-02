#define LUA_VERSION_5_3

using UnityEngine;
using LuaInterface;

public class ToLua_UnityEngine_Input
{
#if LUA_VERSION_5_3
    public static string GetTouchDefined =
@"        try
        {
		    int arg0 = (int)LuaDLL.luaL_checknumber(L, 1);
            int arg1 = (int)LuaDLL.luaL_optinteger(L, 2, TouchBits.ALL);        
		    UnityEngine.Touch o = UnityEngine.Input.GetTouch(arg0);
            ToLua.Push(L, o, arg1);
            return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);			
		}";
#else
      public static string GetTouchDefined =
@"        try
        {
		    int arg0 = (int)LuaDLL.luaL_checknumber(L, 1);
            int arg1 = LuaDLL.luaL_optinteger(L, 2, TouchBits.ALL);        
		    UnityEngine.Touch o = UnityEngine.Input.GetTouch(arg0);
            ToLua.Push(L, o, arg1);
            return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);			
		}";
#endif


    [UseDefinedAttribute]
    public static Touch GetTouch(int index, int flag)
    {        
        return new Touch();
    }
}
