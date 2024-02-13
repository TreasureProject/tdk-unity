//
//  KochavaTracker (Unity)
//
//  Copyright (c) 2020 - 2023 Kochava, Inc. All rights reserved.
//

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

namespace Kochava.Editor
{
    // Kochava Post Build Script.
    // Adds the iOS Native SDK to the Xcode Project.
    // Enables Module support in the Xcode project.
    public static class KochavaPostBuild
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            switch (buildTarget)
            {
#if UNITY_IOS
                case BuildTarget.iOS:
                    // Add the iOS Frameworks contained by the XCFramework.
                    AddIosFramework(path, "KochavaCore");
                    AddIosFramework(path, "KochavaTracker");
                    // Enable Module support.
                    EnableIosModules(path);
                    break;
#endif
            }
        }
        
#if UNITY_IOS
        
        // Enable Modules support in the iOS Xcode Project.
        // See https://forum.unity.com/threads/ios-cloud-build-error-use-of-import-when-modules-are-disabled.330706/#post-2347362
        private static void EnableIosModules(string path)
        {
            // Get the Xcode Project.
            var pbxProjectPath = UnityEditor.iOS.Xcode.PBXProject.GetPBXProjectPath(path);
            var project = new UnityEditor.iOS.Xcode.PBXProject();

            // Read the Project.
            project.ReadFromFile(pbxProjectPath);

            // Get the target guid.
            var targetGuidMain = GetTargetGuid(project, true);
            var targetGuidFramework = GetTargetGuid(project, false);

            // Enable Modules.
            project.AddBuildProperty(targetGuidMain, "CLANG_ENABLE_MODULES", "YES");
            project.AddBuildProperty(targetGuidFramework, "CLANG_ENABLE_MODULES", "YES");

            // Write the Project.
            project.WriteToFile(pbxProjectPath);
        }

        // Add an iOS Framework from an XCFramework to the Xcode Project.
        // See https://forum.unity.com/threads/how-do-i-add-xcframwork-from-code.840163/#post-6904331
        private static void AddIosFramework(string path, string frameworkName)
        {
            // Build paths.
            var xcFrameworkPath = Path.Combine("Assets", "Helika", "Packages", "Kochava", "Plugins", "iOS", frameworkName + ".xcframework");
            var deviceFrameworkPath = Path.Combine(xcFrameworkPath, "ios-arm64", frameworkName + ".framework");
            var simulatorFrameworkPath = Path.Combine(xcFrameworkPath, "ios-arm64_x86_64-simulator", frameworkName + ".framework");
            var destFrameworkPath = Path.Combine("Frameworks", "Kochava", frameworkName + ".framework");

            UnityEngine.Debug.Log("Adding Kochava iOS Framework to Xcode project: " + xcFrameworkPath);

            // Copy the appropriate Framework depending upon if building for Device or Simulator.
            switch (PlayerSettings.iOS.sdkVersion)
            {
                case iOSSdkVersion.DeviceSDK:
                    CopyAndReplaceDirectory(deviceFrameworkPath, Path.Combine(path, destFrameworkPath));
                    break;
                case iOSSdkVersion.SimulatorSDK:
                    CopyAndReplaceDirectory(simulatorFrameworkPath, Path.Combine(path, destFrameworkPath));
                    break;
                default:
                    UnityEngine.Debug.Log("Kochava Post Build: Invalid Deployment option, not copying native iOS SDK.");
                    return;
            }

            // Get the Xcode Project.
            var pbxProjectPath = UnityEditor.iOS.Xcode.PBXProject.GetPBXProjectPath(path);
            var project = new UnityEditor.iOS.Xcode.PBXProject();

            // Read the Project.
            project.ReadFromFile(pbxProjectPath);

            // Get the target guid.
            var targetGuidMain = GetTargetGuid(project, true);
            var targetGuidFramework = GetTargetGuid(project, false);

            // Add the Framework file to the Project.
            var fileGuid = project.AddFile(destFrameworkPath, destFrameworkPath, UnityEditor.iOS.Xcode.PBXSourceTree.Source);
            project.AddFileToBuild(targetGuidFramework, fileGuid);
            project.AddFrameworkToProject(targetGuidFramework, frameworkName + ".framework", false);

            // Add to the search paths.
            project.AddBuildProperty(targetGuidFramework, "FRAMEWORK_SEARCH_PATHS", "$(SRCROOT)/Frameworks/Kochava");
            project.AddBuildProperty(targetGuidFramework, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");

            // Set Other Linker Flags to -ObjC.
            project.AddBuildProperty(targetGuidFramework, "OTHER_LDFLAGS", "-ObjC");

            // Set as Embedded framework.
            UnityEditor.iOS.Xcode.Extensions.PBXProjectExtensions.AddFileToEmbedFrameworks(project, targetGuidMain, fileGuid);

            // Write the Project.
            project.WriteToFile(pbxProjectPath);
        }
        
        // Get the correct Xcode Target for the Unity version.
        private static string GetTargetGuid(UnityEditor.iOS.Xcode.PBXProject project, bool mainTarget)
        {
#if UNITY_2019_3_OR_NEWER
            if (mainTarget)
            {
                return project.GetUnityMainTargetGuid();
            }
            else
            {
                return project.GetUnityFrameworkTargetGuid();
            }
#else
            return project.TargetGuidByName(UnityEditor.iOS.Xcode.PBXProject.GetUnityTargetName());
#endif
        }

#endif

        // Recursively copy and replace all files in the destination directory with those in the source directory.
        private static void CopyAndReplaceDirectory(string srcPath, string dstPath)
        {
            RemoveAtPath(dstPath);
            Directory.CreateDirectory(dstPath);

            foreach (var file in Directory.GetFiles(srcPath))
            {
                File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
            }

            foreach (var dir in Directory.GetDirectories(srcPath))
            {
                CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
            }
        }

        // Copy and replace an individual file.
        private static void CopyAndReplaceFile(string srcFile, string dstFile)
        {
            RemoveAtPath(dstFile);
            File.Copy(srcFile, dstFile);
        }

        // Remove the file or directory at the given path.
        private static void RemoveAtPath(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }
            
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
#endif