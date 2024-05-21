using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Windows;

#if UNITY_EDITOR
public class SF2X_BuildDateTXTFile : UnityEditor.Build.IPreprocessBuildWithReport
{
    public int callbackOrder => 0;
    public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
    {
        using (BinaryWriter Writer = new BinaryWriter(System.IO.File.Open("Assets/Game/Resources/BuildDate.txt", FileMode.OpenOrCreate, FileAccess.Write)))
        {
             var dt = DateTime.Now.ToLocalTime();
             Writer.Write(dt.ToString("dd MMM:HH.mm"));
        }
    }
  
}
#endif