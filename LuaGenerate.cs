using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assets.game.Editor;
using JetBrains.Annotations;

public class LuaGenerate
{
    [CanBeNull]
    public static string GetGameObjectLayerout(Transform root, Transform ts)
    {
        if (ts.Equals(root))
        {
            return null;
        }

        Transform t = ts;
        List < string > list = new List<string>();
        while (!t.Equals(root))
        {
            list.Add(t.name);
            t = t.parent;
        }

        StringBuilder sb = new StringBuilder();
        for(int i=list.Count-1; i>=0; i--)
        {
            if (i==0)
            {
                sb.Append(list[i]);
            }
            else
            {
                sb.Append(list[i] + "/");
            }
        }
        return sb.ToString();
    }
    public static string GenerateTransformCode(GameObject go, ref List<string> btnList, ref List<string> hideList, ref List<string>  exportcompentList)
    {
        StringBuilder sb = new StringBuilder();

        RectTransform[] arr = go.GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform rt in arr)
        {
            bool isExport = rt.name.EndsWith("_");
            bool isHide = rt.name.EndsWith("_0");
            bool isBtn = rt.name.EndsWith("_btn");

            string[] splitarr = rt.name.Split('_');

            string key = splitarr[splitarr.Length - 1];
            if (splitarr.Length != 1)
            {
                if (key.Equals("0"))
                {

                }
                else if (key.Equals("btn"))
                {

                }
                else if (key.Equals(""))
                {

                }
                else if (key.Equals("model"))
                {

                }
                else
                {
                    ExportBaseType etype = new ExportBaseType(key);
                    sb.Append(etype.InitCompent(go.transform, rt));
                    exportcompentList.Add(rt.name);

                }
            }
 



            if (isExport || isHide || isBtn)
            {
                string strLayerout = GetGameObjectLayerout(go.transform, rt);
                if (string.IsNullOrEmpty(strLayerout))
                {
                    continue;
                }
                string strTrueName = rt.name;
      
                sb.AppendFormat("    self.{0} = self.trans:Find('{1}');\n", strTrueName, strLayerout);

                //需要hide
                if (isHide)
                {
                    hideList.Add(strTrueName);
                    
                }

                //需要注册按钮事件的
                if (isBtn)
                {
                    btnList.Add(strTrueName);
                }
            }
        }

