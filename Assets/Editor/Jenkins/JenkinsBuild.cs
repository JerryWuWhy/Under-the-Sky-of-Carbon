using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

public class JenkinsBuild
{
    private static readonly Dictionary<string, string> Dict = new();
    private static string _locationPathName = "Builds";
    private static  BuildTargetGroup _targetGroup = BuildTargetGroup.Unknown;
    private static  BuildTarget _target = BuildTarget.NoTarget;

    #region 通用构建逻辑 一般不要修改这些方法 如果要插入自己的逻辑，可以在后续的方法中插入

    private static void InitArgsDict()
    {
        if (Dict.Count > 0) return;
        var args = System.Environment.GetCommandLineArgs();
        var jenkinsArgs = args.FirstOrDefault(e => e.StartsWith("UnityParams:"));
        Assert.IsFalse(jenkinsArgs == null, $"args from jenkins not found! {string.Join(",", args)}");
        jenkinsArgs = jenkinsArgs.Replace("UnityParams:", "");
        Debug.Log($"jenkinsArgs = {jenkinsArgs}");
        var arr = jenkinsArgs.Split('*');
        foreach (var sub in arr)
        {
            var kv = sub.Split('=');
            Assert.IsTrue(kv.Length == 2, $"arg in error format {sub}");
            Dict.Add(kv[0], kv[1]);
            Debug.Log(kv[0] + "  +  " + kv[1]);
        }
    }

    private static T GetArg<T>(String key)
    {
        if (Dict.TryGetValue(key, out var value))
        {
            if (string.IsNullOrEmpty(value) || value == "null") return default;
            return (T)Convert.ChangeType(value, typeof(T));
        }

        return default;
    }

    private static BuildOptions CreateOptions()
    {
        var options = BuildOptions.None;
        if (EditorUserBuildSettings.symlinkSources)
        {
            options |= BuildOptions.SymlinkSources;
        }

        if (EditorUserBuildSettings.development)
        {
            options |= BuildOptions.Development;
        }

        if (EditorUserBuildSettings.connectProfiler)
        {
            options |= BuildOptions.ConnectWithProfiler;
        }

        if (EditorUserBuildSettings.buildWithDeepProfilingSupport)
        {
            options |= BuildOptions.EnableDeepProfilingSupport;
        }

        if (EditorUserBuildSettings.allowDebugging)
        {
            options |= BuildOptions.AllowDebugging;
        }

        if (EditorUserBuildSettings.waitForManagedDebugger)
        {
            options |= BuildOptions.WaitForPlayerConnection;
        }

        if (EditorUserBuildSettings.buildScriptsOnly)
        {
            options |= BuildOptions.BuildScriptsOnly;
        }

        return options;
    }

    private static void CheckScriptingDefine(ref string defines, bool b, string symbol)
    {
        if (b)
        {
            if (!defines.Contains(symbol))
                if (defines.Length < 1 || defines.EndsWith(";"))
                    defines += symbol;
                else
                    defines += $";{symbol}";
        }
        else
        {
            defines = defines.Replace($";{symbol}", string.Empty)
                .Replace($"{symbol};", string.Empty)
                .Replace(symbol, string.Empty);
        }
    }

