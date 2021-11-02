using UnityEngine;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LuaFramework {
    public class LuaBehaviour : Base {
        private string data = null;
        private AssetBundle bundle = null;
        private Dictionary<string, LuaFunction> buttons = new Dictionary<string, LuaFunction>();

        protected void Awake() {
            LuaCall.CallFunc(name+".Awake", gameObject);
        }

        protected void Start() {
            LuaCall.CallFunc(name+".Start");
        }

        protected void OnClick() {
            LuaCall.CallFunc(name+".OnClick");
        }

        protected void OnClickEvent(GameObject go) {
            LuaCall.CallFunc(name+".OnClick", go);
        }

        /// <summary>
        /// 初始化面板
        /// </summary>
        public void OnInit(AssetBundle bundle, string text = null) {
            this.data = text;   //初始化附加参数
            this.bundle = bundle; //初始化
            Debug.LogWarning("OnInit---->>>" + name + " text:>" + text);
        }

        /// <summary>
        /// 获取一个GameObject资源
        /// </summary>
        /// <param name="name"></param>
        public GameObject LoadAsset(string name) {
            if (bundle == null) return null;

            return bundle.LoadAsset(name, typeof(GameObject)) as GameObject;

        }


        /// <summary>
        /// 删除单击事件
        /// </summary>
        /// <param name="go"></param>
        public void RemoveClick(GameObject go) {
            if (go == null) return;
            LuaFunction luafunc = null;
            if (buttons.TryGetValue(go.name, out luafunc)) {
                buttons.Remove(go.name);
                luafunc.Dispose();
                luafunc = null;
            }
        }

        /// <summary>
        /// 清除单击事件
        /// </summary>
        public void ClearClick() {
            foreach (var de in buttons) {
                if (de.Value != null) {
                    de.Value.Dispose();
                }
            }
            buttons.Clear();
        }
        
        //-----------------------------------------------------------------
        protected void OnDestroy() {
            if (bundle) {
                bundle.Unload(true);
                bundle = null;  //销毁素材
            }
            ClearClick();
            Util.ClearMemory();
            Debug.Log("~" + name + " was destroy!");
        }
    }
}
