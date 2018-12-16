#define USING_NGUI
//#define USING_UGUI

using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;


namespace com.tencent.pandora.tools
{
    public class LuaCodeGererator
    {
        private Dictionary<string, string> _functionData;
        private Dictionary<string, string> _componentData;
        private string _findGameObjectFormater = "self.{0} = self.transform:Find(\"{1}\").gameObject;";
        private string _findComponentFormater = "self.{0} = self.transform:Find(\"{1}\"):GetComponent({2});";

#if USING_NGUI
        private string _buttonBindFunctionFormater = "self.buttonFunctionMap[self.panel.{0}] = function(go) self:{1}(go); end;";
#endif

#if USING_UGUI
        private string _buttonBindFunctionFormater = "self.eventFunctionMap[self.panel.{0}:GetComponent("Button").onClick] = function() self:{1}(); end;";
#endif
        private string _functionFormater = "function mt:{0}()\r\n\t--todo add your code \r\nend;";


        private void LoadData()
        {
            _functionData = LuaCodeRecorder.Read(DataType.Function, LuaCodeFiller.Instance.ActionRoot.name);
            _componentData = LuaCodeRecorder.Read(DataType.Component, LuaCodeFiller.Instance.ActionRoot.name);
        }

        private void WriteData()
        {
            LuaCodeRecorder.Write(DataType.Function, LuaCodeFiller.Instance.ActionRoot.name, _functionData);
            LuaCodeRecorder.Write(DataType.Component, LuaCodeFiller.Instance.ActionRoot.name, _componentData);
        }

        public void GenerateFindChildCode(List<FillerElement> list)
        {
            foreach (var item in list)
            {

            }
        }

        public string GenerateLuaVariableName(Transform trans, string componentName)
        {
            string name = TransName(trans.name);

            if (componentName != "self" && name.Contains(componentName) == false)
            {
                name = name + componentName;
            }
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

        private string TransName(string originalName)
        {
            string[] nameSplits = originalName.Split('_');
            if (nameSplits.Length != 2)
            {
                throw new ArgumentException("the name of originalName must be segmented by ‘_’,eg. Button_close.", "originalName");
            }
            return nameSplits[1] + nameSplits[0];
        }

        private string JointName(string left, string right)
        {
            string capitalizedName = right.Substring(0, 1).ToUpper() + right.Substring(1);
            return left + capitalizedName;
        }

        private bool IsVariableNameExisted(string name)
        {
            var keys = _componentData.Keys;
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
        private string GetTransformPath(Transform trans)
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