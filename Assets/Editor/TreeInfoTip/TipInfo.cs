using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeInfoTip
{
    public struct TipInfo
    {
        public string path;
        public string title;
        public string guid;
        public bool isShow;

        public TipInfo(string path, string title, string guid, bool isShow)
        {
            this.path = path;
            this.title = title;
            this.guid = guid;
            this.isShow = isShow;
        }
    }
}