using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class PostBuildProcessor
{
    [PostProcessBuild(999)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        // Thêm URL Types (deep link)
        PlistElementDict rootDict = plist.root;

        PlistElementArray urlTypes = rootDict.CreateArray("CFBundleURLTypes");
        PlistElementDict urlDict = urlTypes.AddDict();
        urlDict.SetString("CFBundleURLName", "xheroscan.deeplink");
        PlistElementArray urlSchemes = urlDict.CreateArray("CFBundleURLSchemes");
        urlSchemes.AddString("xheroscan");

        plist.WriteToFile(plistPath);
    }
}