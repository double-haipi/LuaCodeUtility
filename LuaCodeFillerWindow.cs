using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace com.tencent.pandora.tools
{
    public class LuaCodeFillerWindow : EditorWindow
    {
        private LuaCodeFiller _filler;
        private Vector2 _scrollPosition = Vector2.zero;

        //120:预留8层结构的缩进,150:title字体最大宽度
        private const int OPTIONS_LEFT_PADDING = 270;

        #region GUI
        [MenuItem("GameObject/LuaCodeFiller", priority = 11)]
        private static void Init()
        {
            EditorWindow.GetWindow(typeof(LuaCodeFillerWindow), false, "LuaCodeFiller", true).Show();
        }

        private void OnEnable()
        {
            //Selection.selectionChanged = OnSelectionChanged;
            if (_filler == null)
            {
                _filler = LuaCodeFiller.Instance;
            }
        }


        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            DrawActionPanelField();
            DrawDataTree();
            DrawButtons();
            EditorGUILayout.EndVertical();
        }

        private void DrawActionPanelField()
        {
            _filler.ActionRoot = (Transform)EditorGUILayout.ObjectField("ActionPanel：", _filler.ActionRoot, typeof(Transform), true);
            if (_filler.ActionRoot != null && _filler.ActionRoot.name != EditorPrefs.GetString(_filler.ActionName))
            {
                EditorPrefs.SetString(_filler.ActionName, _filler.ActionRoot.name);
                _filler.SetActionRoot();
            }
        }

        private void DrawDataTree()
        {
            GUILayout.Space(5f);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.BeginHorizontal();
            FillerElement root = _filler.DataRoot;
            int index = 0;
            int columns = 0;
            RecursiveDraw(root, ref index, ref columns);

            GUILayout.Space(OPTIONS_LEFT_PADDING + columns * 120);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space((index + 1) * 20);
            EditorGUILayout.EndScrollView();
        }

        private void RecursiveDraw( FillerElement element, ref int index, ref int columns )
        {
            DrawElementTitle(element, index);
            //绘制选择项
            if (element.name != _filler.ActionRoot.name)
            {
                DrawElementOptions(element, index, ref columns);
            }
            index++;

            if (element.IsFold == true || element.children == null)
            {
                return;
            }

            for (int i = 0, length = element.children.Count; i < length; i++)
            {
                RecursiveDraw((FillerElement)element.children[i], ref index, ref columns);
            }

        }

        private void DrawElementTitle( FillerElement element, int index )
        {
            //title rect 宽 150
            Rect rect = new Rect((element.depth + 2) * 15, index * 20, 150, 15);

            if (element.hasChildren)
            {
                //有子节点的绘制展开标签
                string title = "";
                if (element.IsFold)
                {
                    title = "\u25BA" + element.name;
                }
                else
                {
                    title = "\u25BC" + element.name;
                }

                bool isFold = GUI.Toggle(rect, element.IsFold, title, "PreToolbar2");
                if (isFold != element.IsFold)
                {
                    element.IsFold = isFold;
                    Repaint();
                }
            }
            else
            {
                GUI.Label(rect, element.name);
            }

        }

        private void DrawElementOptions( FillerElement element, int index, ref int columns )
        {
            //option 的 宽120,高15,
            Rect rectForSelf = new Rect(OPTIONS_LEFT_PADDING, index * 20, 120, 15);
            element.GameObjectSelected = GUI.Toggle(rectForSelf, element.GameObjectSelected, "self");

            Dictionary<string, bool> componentsDict = element.ComponentsDict;
            List<string> keys = new List<string>(componentsDict.Keys);
            for (int i = 0, length = keys.Count; i < length; i++)
            {
                Rect rectForComponent = new Rect(OPTIONS_LEFT_PADDING + (i + 1) * 120, index * 20, 120, 15);
                componentsDict[keys[i]] = GUI.Toggle(rectForComponent, componentsDict[keys[i]], keys[i]);
            }

            int currentColumns = keys.Count + 1;
            if (columns < currentColumns)
            {
                columns = currentColumns;
            }
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(80f)))
            {
                _filler.UpdateActionData();
            }

            if (GUILayout.Button("Execute", GUILayout.Width(80f)))
            {
                //复制一份数据,否则数据变化会导致无法使用foreach遍历
                List<FillerElement> newDataList = new List<FillerElement>();
                foreach (var item in _filler.DataList)
                {
                    FillerElement element = new FillerElement();
                    element.name = item.name;
                    element.id = item.id;
                    element.depth = item.depth;
                    element.CachedTransform = item.CachedTransform;
                    element.GameObjectSelected = item.GameObjectSelected;
                    Dictionary<string, bool> componentDict = new Dictionary<string, bool>();
                    foreach (var innerItem in item.ComponentsDict)
                    {
                        componentDict.Add(innerItem.Key, innerItem.Value);
                    }
                    element.ComponentsDict = componentDict;
                    newDataList.Add(element);
                }
                _filler.Fill(newDataList);
            }
            EditorGUILayout.EndHorizontal();
        }


        #endregion


    }
}
