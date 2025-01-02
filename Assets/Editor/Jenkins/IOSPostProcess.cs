#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class IOSPostProcess : IPostprocessBuildWithReport
{
    public int callbackOrder => 100;

    public void OnPostprocessBuild(BuildReport report)
    {
        var pathToBuiltProject = report.summary.outputPath;
        var target = report.summary.platform;
        if (target != BuildTarget.iOS)
            return;
        Debug.LogFormat("Postprocessing build at \"{0}\" for target {1}", pathToBuiltProject, target);
        PBXProject project = new PBXProject();
        string pbxFilename = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
        //string pbxFilename = pathToBuiltProject + "/Unity-iPhone.xcworkspace/project.pbxproj";
        project.ReadFromFile(pbxFilename);

        string targetGUID = project.GetUnityMainTargetGuid();
        string targetGUID2 = project.TargetGuidByName("UnityFramework");

        // var token = project.GetBuildPropertyForAnyConfig(targetGUID, "USYM_UPLOAD_AUTH_TOKEN");
        // if (string.IsNullOrEmpty(token))
        // {
        //     token = "FakeToken";
        // }

        // project.SetBuildProperty(targetGUID, "USYM_UPLOAD_AUTH_TOKEN", token);
        // project.SetBuildProperty(targetGUID2, "USYM_UPLOAD_AUTH_TOKEN", token);
        // project.SetBuildProperty(project.ProjectGuid(), "USYM_UPLOAD_AUTH_TOKEN", token);
        //
        // var script =
        //     "rm -rf  \"${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/Frameworks/UnityFramework.framework/Frameworks\"\n" +
        //     "rm -rf  \"${CONFIGURATION_BUILD_DIR}/UnityFramework.framework/Frameworks\"";
        // project.InsertShellScriptBuildPhase(0, targetGUID, "RmUnityFramework", "/bin/sh -x", script);
        project.AddBuildProperty(
            targetGUID2,
            "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES",
            "NO");
        project.AddBuildProperty(
            targetGUID,
            "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES",
            "YES");
        project.AddBuildProperty(targetGUID,
            "GCC_ENABLE_OBJC_EXCEPTIONS",
            "YES");
        project.AddBuildProperty(targetGUID2,
            "GCC_ENABLE_OBJC_EXCEPTIONS",
            "YES");  
        project.AddBuildProperty(targetGUID,
            "CLANG_ENABLE_MODULES",
            "YES");
        project.AddBuildProperty(targetGUID2,
            "CLANG_ENABLE_MODULES",
            "YES");
        
        project.SetBuildProperty(targetGUID, "ENABLE_BITCODE", "NO");
        project.SetBuildProperty(targetGUID2, "ENABLE_BITCODE", "NO");
        project.WriteToFile(pbxFilename);

        var filePath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(filePath));
        var rootDict = plist.root;
        rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        // rootDict.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_ANALYTICS_STORAGE", false);
        // rootDict.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_STORAGE", false);
        // rootDict.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_USER_DATA", false);
        // rootDict.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_PERSONALIZATION_SIGNALS", false);
        if (Debug.isDebugBuild)
        {
            rootDict.SetBoolean("UIFileSharingEnabled", true);
        }

        // // https://developers.facebook.com/docs/ios/getting-started#step-3---configure-your-project
        // var appId = FacebookSettings.AppId;
        // var clientToken = FacebookSettings.ClientToken;
        // var displayName = FacebookSettings.AppLabels[0];
        // var urlTypes = rootDict.CreateArray("CFBundleURLTypes");
        // var dict = urlTypes.AddDict();
        // var urlSchemes = dict.CreateArray("CFBundleURLSchemes");
        // urlSchemes.AddString($"fb{appId}");
        // rootDict.SetString("FacebookAppID", appId);
        // rootDict.SetString("FacebookClientToken", clientToken);
        // rootDict.SetString("FacebookDisplayName", displayName);

        File.WriteAllText(filePath, plist.WriteToString());
    }
}
#endif