    /// <summary>
    ///  Jenkins 的主入口 
    /// </summary>
    public static void Build()
    {
        Debug.Log("----------------------start build ------------------");
        InitArgsDict();
        Debug.Log("init args" + string.Join(",", Dict.Select(e => e.Key + ":" + e.Value + ",").ToArray()));
        var p = GetArg<string>("BuildPlatform");
        Debug.Log("Build app ,platform : "+p);
        CommonPreJenkinsBuild(p);
        switch (p )
        {
            case "Android":
                JenkinsBuildAndroid();
                break;
            case "iOS":
                JenkinsBuildiOS();
                break;
            case "WebGL":
                JenkinsBuildWebGL();
                break;
        }
        BeforeBuildPlayer();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = (from editorBuildSettingsScene in EditorBuildSettings.scenes select editorBuildSettingsScene.path)
                .ToArray(),
            locationPathName = _locationPathName,
            targetGroup = _targetGroup,
            target = _target,
            options = CreateOptions()
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.LogAssertion($"Build {p} succeeded: " + (summary.totalSize / 1024 / 1024) + " M, path -> " +
                               options.locationPathName);
        }
        else if (summary.result == BuildResult.Failed)
        {
            throw new Exception($"Build {p} failed ");
        }
    }
  
    private static void JenkinsBuildAndroid()
    {
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        EditorUserBuildSettings.buildAppBundle = GetArg<bool>("BuildAAB");
        EditorUserBuildSettings.androidCreateSymbols = (GetArg<bool>("SaveSymbols")|| GetArg<bool>("UploadAndroidSymbolsToFirebase"))
            ? (GetArg<bool>("SaveSymbols") ? AndroidCreateSymbols.Debugging : AndroidCreateSymbols.Public)
            : AndroidCreateSymbols.Disabled;
        EditorUserBuildSettings.allowDebugging = GetArg<bool>("ScriptDebugging");

        PlayerSettings.Android.useCustomKeystore = GetArg<bool>("UseCustomKeystore");
        if (PlayerSettings.Android.useCustomKeystore)
        {
            PlayerSettings.Android.keystorePass = GetArg<string>("KeystorePass");
            PlayerSettings.Android.keyaliasPass = GetArg<string>("KeyaliasPass");
            PlayerSettings.Android.keyaliasName = GetArg<string>("KeyaliasName");
            PlayerSettings.Android.keystoreName = GetArg<string>("KeystorePath");
        }

        PlayerSettings.Android.useAPKExpansionFiles = EditorUserBuildSettings.buildAppBundle;
        // Unity 版本大于 2022时，使用下面的代码
        // PlayerSettings.Android.splitApplicationBinary = EditorUserBuildSettings.buildAppBundle;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,
            GetArg<bool>("UseMono") ? ScriptingImplementation.Mono2x : ScriptingImplementation.IL2CPP);

        // ModifyAndroidManifest.Debug = EditorUserBuildSettings.connectProfiler;
        _locationPathName = GetArg<string>("BuildApkPath");
        _targetGroup = BuildTargetGroup.Android;
        _target = BuildTarget.Android;
    }

    private static void JenkinsBuildiOS()
    {
        EditorUserBuildSettings.symlinkSources = false;
        EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.appleEnableAutomaticSigning = GetArg<bool>("iOSAutoSign");
        PlayerSettings.iOS.appleDeveloperTeamID = GetArg<string>("AppleDevTeamId");
        if (!PlayerSettings.iOS.appleEnableAutomaticSigning)
        {
            PlayerSettings.iOS.iOSManualProvisioningProfileID = GetArg<string>("iOSProvisioningProfile");
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Distribution;
        }

        _locationPathName = GetArg<string>("XcodeProjectPath");
        _targetGroup = BuildTargetGroup.iOS;
        _target = BuildTarget.iOS;
    }

    private static void JenkinsBuildWebGL()
    {
        _locationPathName = GetArg<string>("WebGLBuildPath");
        _targetGroup = BuildTargetGroup.WebGL;
        _target = BuildTarget.WebGL;
    }
    
    #endregion
    
    /// <summary>
    ///  通用的构建前的设置，比如版本号，宏定义等
    ///  注意 此时各种针对平台的设置项还没有设置
    /// </summary>
    private static void CommonPreJenkinsBuild(string platform)
    {
        EditorUserBuildSettings.connectProfiler = GetArg<bool>("AutoConnectProfiler");
        EditorUserBuildSettings.development = GetArg<bool>("Development");
        Debug.Log("设置版本号");
        PlayerSettings.bundleVersion = GetArg<string>("Version");
        switch (platform)
        {
            case "Android":
                PlayerSettings.Android.bundleVersionCode = Mathf.Max(1, GetArg<int>("BuildNumber"));
                break;
            case "iOS":
                PlayerSettings.iOS.buildNumber = GetArg<string>("BuildNumber");
                break;
        }

        #region 自定义宏
        // 添加/删除 自定义宏 
        // ！！！！！！确保你的宏不会参与到和打包流程有关的Editor脚本！！！！！！
        // ！！！由于jenkins打包执行在batchmode下，修改宏后Unity进程不会停止运行，
        // ！！！并不会重新加载Editor程序集，此时仍然运行在旧的宏环境下
        // ！！！所以这里修改的宏不会对Editor脚本生效
        // ！！！！！！确保你的宏不会参与到和打包流程有关的Editor脚本！！！！！！
        //如有需要  将下面的代码取消注释
        
        // var p = android ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
        // var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(p);
        // Debug.Log("Before modify define symbols : " + defines);

        //--- 例子 当打包机传入的‘Debug’为true时，添加DEBUG_BUILD宏 否则 移除DEBUG_BUILD宏
        // CheckScriptingDefine(ref defines, GetArg<bool>("Debug"), "DEBUG_BUILD");

        // PlayerSettings.SetScriptingDefineSymbolsForGroup(p, defines);
        // Debug.Log("After modify define symbols : " + defines);
        #endregion
    }

    /// <summary>
    ///  在执行BuildPlayer前的处理 ,比如资源打包，
    ///  和CommonPreJenkinsBuild的区别是，这个方法调用时
    ///  各种针对所构建平台的设置已经完成 马上就要调用BuildPlayer
    /// </summary>
    private static void BeforeBuildPlayer()
    {
        Debug.Log("BeforeBuildPlayer");
        // HybridCLRTools.MakeHybridCLRDlls();
        // AssetDatabase.Refresh();
        // AssetExpressEditor.BuildAssetBundlesByAPI(GetArg<string>("Version"), GetArg<string>("BuildAbClassName"),
        //     GetArg<string>("BuildAbBundleName"));
       // ResourceArrange.BuildAssetBundle();
       // AssetDatabase.Refresh();
    }

    /*
     *  如果上述方法不能满足需求，可以查看上面的其他方法 插入自己的处理逻辑 
     */
  
}