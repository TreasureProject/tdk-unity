//
//  KochavaAppleTracking (Unity)
//
//  Copyright (c) 2020 - 2023 Kochava, Inc. All rights reserved.
//

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

namespace Kochava.Editor
{
    // Kochava Apple Tracking Post Build Script.
    // Adds the Apple Tracking Module to the Xcode Project.
    public static class KochavaAppleTrackingPostBuild
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            switch (buildTarget)
            {
#if UNITY_IOS
                case BuildTarget.iOS:
                    // Add the iOS Frameworks contained by the XCFramework.
                    KochavaPostBuild.AddIosFramework("KochavaAppleTracking", path, "KochavaTracking");
                    break;
#endif
                default:
                    break;
            }
        }
    }
}
#endif