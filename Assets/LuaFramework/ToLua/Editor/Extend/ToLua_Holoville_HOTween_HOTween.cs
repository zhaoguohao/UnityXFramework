using UnityEngine;
using LuaInterface;
using Holoville.HOTween;

public class ToLua_Holoville_HOTween_HOTween
{
    public static string ReverseDefined =
        @"try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes(L, 1, typeof(bool)))
			{
				bool arg0 = LuaDLL.lua_toboolean(L, 1);
				int o = Holoville.HOTween.HOTween.Reverse(arg0, false);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(Holoville.HOTween.Tweener), typeof(bool)))
			{
				Holoville.HOTween.Tweener arg0 = (Holoville.HOTween.Tweener)ToLua.ToObject(L, 1);
				bool arg1 = LuaDLL.lua_toboolean(L, 2);
				int o = Holoville.HOTween.HOTween.Reverse(arg0, arg1);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(Holoville.HOTween.Sequence), typeof(bool)))
			{
				Holoville.HOTween.Sequence arg0 = (Holoville.HOTween.Sequence)ToLua.ToObject(L, 1);
				bool arg1 = LuaDLL.lua_toboolean(L, 2);
				int o = Holoville.HOTween.HOTween.Reverse(arg0, arg1);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(string), typeof(bool)))
			{
				string arg0 = ToLua.ToString(L, 1);
				bool arg1 = LuaDLL.lua_toboolean(L, 2);
				int o = Holoville.HOTween.HOTween.Reverse(arg0, arg1);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(int), typeof(bool)))
			{
				int arg0 = (int)LuaDLL.lua_tonumber(L, 1);
				bool arg1 = LuaDLL.lua_toboolean(L, 2);
				int o = Holoville.HOTween.HOTween.Reverse(arg0, arg1);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes(L, 1, typeof(object), typeof(bool)))
			{
				object arg0 = ToLua.ToVarObject(L, 1);
				bool arg1 = LuaDLL.lua_toboolean(L, 2);
				int o = Holoville.HOTween.HOTween.Reverse(arg0, arg1);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, ""invalid arguments to method: Holoville.HOTween.HOTween.Reverse"");
			}
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}";

    [UseDefinedAttribute]
    public static int Reverse(Tweener p_tweener, bool p_forcePlay = false)
    {
        return 0;
    }

}
