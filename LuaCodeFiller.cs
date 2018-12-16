#define USING_NGUI
//#define USING_UGUI
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

/*
 1.数据先直接使用从树形结构转化过来的列表形式，后面再处理树形结构的收放
 2.对齐放置元素。
 3.Editor下的变量在代码重新编译后，要重新初始化
 
 */

namespace com.tencent.pandora.tools
{
    public class LuaCodeFiller
    {
        private static LuaCodeFiller _instance;
        private const string _actionName = "ACTION_NAME";
        private Transform _actionRoot;
        private TreeModel<FillerElement> _actionData;
        private List<FillerElement> _actionDataList;

       

        private Dictionary<Transform, FillerElement> _selected;

        //可以展示的component类型名
#if USING_NGUI
        private List<string> _componentFilter = new List<string> { "UILabel", "UIScrollView", "UIEventListener", "UISprite", "UIPanel" };
#endif

#if USING_UGUI
        private string[] _componentFilter = { "", "", };
#endif

        public static LuaCodeFiller Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LuaCodeFiller();
                    _instance.SetActionRoot();
                }
                return _instance;
            }
        }
        public string ActionName { get { return _actionName; } }

        public Transform ActionRoot { get { return _actionRoot; } set { _actionRoot = value; } }

        public List<FillerElement> DataList { get { return _actionDataList; } }

        public void SetActionRoot()
        {
            //如果_root 为空，自动赋值
            if (_actionRoot == null)
            {
                string actionName = EditorPrefs.GetString(_actionName, "");
                if (actionName != "")
                {
                    GameObject go = GameObject.Find(actionName);
                    _actionRoot = (go == null) ? null : go.transform;
                }
            }
            UpdateActionData();
        }

        private void UpdateActionData()
        {
            UpdateActionDataList();
            //todo 生成树形结构
            Debug.Log("");

        }
        private void UpdateActionDataList()
        {
            if (_actionDataList == null)
            {
                _actionDataList = new List<FillerElement>();
            }
            _actionDataList.Clear();

            Recursive(_actionRoot, -1, _actionDataList);
        }

        private void Recursive(Transform trans, int depth, List<FillerElement> dataList)
        {
            FillerElement element = new FillerElement();
            element.CachedTransform = trans;
            element.name = trans.name;
            element.id = trans.gameObject.GetInstanceID();
            element.depth = depth;
            element.ComponentsDict = new Dictionary<string, bool>();

            List<string> componentNames = GetFilterdComponentNames(trans);
            foreach (var item in componentNames)
            {
                if (!element.ComponentsDict.ContainsKey(item))
                {
                    element.ComponentsDict.Add(item, false);
                }
            }

            dataList.Add(element);

            if (trans.childCount > 0)
            {
                depth++;
                for (int i = 0, length = trans.childCount; i < length; i++)
                {
                    Recursive(trans.GetChild(i), depth, dataList);
                }
            }

        }


        private List<string> GetFilterdComponentNames(Transform trans)
        {
            List<string> result = new List<string>();
            string pattern = @"\([^)]*\)";

            var components = trans.GetComponents<Component>();
            foreach (var item in components)
            {
                var matches = Regex.Matches(item.ToString(), pattern);
                string lastMatch = matches[matches.Count - 1].Value;
                string componentName = lastMatch.Trim('(', ')');

                if (_componentFilter.Contains(componentName))
                {
                    result.Add(componentName);
                }
            }
            return result;
        }

  

    }


    public class FillerElement : TreeElement
    {
        [SerializeField]
        private bool _gameObjectSelected = false;
        private Dictionary<string, bool> _componentsDict;
        private Transform _cachedtransform;

        public bool GameObjectSelected { get { return _gameObjectSelected; } set { _gameObjectSelected = value; } }
        public Dictionary<string, bool> ComponentsDict { get { return _componentsDict; } set { _componentsDict = value; } }

        public Transform CachedTransform { get { return _cachedtransform; } set { _cachedtransform = value; } }
    }
}