        return sb.ToString();
    }

    [MenuItem("Assets/生成Lua")]
    static void GenerateLuaCode()
    {
        /*
         * 以_结尾的表示需要导出lua代码
         * 以_0结尾的导出，并设置为隐藏
         * 以_btn结尾的导出，并注册onclick事件
         */ 
        GenerateViewCode();
        GenerateDataCode();
    }

    static void GenerateViewCode()
    {

        GameObject go = Selection.activeObject as GameObject;
        if (null == go)
        {
            Debugger.LogError("没有选中prefab");
            return;
        }
        string strClassName = go.name;
        string strPath = AssetDatabase.GetAssetPath(go);
        string absolutePath;
        Debug.Log(strPath);
        absolutePath = Directory.GetCurrentDirectory();
        Debug.Log(absolutePath);

        int idx = strPath.IndexOf("prefab/");
        string str = strPath.Substring(idx + "prefab/".Length);
        str = str.Replace(".prefab", "");

        //遍历GameObject取得所有的transform
        List<string> btnList = new List<string>();
        List<string> hideList = new List<string>();
        List<string> exportcompentList = new List<string>();
        string strInitCode = GenerateTransformCode(go, ref btnList, ref hideList,ref exportcompentList);

        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("{0} = BasePanel.new();", strClassName);
        sb.AppendLine();
        sb.AppendLine();


        sb.AppendFormat("function {0}:Open() \n" +
                        "    self:CreatePanel('{1}', '{0}');\n" +
                        "    return '{0}';\n" +
                        "end\n", strClassName, str);

        sb.AppendLine();
        sb.AppendFormat("function {0}:InitCompent(obj)\n" +
                        "    self.trans = obj.transform;\n" +
                        "{1}\n", strClassName, strInitCode);

        for (int i = 0; i < hideList.Count; i++)
        {
            string hideName = hideList[i];
            sb.AppendFormat("    self.{0}.gameObject:SetActive(false);\n", hideName);
        }

        sb.AppendFormat("    self:RegistEvent();\n" +
                        "\n", strClassName, strInitCode);
        sb.AppendLine();

        sb.AppendFormat("    {0}Data:SetUICompent(self);\n" +
                        "end\n", strClassName);
        sb.AppendLine();


        sb.AppendFormat("function {0}:RegistEvent()\n", strClassName);
        sb.AppendFormat("    local delegate = nil;\n");

        for (int i = 0; i < btnList.Count; i++)
        {
            string strBtn = btnList[i];

            sb.AppendFormat("    delegate = bridge.GetDelegateLuaFunction(self.{0}Clicked, self, nil, true);\n" +
                            "    EventTriggerListener.Get(self.{0}.gameObject).onClick = delegate;\n", strBtn);
            sb.AppendLine();
        }
        sb.Append("end\n");
        sb.AppendLine();


        for (int i = 0; i < btnList.Count; i++)
        {
            string strBtn = btnList[i];
            sb.AppendFormat("function {0}:{1}Clicked()\n", strClassName, strBtn);
            sb.AppendLine();
            sb.AppendFormat("     print('{0}:{1}')\n", strClassName, strBtn);
            sb.AppendLine();
            sb.Append("end\n");
            sb.AppendLine();
        }

        for (int i = 0; i < exportcompentList.Count; i++)
        {
            string exportcompent = exportcompentList[i];
            string[] exports = exportcompent.Split('_');
            sb.AppendFormat("function {0}:Set{1}(info)\n", strClassName, exportcompent);
            sb.AppendLine();
            if (exports[exports.Length-1] == "Text")
            {
                sb.AppendFormat("    self.{0}.text = info\n", exportcompent, exportcompent);
            }
            else if (exports[exports.Length - 1] == "Image")
            {

                sb.AppendFormat("    uiMgr:setSpriteInPanel(info, self.panelname, self.{0});\n", exportcompent);
            }

            sb.AppendLine();
            sb.Append("end\n");
            sb.AppendLine();


        }

        sb.AppendFormat("function {0}:OnDestroy() \n\t{0}Data.uicompent = nil\nend\n", strClassName);


        string codepath;

        codepath = absolutePath + "/Assets/Lua/ProjectLua/uimodule/" + strClassName + ".lua";
        if (!File.Exists(codepath))
        {
            var file = File.Create(codepath);
            file.Close();
            file.Dispose();

        }
        File.WriteAllText(codepath, sb.ToString(), Encoding.UTF8);
    }

    static void GenerateDataCode()
    {

        GameObject go = Selection.activeObject as GameObject;
        if (null == go)
        {
            Debugger.LogError("没有选中prefab");
            return;
        }
        string strClassName = go.name + "Data";
        var absolutePath = Directory.GetCurrentDirectory();

        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("{0} = BasePanelData.new();", strClassName);
        sb.AppendLine();
        sb.AppendLine();

        sb.AppendFormat("function {0}:{1}() \n\nend\n", strClassName, "Init");

        sb.AppendLine();

        sb.AppendFormat("function {0}:SetDataToUICompent()\n", strClassName);

        sb.AppendFormat("\n\tif self.serverackdata ~= nil then\n\t\tself:SetUicompent(self.serverackdata);\n\tend\n\nend\n");

        sb.AppendLine();

        sb.AppendFormat("function {0}:SetServerAckData(data)\n", strClassName);

        sb.AppendFormat("\n\tself.serverackdata = data;\n\tself:SetUicompent(self.serverackdata);\n\nend\n");
        sb.AppendLine();

        sb.AppendFormat("function {0}:SetUicompent(data)\n", strClassName);

        sb.AppendFormat("\n\tif self.uicompent ~= nil then\n\n\n\tend\n\nend\n");



        sb.AppendLine();

        sb.AppendFormat("{0}:Init();", strClassName);


        string codepath;

        codepath = absolutePath + "/Assets/Lua/ProjectLua/uimodule/" + strClassName + ".lua";
        if (!File.Exists(codepath))
        {
            var file = File.Create(codepath);
            file.Close();
            file.Dispose();

        }
        Debug.Log(codepath);
        File.WriteAllText(codepath, sb.ToString(), Encoding.UTF8);
    }


}