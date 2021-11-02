
using LuaInterface;

namespace LuaFramework
{
    class WrapCSObj
    {
        public object obj;
        System.Type type;
   
        public WrapCSObj(object obj)
        {
            this.obj = obj;
            type = obj.GetType();
        }
        public static void Register(LuaState L)
        {
            L.BeginClass(typeof(WrapCSObj), typeof(System.Object));
            L.RegFunction("__index", Index);
            L.RegFunction("__call", CreateNew);
            L.EndClass();
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int CreateNew(System.IntPtr L)
        {
            try
            {
                ToLua.CheckArgsCount(L, 2);
                var obj = ToLua.ToObject(L, 2);
                var wrap = new WrapCSObj(obj);
                ToLua.Push(L, wrap);
                return 1;
            }
            catch (System.Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int Index(System.IntPtr L)
        {
            try
            {
                ToLua.CheckArgsCount(L, 2);
                var wrap = (WrapCSObj)ToLua.CheckObject(L, 1, typeof(WrapCSObj));
                string name = ToLua.ToString(L, 2);
                var val = wrap.GetVarByName(name);
                ToLua.Push(L, val);
                return 1;
            }
            catch (System.Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        public object GetVarByName(string name)
        {
            var pi = type.GetProperty(name);
            if (pi != null)
                return pi.GetValue(obj, null);
            var fi = type.GetField(name);
            if (fi != null)
                return fi.GetValue(obj);
            return null;
        }
    }// end class WrapCSObj
}// end namespace LuaFramework
