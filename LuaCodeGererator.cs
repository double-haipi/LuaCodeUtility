/*
思想:
 * 所有的组件都会存放在_componentData中
 * 选定的button,需要绑定事件的,放到_functionData中
 * 
 
 */

using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;


namespace com.tencent.pandora.tools
{
    public class LuaCodeGererator
    {

        private Dictionary<string, string> _componentDataDict = new Dictionary<string, string>();
        private Dictionary<string, string> _functionDataDict = new Dictionary<string, string>();
        private Dictionary<string, string> _buttonFunctionMapDict = new Dictionary<string, string>();
        private string _findGameObjectFormater = "self.{0} = self.transform:Find(\"{1}\").gameObject;";
        private string _findComponentFormater = "self.{0} = self.transform:Find(\"{1}\"):GetComponent({2});";

        //这个稍后要改为UIButton添加的形式
        private string _buttonBindFunctionFormater = "self.buttonFunctionMap[self.panel.{0}] = function() self:{1}(self.panel.{0}); end;";

        private string _functionFormater = "function mt:{0}()\r\n\t--todo add your code \r\nend;";


        public Dictionary<string, string> ComponentDataDict
        {
            get { return _componentDataDict; }
            set { _componentDataDict = value; }
        }
        public Dictionary<string, string> FunctionDataDict
        {
            get { return _functionDataDict; }
            set { _functionDataDict = value; }
        }

        public Dictionary<string, string> ButtonFunctionMapDict
        {
            get { return _buttonFunctionMapDict; }
            set { _buttonFunctionMapDict = value; }
        }

        public void GenerateLuaCode( List<FillerElement> list )
        {
            GenerateFindChildCode(list);
        }

        private void GenerateFindChildCode( List<FillerElement> list )
        {
            foreach (var item in list)
            {
                // self 自身
                if (item.GameObjectSelected == true)
                {
                    string variableName = GenerateLuaVariableName(item.CachedTransform, "self");
                    string code = string.Format(_findGameObjectFormater, variableName, GetTransformPath(item.CachedTransform));
                    _componentDataDict.Add(variableName, code);
                }
                //组件
                foreach (var innerItem in item.ComponentsDict)
                {
                    if (innerItem.Value == true)
                    {
                        string variableName = GenerateLuaVariableName(item.CachedTransform, innerItem.Key);
                        string code = string.Format(_findComponentFormater, variableName, GetTransformPath(item.CachedTransform), innerItem.Key);
                        _componentDataDict.Add(variableName, code);
                        GenerateBindFunctionCode(variableName, innerItem.Key);
                    }
                }
            }

        }

        public void GenerateBindFunctionCode( string variableName, string componentName )
        {
            if (componentName.Contains("Button"))
            {
                string functionName = JointName("On", variableName + "Click");
                string functionCode = string.Format(_functionFormater, functionName);
                string bindCode = string.Format(_buttonBindFunctionFormater, variableName, functionName);

                _functionDataDict.Add(variableName, functionCode);
                _buttonFunctionMapDict.Add(variableName, bindCode);
            }
        }

        //prefab中,各节点的名字格式类似于Button_close,转化为lua 变量名为closeButton,如果重名,则在前面前父节点的名字限定.
        //要做重名处理(这里要测试重名处理情况)

        public string GenerateLuaVariableName( Transform trans, string componentName )
        {
            string name = TransName(trans.name);
            //如果是gameObject,Lua变量名加GameObject后缀
            if (componentName == "self")
            {
                name = name + "GameObject";
            }

            //如果是component,则lua变量名加上component的名字
            //if (componentName != "self" && name.Contains(componentName) == false)
            //{
            //    name = name + componentName;
            //}

            string pathInHierarchy = GetTransformPath(trans);
            string[] pathSplits = pathInHierarchy.Split('/');
            int index = pathSplits.Length - 1;
            while (IsVariableNameExisted(name) == true)
            {
                if (index < 0)
                {
                    throw new ArgumentException(string.Format("can not auto gererate an unique lua variabel name for {0},please modify its name or its parent node name.", pathInHierarchy), "trans");
                }
                name = JointName(TransName(pathSplits[index]), name);
                index--;
            }

            return name;
        }

        private string TransName( string originalName )
        {
            string[] nameSplits = originalName.Split('_');
            if (nameSplits.Length != 2)
            {
                throw new ArgumentException("originalName must be segmented by ‘_’,eg. Button_close.", "originalName");
            }
            return nameSplits[1] + nameSplits[0];
        }

        private string JointName( string left, string right )
        {
            string capitalizedName = right.Substring(0, 1).ToUpper() + right.Substring(1);
            return left + capitalizedName;
        }

        private bool IsVariableNameExisted( string name )
        {
            var keys = _componentDataDict.Keys;
            foreach (var item in keys)
            {
                if (item == name)
                {
                    return true;
                }
            }
            return false;
        }

        //path 是相对于当前活动面板的
        private string GetTransformPath( Transform trans )
        {
            Transform parentTrans = trans;
            if (LuaCodeFiller.Instance.ActionRoot == null || parentTrans == null)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            while (parentTrans != null)
            {
                sb.Insert(0, parentTrans.name);
                sb.Insert(0, "/");
                parentTrans = parentTrans.parent;
            }
            string path = sb.ToString(1, sb.Length - 1);
            int rootNodeNameIndex = path.IndexOf(LuaCodeFiller.Instance.ActionRoot.name);
            int subIndex = rootNodeNameIndex + LuaCodeFiller.Instance.ActionRoot.name.Length + 1;
            if (rootNodeNameIndex != -1 && subIndex < path.Length)
            {
                return path.Substring(rootNodeNameIndex + LuaCodeFiller.Instance.ActionRoot.name.Length + 1);
            }
            else
            {
                return "";
            }
        }

    }
}