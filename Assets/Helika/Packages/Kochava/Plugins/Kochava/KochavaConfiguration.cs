//
//  KochavaTracker (Unity)
//
//  Copyright (c) 2013 - 2023 Kochava, Inc. All rights reserved.
//

// Build defines to assign each platform.
#if UNITY_EDITOR
#define KVA_EDITOR
#elif UNITY_ANDROID
#define KVA_ANDROID
#elif UNITY_IOS
#define KVA_IOS
#elif UNITY_WEBGL
#define KVA_WEBGL
#elif UNITY_STANDALONE_OSX
#define KVA_MACOS
#elif UNITY_STANDALONE_LINUX
#define KVA_LINUX
#elif UNITY_STANDALONE_WIN
#define KVA_WINDOWS
#elif WINDOWS_UWP
#define KVA_UWP
#else
#define KVA_OTHER
#endif

// Imports
using UnityEngine;
using System;

// Kochava SDK
namespace Kochava
{
    // Kochava Tracker SDK Configuration Object
    public class KochavaConfiguration : MonoBehaviour {

        // Editor-configurable settings
        #region Settings

        [Header("App GUID Configuration")]
        [Tooltip("App GUID when running in the Unity Editor regardless of the selected platform.")]
        public AppGuid editorAppGuid = new AppGuid();
        [Tooltip("App GUID for the Android Platform.")]
        public AppGuid androidAppGuid = new AppGuid();
        [Tooltip("App GUID for the iOS Platform.")]
        public AppGuid iosAppGuid = new AppGuid();
        [Tooltip("App GUID for the WebGL Platform.")]
        public AppGuid webGlAppGuid = new AppGuid();
        [Tooltip("App GUID for the macOS Platform.")]
        public AppGuid macOsAppGuid = new AppGuid();
        [Tooltip("App GUID for the Linux Platform.")]
        public AppGuid linuxAppGuid = new AppGuid();
        [Tooltip("App GUID for the Windows Platform.")]
        public AppGuid windowsAppGuid = new AppGuid();
        [Tooltip("App GUID for the Universal Windows Platform.")]
        public AppGuid uwpAppGuid = new AppGuid();
        [Tooltip("App GUID for use if no other App GUID was specified.")]
        public AppGuid fallbackAppGuid = new AppGuid();

        [Header("Logging")]
        [Tooltip("Log Level used in a Production build.")]
        public KochavaTrackerLogLevel productionLogLevel;
        [Tooltip("Log Level used in a Development build.")]
        public KochavaTrackerLogLevel developmentLogLevel;

        [Header("App Tracking Transparency (iOS)")]
        public IosAttConfig iosATTConfiguration = new IosAttConfig();

        [Header("App Clips (iOS)")]
        [Tooltip("App Group Identifier for iOS App Clips support.")]
        public string iosAppClipIdentifier = "";

        [Header("Instant Apps (Android)")]
        [Tooltip("App GUID for Android Instant App support.")]
        public AppGuid androidInstantAppGuid = new AppGuid();

        [Header("Partner Configuration (Advanced)")]
        [Tooltip("Only use if directed to by your Client Success Manager.")]
        public string kochavaPartnerName = "";

        #endregion

        // initialize kochava -- it will be ready to use during Start()
        private void Awake()
        {
            // Ensure this config game object is not destroyed.
            DontDestroyOnLoad(gameObject);
            
            // App GUIDs
            KochavaTracker.Instance.RegisterEditorAppGuid(editorAppGuid.Get());
            KochavaTracker.Instance.RegisterAndroidAppGuid(androidAppGuid.Get());
            KochavaTracker.Instance.RegisterIosAppGuid(iosAppGuid.Get());
            KochavaTracker.Instance.RegisterWebGlAppGuid(webGlAppGuid.Get());
            KochavaTracker.Instance.RegisterMacOsAppGuid(macOsAppGuid.Get());
            KochavaTracker.Instance.RegisterLinuxAppGuid(linuxAppGuid.Get());
            KochavaTracker.Instance.RegisterWindowsAppGuid(windowsAppGuid.Get());
            KochavaTracker.Instance.RegisterUwpAppGuid(uwpAppGuid.Get());
            KochavaTracker.Instance.RegisterFallbackAppGuid(fallbackAppGuid.Get());

            // Log Level
            KochavaTracker.Instance.SetLogLevel(Debug.isDebugBuild ? developmentLogLevel : productionLogLevel);

            // Instant Apps
#if KVA_ANDROID
            var resolvedAndroidInstantAppGuid = androidAppGuid.Get();
            if (!string.IsNullOrEmpty(resolvedAndroidInstantAppGuid))
            {
                KochavaTracker.Instance.EnableAndroidInstantApps(resolvedAndroidInstantAppGuid);
            }
#endif

            // App Clips
#if KVA_IOS
            if (!string.IsNullOrEmpty(iosAppClipIdentifier))
            {
                KochavaTracker.Instance.EnableIosAppClips(iosAppClipIdentifier);
            }
#endif

            // App Tracking Transparency options
#if KVA_IOS
            if (iosATTConfiguration.enabled)
            {
                KochavaTracker.Instance.EnableIosAtt(); 
            }
            KochavaTracker.Instance.SetIosAttAuthorizationAutoRequest(iosATTConfiguration.autoRequest);
            KochavaTracker.Instance.SetIosAttAuthorizationWaitTime(iosATTConfiguration.waitTime);
#endif
            // Partner Name
            KochavaTracker.Instance.RegisterPartnerName(kochavaPartnerName);

            // Start
            KochavaTracker.Instance.Start();
        }

        // App GUID container for specifying Production and Development App GUIDs.
        [Serializable]
        public class AppGuid
        {
            [Tooltip("App GUID used in a Production build.")]
            public string production = "";
            [Tooltip("Optional App GUID to use in a Development build.")]
            public string development = "";

            // Return the App GUID to use.
            public string Get()
            {
                if (Debug.isDebugBuild && !string.IsNullOrEmpty(development))
                {
                    return development;
                }
                return production;
            }
        }

        // iOS App Tracking Transparency Configuration.
        [Serializable]
        public class IosAttConfig
        {
            [Tooltip("Enable SDK ATT enforcement")]
            public bool enabled = false;
            [Tooltip("How many seconds the SDK should wait for a response.")]
            public double waitTime = 30;
            [Tooltip("If the SDK should automatically request authorization.")]
            public bool autoRequest = true;
        }
    }
}


