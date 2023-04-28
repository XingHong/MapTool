using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExportTool
{
    [MenuItem("MapTool/GenData")]
    public static void GenData()
    {
        GenLuaCode();
    }

    static void GenLuaCode()
    {
        string luaFilePath = Path.Combine(Application.dataPath, "MapData.lua");
        StreamWriter file = new StreamWriter(luaFilePath, false, Encoding.UTF8);
        file.NewLine = System.Environment.NewLine;
        List<ExportData> list = PainTool.GenEdgeInfo();
        file.WriteLine("-- auto generate by maptool, number x represent x_0000");
        file.WriteLine("MapData = {");
        for (int i = 0; i < list.Count; ++i)
        {
            var item = list[i];
            if (string.IsNullOrEmpty(item.dirInfo))
            {
                file.WriteLine(item.groupId + ",");
            }
            else
            {
                file.WriteLine("\"" + item.groupId + "_" + item.dirInfo + "\",");
            }
        }
        file.WriteLine("}");
        file.Close();
        AssetDatabase.Refresh();
    }
}

public class ExportData
{
    public int groupId = 0;
    public string dirInfo = string.Empty;
}
