/********************************************************************
	Created: 2023/01/07
	License Copyright (c) hankangwen
	Author: hankangwen(hankangwen@qq.com)
	Github: https://github.com/hankangwen/Unity-TreeInfoTip
*********************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace TreeInfoTip
{
    public class TreeInfoTipManager
    {
        private static TreeInfoTipManager _instance;

        public static TreeInfoTipManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TreeInfoTipManager();
                return _instance;
            }
        }

        #region DirectoryV2
        private TreeInfoTipManager()
        {
            _directoryV2Path = EditorPrefs.GetString(_directoryV2SaveKey, _directoryV2Path);
        }

        private string _directoryV2SaveKey = "TreeInfoTip_DirectoryV2";
        private string _directoryV2Path = Application.dataPath + "/Editor/TreeInfoTip/DirectoryV2.xml";
        public string DirectoryV2Path
        {
            get => _directoryV2Path;
            set
            {
                if (!File.Exists(value))
                {
                    Debug.LogError($"Can not find file {value}, please check again.");
                    return;
                }
                _directoryV2Path = value;
                EditorPrefs.SetString(_directoryV2SaveKey, _directoryV2Path);
                _guid2Title = null;
            }
        }
        #endregion
        
        
        public bool IsOpen = true;  //是否开启TreeInfoTip
        
        private Dictionary<string, string> _guid2Title;
        public Dictionary<string, string> Guid2Title
        {
            get
            {
                if (_guid2Title == null) 
                    CreateGuid2Title();
                return _guid2Title;
            }
        }

        private readonly string PATH = "path";
        private readonly string TITLE = "title";
        private readonly string GUID = "guid";

        public bool AddToGuid2Title(string guid, string title, string path)
        {
            if (_guid2Title.ContainsKey(guid))
            {
                _guid2Title[guid] = title;
                UpdateDirectoryV2(guid, title, path);
            }
            else
            {
                _guid2Title.Add(guid, title);
                AddDirectoryV2(guid, title, path);
            }

            return true;
        }

        public string GetTitleByGuid(string guid)
        {
            if (Guid2Title.TryGetValue(guid, out string title))
            {
                return title;
            }
            return String.Empty;
        }

        private bool AddDirectoryV2(string guid, string title, string path)
        {
            string xmlPath = DirectoryV2Path;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            var root = xmlDoc.DocumentElement;
            
            var element = xmlDoc.CreateElement("tree");
            element.SetAttribute(PATH, path);
            element.SetAttribute(TITLE, title);
            element.SetAttribute(GUID, guid);
            root.AppendChild(element);
            // TODO:将指定的节点紧接着插入指定的引用节点之后
            // root.InsertAfter(element, element);
            
            xmlDoc.Save(xmlPath);
            
            AssetDatabase.Refresh();
            return true;
        }
        
        //替换
        private bool UpdateDirectoryV2(string guid, string title, string path)
        {
            string xmlPath = DirectoryV2Path;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            
            var treeNodes = xmlDoc.SelectNodes("trees/tree");
            if (treeNodes != null)
            {
                foreach (XmlNode node in treeNodes)
                {
                    var element = node as XmlElement;
                    if (element == null)
                        continue;

                    if (element.GetAttribute(GUID) == guid)
                    {
                        element.SetAttribute(PATH, path);
                        element.SetAttribute(TITLE, title);
                        break;
                    }
                }
            }
            xmlDoc.Save(xmlPath);
            AssetDatabase.Refresh();
            return true;
        }
        
        //创建
        private void CreateGuid2Title()
        {
            string xmlPath = DirectoryV2Path;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            bool isModify = false;
            var treeNodes = xmlDoc.SelectNodes("trees/tree");
            if (treeNodes != null)
            {
                _guid2Title = new Dictionary<string, string>(treeNodes.Count);
                foreach (XmlNode node in treeNodes)
                {
                    var element = node as XmlElement;
                    if (element == null)
                        continue;

                    string guid = element.GetAttribute(GUID);
                    string title = element.GetAttribute(TITLE);
                    _guid2Title.Add(guid, title);

                    string path = element.GetAttribute(PATH);
                    string guid2Path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path != guid2Path)
                    {
                        isModify = true;
                        element.SetAttribute(PATH, guid2Path);
                    }

                    //TODO KERVEN:Check file is exists.
                    // bool isDirectory = (File.GetAttributes(guid2Path) & FileAttributes.Directory) == FileAttributes.Directory;
                    // bool isExists = false;
                    // if (isDirectory)
                    // {
                    //     isExists = Directory.Exists(guid2Path);
                    // }
                    // else
                    // {
                    //     isExists = File.Exists(guid2Path);
                    // }
                    //
                    // if (!isExists)
                    // {
                    //     Debug.LogError($"File is delete, path = {guid2Path}");
                    // }
                }
            }

            if (isModify)
            {
                xmlDoc.Save(xmlPath);
                AssetDatabase.Refresh();
            }
        }
    }
}

