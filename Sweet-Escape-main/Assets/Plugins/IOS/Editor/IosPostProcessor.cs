using System;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

namespace Editor
{
  public static class IosPostProcessor
  {
    [PostProcessBuild(1000)]
    [UsedImplicitly]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
#if UNITY_IOS
      if (target != BuildTarget.iOS)
        return;

      ModifyPlist(path);
      AddCapabilities(path);
#endif
    }

#if UNITY_IOS
    private static void ModifyPlist(string projectPath)
    {
      try
      {
        var plistPath = Path.Combine(projectPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        plist.WriteToFile(plistPath);
      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }
    }

    private static void AddCapabilities(string projectPath)
    {
      try
      {
        var path = PBXProject.GetPBXProjectPath(projectPath);
        var project = new PBXProject();
        project.ReadFromString(File.ReadAllText(path));
        var target = project.GetUnityMainTargetGuid();

        project.SetBuildProperty(project.GetUnityFrameworkTargetGuid(), "ENABLE_BITCODE", "NO");

        project.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
        project.SetBuildProperty(project.GetUnityFrameworkTargetGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES",
          "NO");

        project.WriteToFile(path);

        const string ENTITLEMENTS_PATH = "Unity-iPhone/Unity-iPhone.entitlements";

        var capManager = new ProjectCapabilityManager(path, ENTITLEMENTS_PATH, null, target);

        capManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        capManager.AddPushNotifications(true);

        capManager.AddSignInWithApple();

        capManager.WriteToFile();
      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }
    }
#endif
  }
}