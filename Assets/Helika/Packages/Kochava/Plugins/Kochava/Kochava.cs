//
//  KochavaMeasurement (Unity)
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

// Define cases where the Net Standard SDK is used.
#if !KVA_ANDROID && !KVA_IOS
#define KVA_NETSTD
#endif

// Imports
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using Kochava.Internal;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

// Kochava SDK
namespace Kochava
{
    #region PublicAPI

    // Log Levels
    public enum KochavaMeasurementLogLevel
    {
        None,
        Error,
        Warn,
        Info,
        Debug,
        Trace
    }

    // Standard event types
    // For samples and expected usage see: https://support.kochava.com/reference-information/post-install-event-examples/
    public enum KochavaMeasurementEventType
    {
        Achievement,
        AddToCart,
        AddToWishList,
        CheckoutStart,
        LevelComplete,
        Purchase,
        Rating,
        RegistrationComplete,
        Search,
        TutorialComplete,
        View,
        AdView,
        PushReceived,
        PushOpened,
        ConsentGranted,
        Deeplink,
        AdClick,
        StartTrial,
        Subscribe
    }

    // Install Attribution result.
    public class KochavaMeasurementInstallAttribution
    {
        // if attribution has been retrieved from the server.
        public readonly bool Retrieved;
        // the raw attribution response from the server as a dictionary.
        public readonly JObject Raw;
        // if this install or a de-duplicated install on this device is attributed.
        public readonly bool Attributed;
        // if this is the first install on this device.
        public readonly bool FirstInstall;

        internal KochavaMeasurementInstallAttribution()
        {
            Retrieved = false;
            Raw = new JObject();
            Attributed = false;
            FirstInstall = false;
        }

        internal KochavaMeasurementInstallAttribution(bool retrieved, JObject raw, bool attributed, bool firstInstall)
        {
            Retrieved = retrieved;
            Raw = raw ?? new JObject();
            Attributed = attributed;
            FirstInstall = firstInstall;
        }

        internal KochavaMeasurementInstallAttribution(JObject json)
        {
            Retrieved = Util.OptBool(json["retrieved"], false);
            Raw = Util.OptJObject(json["raw"], new JObject());
            Attributed = Util.OptBool(json["attributed"], false);
            FirstInstall = Util.OptBool(json["firstInstall"], false);
        }
    }

    // Deeplink result.
    public class KochavaMeasurementDeeplink
    {
        // Destination path or url.
        // Will be empty if no deeplink was passed in and there was no deferred deeplink.
        public readonly string Destination;
        // The raw response as a dictionary. Will always include "destination" but may include other metadata.
        public readonly JObject Raw;

        internal KochavaMeasurementDeeplink(string destination, JObject raw)
        {
            Destination = destination ?? "";
            Raw = raw ?? new JObject();
        }

        internal KochavaMeasurementDeeplink(JObject json)
        {
            Destination = Util.OptString(json["destination"], "");
            Raw = Util.OptJObject(json["raw"], new JObject());
        }
    }
    
    // Init result.
    public class KochavaMeasurementInit
    {
        // If consent gdpr currently applies.
        public readonly bool ConsentGdprApplies;

        internal KochavaMeasurementInit(bool consentGdprApplies)
        {
            ConsentGdprApplies = consentGdprApplies;
        }

        internal KochavaMeasurementInit(JObject json)
        {
            ConsentGdprApplies = Util.OptBool(json["consentGdprApplies"], false);
        }
    }

    // Represents a generic platform event.
    public class KochavaMeasurementPlatformEvent
    {
        // The identifier name of the platform event.
        public readonly string Name;

        // The raw value of the platform event.
        public readonly JObject Value;

        internal KochavaMeasurementPlatformEvent(string name, JObject value)
        {
            Name = name;
            Value = value;
        }

        internal KochavaMeasurementPlatformEvent(JObject json)
        {
            Name = Util.OptString(json["name"], "");
            Value = Util.OptJObject(json["value"], new JObject());
        }
    }

    // Standard Event.
    public class KochavaMeasurementEvent
    {
        private readonly string EventName;
        private readonly JObject EventData = new JObject();
        private string IosAppStoreReceiptBase64String;
        private string AndroidGooglePlayReceiptData;
        private string AndroidGooglePlayReceiptSignature;

        // Creates an Event with a Standard event type.
        public KochavaMeasurementEvent(KochavaMeasurementEventType eventType)
        {
            switch (eventType)
            {
                case KochavaMeasurementEventType.Achievement:
                    EventName = "Achievement";
                    break;
                case KochavaMeasurementEventType.AddToCart:
                    EventName = "Add to Cart";
                    break;
                case KochavaMeasurementEventType.AddToWishList:
                    EventName = "Add to Wish List";
                    break;
                case KochavaMeasurementEventType.CheckoutStart:
                    EventName = "Checkout Start";
                    break;
                case KochavaMeasurementEventType.LevelComplete:
                    EventName = "Level Complete";
                    break;
                case KochavaMeasurementEventType.Purchase:
                    EventName = "Purchase";
                    break;
                case KochavaMeasurementEventType.Rating:
                    EventName = "Rating";
                    break;
                case KochavaMeasurementEventType.RegistrationComplete:
                    EventName = "Registration Complete";
                    break;
                case KochavaMeasurementEventType.Search:
                    EventName = "Search";
                    break;
                case KochavaMeasurementEventType.TutorialComplete:
                    EventName = "Tutorial Complete";
                    break;
                case KochavaMeasurementEventType.View:
                    EventName = "View";
                    break;
                case KochavaMeasurementEventType.AdView:
                    EventName = "Ad View";
                    break;
                case KochavaMeasurementEventType.PushReceived:
                    EventName = "Push Received";
                    break;
                case KochavaMeasurementEventType.PushOpened:
                    EventName = "Push Opened";
                    break;
                case KochavaMeasurementEventType.ConsentGranted:
                    EventName = "Consent Granted";
                    break;
                case KochavaMeasurementEventType.Deeplink:
                    EventName = "_Deeplink";
                    break;
                case KochavaMeasurementEventType.AdClick:
                    EventName = "Ad Click";
                    break;
                case KochavaMeasurementEventType.StartTrial:
                    EventName = "Start Trial";
                    break;
                case KochavaMeasurementEventType.Subscribe:
                    EventName = "Subscribe";
                    break;
                default:
                    EventName = "";
                    break;
            }
        }

        // Creates an Event with a custom name.
        public KochavaMeasurementEvent(string eventName)
        {
            EventName = eventName ?? "";
        }

        // Sends the event.
        public void Send()
        {
            KochavaMeasurement.Instance.SendEventWithEvent(this);
        }

        // Sets a custom key/value to the event of type string.
        public void SetCustomStringValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;
            EventData.Add(key, value);
        }

        // Sets a custom key/value to the event of type bool.
        public void SetCustomBoolValue(string key, bool value)
        {
            if (string.IsNullOrEmpty(key)) return;
            EventData.Add(key, value);
        }

        // Sets a custom key/value to the event of type double.
        public void SetCustomNumberValue(string key, double value)
        {
            if (string.IsNullOrEmpty(key)) return;
            EventData.Add(key, value);
        }

        // Sets a custom key/value to the event of type object.
        private void SetCustomDictionaryValue(string key, JObject value)
        {
            if (string.IsNullOrEmpty(key) || value == null) return;
            EventData.Add(key, value);
        }

        // (Android Only) Sets the receipt from the Android Google Play Store.
        public void SetAndroidGooglePlayReceipt(string androidGooglePlayReceiptData, string androidGooglePlayReceiptSignature)
        {
            if (string.IsNullOrEmpty(androidGooglePlayReceiptData) || string.IsNullOrEmpty(androidGooglePlayReceiptSignature)) return;
            AndroidGooglePlayReceiptData = androidGooglePlayReceiptData;
            AndroidGooglePlayReceiptSignature = androidGooglePlayReceiptSignature;
        }

        // (iOS Only) Sets the receipt from the iOS Apple App Store.
        public void SetIosAppStoreReceipt(string iosAppStoreReceiptBase64String)
        {
            if (string.IsNullOrEmpty(iosAppStoreReceiptBase64String)) return;
            IosAppStoreReceiptBase64String = iosAppStoreReceiptBase64String;
        }

        // Standard Parameters.
        public void SetAction(string value) => SetCustomStringValue("action", value);
        public void SetBackground(bool value) => SetCustomBoolValue("background", value);
        public void SetCheckoutAsGuest(string value) => SetCustomStringValue("checkout_as_guest", value);
        public void SetCompleted(bool value) => SetCustomBoolValue("completed", value);
        public void SetContentId(string value) => SetCustomStringValue("content_id", value);
        public void SetContentType(string value) => SetCustomStringValue("content_type", value);
        public void SetCurrency(string value) => SetCustomStringValue("currency", value);
        public void SetDate(string value) => SetCustomStringValue("date", value);
        public void SetDescription(string value) => SetCustomStringValue("description", value);
        public void SetDestination(string value) => SetCustomStringValue("destination", value);
        public void SetDuration(double value) => SetCustomNumberValue("duration", value);
        public void SetEndDate(string value) => SetCustomStringValue("end_date", value);
        public void SetItemAddedFrom(string value) => SetCustomStringValue("item_added_from", value);
        public void SetLevel(string value) => SetCustomStringValue("level", value);
        public void SetMaxRatingValue(double value) => SetCustomNumberValue("max_rating_value", value);
        public void SetName(string value) => SetCustomStringValue("name", value);
        public void SetOrderId(string value) => SetCustomStringValue("order_id", value);
        public void SetOrigin(string value) => SetCustomStringValue("origin", value);
        public void SetPayload(JObject value) => SetCustomDictionaryValue("payload", value);
        public void SetPrice(double value) => SetCustomNumberValue("price", value);
        public void SetQuantity(double value) => SetCustomNumberValue("quantity", value);
        public void SetRatingValue(double value) => SetCustomNumberValue("rating_value", value);
        public void SetReceiptId(string value) => SetCustomStringValue("receipt_id", value);
        public void SetReferralFrom(string value) => SetCustomStringValue("referral_from", value);
        public void SetRegistrationMethod(string value) => SetCustomStringValue("registration_method", value);
        public void SetResults(string value) => SetCustomStringValue("results", value);
        public void SetScore(string value) => SetCustomStringValue("score", value);
        public void SetSearchTerm(string value) => SetCustomStringValue("search_term", value);
        public void SetSource(string value) => SetCustomStringValue("source", value);
        public void SetSpatialX(double value) => SetCustomNumberValue("spatial_x", value);
        public void SetSpatialY(double value) => SetCustomNumberValue("spatial_y", value);
        public void SetSpatialZ(double value) => SetCustomNumberValue("spatial_z", value);
        public void SetStartDate(string value) => SetCustomStringValue("start_date", value);
        public void SetSuccess(string value) => SetCustomStringValue("success", value);
        public void SetUri(string value) => SetCustomStringValue("uri", value);
        public void SetUserId(string value) => SetCustomStringValue("user_id", value);
        public void SetUserName(string value) => SetCustomStringValue("user_name", value);
        public void SetValidated(string value) => SetCustomStringValue("validated", value);

        // Ad LTV Parameters
        public void SetAdCampaignId(string value) => SetCustomStringValue("ad_campaign_id", value);
        public void SetAdCampaignName(string value) => SetCustomStringValue("ad_campaign_name", value);
        public void SetAdDeviceType(string value) => SetCustomStringValue("device_type", value);
        public void SetAdGroupId(string value) => SetCustomStringValue("ad_group_id", value);
        public void SetAdGroupName(string value) => SetCustomStringValue("ad_group_name", value);
        public void SetAdMediationName(string value) => SetCustomStringValue("ad_mediation_name", value);
        public void SetAdNetworkName(string value) => SetCustomStringValue("ad_network_name", value);
        public void SetAdPlacement(string value) => SetCustomStringValue("placement", value);
        public void SetAdSize(string value) => SetCustomStringValue("ad_size", value);
        public void SetAdType(string value) => SetCustomStringValue("ad_type", value);

        // Returns the event name.
        internal string GetEventName()
        {
            return EventName;
        }

        // Returns the event data
        internal JObject GetEventData()
        {
            return EventData;
        }

        // Returns the iOS receipt.
        internal string GetIosAppStoreReceiptBase64String()
        {
            return IosAppStoreReceiptBase64String;
        }

        // Returns the Andrpod Google Receipt Data.
        internal string GetAndroidGooglePlayReceiptData()
        {
            return AndroidGooglePlayReceiptData;
        }

        // Returns the Android Google Receipt Signature.
        internal string GetAndroidGooglePlayReceiptSignature()
        {
            return AndroidGooglePlayReceiptSignature;
        }
    }

    // Main SDK Entrypoint.
    public class KochavaMeasurement
    {
        // Version data that is updated via a script. Do not change.
        private const string SdkName = "Unity";
        private const string SdkVersion = "6.2.0";
        private const string SdkBuildDate = "2024-07-16T15:13:20Z";

        // Native SDK API Handler
        private readonly NativeApi NativeApi;

        // Registered app guids
        private string RegisteredEditorAppGuid = null;
        private string RegisteredAndroidAppGuid = null;
        private string RegisteredIosAppGuid = null;
        private string RegisteredWebGlAppGuid = null;
        private string RegisteredMacOsAppGuid = null;
        private string RegisteredLinuxAppGuid = null;
        private string RegisteredWindowsAppGuid = null;
        private string RegisteredUwpAppGuid = null;
        private string RegisteredFallbackAppGuid = null;
        private string RegisteredPartnerName = null;

        // Initialize with the Native API Handler for the current platform.
        internal KochavaMeasurement(NativeApi nativeApi)
        {
            NativeApi = nativeApi;
        }

        // Singleton instance.
        public static KochavaMeasurement Instance => SingletonHandler.Instance.Measurement;

        // Reserved function, only use if directed to by your Client Success Manager.
        public void ExecuteAdvancedInstruction(string name, string value)
        {
            NativeApi.ExecuteAdvancedInstruction(name, value);
        }

        // Sets the log level. This should be set prior to starting the SDK.
        public void SetLogLevel(KochavaMeasurementLogLevel logLevel)
        {
            NativeApi.SetLogLevel(logLevel);
        }

        // Sets the sleep state.
        public void SetSleep(bool sleep)
        {
            NativeApi.SetSleep(sleep);
        }

        // Sets if app level advertising tracking should be limited.
        public void SetAppLimitAdTracking(bool appLimitAdTracking)
        {
            NativeApi.SetAppLimitAdTracking(appLimitAdTracking);
        }

        // Register a custom device identifier for install attribution.
        public void RegisterCustomDeviceIdentifier(string name, string value)
        {
            NativeApi.RegisterCustomDeviceIdentifier(name, value);
        }
        
        // Register a custom value to be included in SDK payloads.
        public void RegisterCustomStringValue(string name, string value)
        {
            NativeApi.RegisterCustomStringValue(name, value);
        }

        // Register a custom value to be included in SDK payloads.
        public void RegisterCustomBoolValue(string name, bool? value)
        {
            NativeApi.RegisterCustomBoolValue(name, value);
        }

        // Register a custom value to be included in SDK payloads.
        public void RegisterCustomNumberValue(string name, double? value)
        {
            NativeApi.RegisterCustomNumberValue(name, value);
        }

        // Registers an Identity Link that allows linking different identities together in the form of key and value pairs.
        public void RegisterIdentityLink(string name, string value)
        {
            NativeApi.RegisterIdentityLink(name, value);
        }

        // (Android Only) Enable the Instant App feature by setting the instant app guid.
        public void EnableAndroidInstantApps(string instantAppGuid)
        {
            NativeApi.EnableAndroidInstantApps(instantAppGuid);
        }

        // (iOS Only) Enables App Clips by setting the Container App Group Identifier for App Clips data migration.
        public void EnableIosAppClips(string containerAppGroupIdentifier)
        {
            NativeApi.EnableIosAppClips(containerAppGroupIdentifier);
        }

        // (iOS Only) Enables App Tracking Transparency.
        public void EnableIosAtt()
        {
            NativeApi.EnableIosAtt();
        }

        // (iOS Only) Sets the amount of time in seconds to wait for App Tracking Transparency Authorization. Default 30 seconds.
        public void SetIosAttAuthorizationWaitTime(double waitTime)
        {
            NativeApi.SetIosAttAuthorizationWaitTime(waitTime);
        }

        // (iOS Only) Sets if the SDK should automatically request App Tracking Transparency Authorization on start. Default true.
        public void SetIosAttAuthorizationAutoRequest(bool autoRequest)
        {
            NativeApi.SetIosAttAuthorizationAutoRequest(autoRequest);
        }

        // Register a privacy profile, creating or overwriting an existing pofile.
        public void RegisterPrivacyProfile(string name, string[] keys)
        {
            NativeApi.RegisterPrivacyProfile(name, keys);
        }

        // Enable or disable an existing privacy profile.
        public void SetPrivacyProfileEnabled(string name, bool enabled)
        {
            NativeApi.SetPrivacyProfileEnabled(name, enabled);
        }

        // Register a deeplink wrapper domain for enhanced deeplink ESP integrations.
        public void RegisterDeeplinkWrapperDomain(string domain)
        {
            NativeApi.RegisterDeeplinkWrapperDomain(domain);
        }
        
        // Set the init completed callback listener.
        public void SetInitCompletedListener(Action<KochavaMeasurementInit> callback)
        {
            NativeApi.SetInitCompletedListener(callback);
        }
            
        // Set if consent has been explicitly opted in or out by the user.
        public void SetIntelligentConsentGranted(bool granted)
        {
            NativeApi.SetIntelligentConsentGranted(granted);
        }

        // Returns if the SDK is currently started.
        public bool GetStarted()
        {
            return NativeApi.GetStarted();
        }

        // Register the Editor App GUID. Do this prior to calling Start.
        public void RegisterEditorAppGuid(string editorAppGuid)
        {
            RegisteredEditorAppGuid = editorAppGuid;
        }

        // Register the Android App GUID. Do this prior to calling Start.
        public void RegisterAndroidAppGuid(string androidAppGuid)
        {
            RegisteredAndroidAppGuid = androidAppGuid;
        }

        // Register the iOS App GUID. Do this prior to calling Start.
        public void RegisterIosAppGuid(string iosAppGuid)
        {
            RegisteredIosAppGuid = iosAppGuid;
        }

        // Register the WebGL App GUID. Do this prior to calling Start.
        public void RegisterWebGlAppGuid(string webGlAppGuid)
        {
            RegisteredWebGlAppGuid = webGlAppGuid;
        }

        // Register the MacOS App GUID. Do this prior to calling Start.
        public void RegisterMacOsAppGuid(string macOsAppGuid)
        {
            RegisteredMacOsAppGuid = macOsAppGuid;
        }

        // Register the Linux App GUID. Do this prior to calling Start.
        public void RegisterLinuxAppGuid(string linuxAppGuid)
        {
            RegisteredLinuxAppGuid = linuxAppGuid;
        }

        // Register the Windows App GUID. Do this prior to calling Start.
        public void RegisterWindowsAppGuid(string windowsAppGuid)
        {
            RegisteredWindowsAppGuid = windowsAppGuid;
        }

        // Register the Universal Windows Platform (UWP) App GUID. Do this prior to calling Start.
        public void RegisterUwpAppGuid(string uwpAppGuid)
        {
            RegisteredUwpAppGuid = uwpAppGuid;
        }

        // Register the Fallback App GUID. Do this prior to calling Start.
        // This App GUID will be used if an App GUID was not specifically set for the current platform.
        public void RegisterFallbackAppGuid(string fallbackAppGuid)
        {
            RegisteredFallbackAppGuid = fallbackAppGuid;
        }

        // Register your Partner Name. Do this prior to calling Start.
        // NOTE: Only use this method if directed to by your Client Success Manager.
        public void RegisterPartnerName(string partnerName)
        {
            RegisteredPartnerName = partnerName;
        }

        // Start the SDK with the previously registered App GUID or Partner Name.
        public void Start()
        {
            // Inject version extension.
            const string wrapper = "{\"name\":\"" + SdkName + "\",\"version\":\"" + SdkVersion + "\",\"build_date\":\"" + SdkBuildDate + "\"}";
            ExecuteAdvancedInstruction("wrapper", wrapper);

            // Start the SDK
#if KVA_EDITOR
            if (!string.IsNullOrEmpty(RegisteredEditorAppGuid))
            {
                NativeApi.StartWithAppGuid(RegisteredEditorAppGuid);
                return;
            }
#elif KVA_ANDROID
            if (!string.IsNullOrEmpty(RegisteredAndroidAppGuid))
            {
                NativeApi.StartWithAppGuid(RegisteredAndroidAppGuid);
                return;
            }
#elif KVA_IOS
            if (!string.IsNullOrEmpty(RegisteredIosAppGuid))
            {
                NativeApi.StartWithAppGuid(RegisteredIosAppGuid);
                return;
            }
#elif KVA_WEBGL
            if (!string.IsNullOrEmpty(RegisteredWebGlAppGuid))
            {
                NativeApi.StartWithAppGuid(RegisteredWebGlAppGuid);
                return;
            }
#elif KVA_MACOS
            if (!string.IsNullOrEmpty(RegisteredMacOsAppGuid))
            {
                NativeApi.StartWithAppGuid(RegisteredMacOsAppGuid);
                return;
            }
#elif KVA_LINUX
            if (!string.IsNullOrEmpty(RegisteredLinuxAppGuid))
            {
                NativeApi.StartWithAppGuid(RegisteredLinuxAppGuid);
                return;
            }
#elif KVA_WINDOWS
            if (!string.IsNullOrEmpty(RegisteredWindowsAppGuid))
            {
                NativeApi.StartWithAppGuid(RegisteredWindowsAppGuid);
                return;
            }
#elif KVA_UWP
            if (!string.IsNullOrEmpty(RegisteredUwpAppGuid))
            {
                NativeApi.StartWithAppGuid(RegisteredUwpAppGuid);
                return;
            }
#endif
            // Fallback to the fallback App GUID if one exists.
            if (!string.IsNullOrEmpty(RegisteredFallbackAppGuid))
            {
                NativeApi.StartWithAppGuid(RegisteredFallbackAppGuid);
                return;
            }

            // Fallback to a partner name if it exists.
            if (!string.IsNullOrEmpty(RegisteredPartnerName))
            {
                NativeApi.StartWithPartnerName(RegisteredPartnerName);
                return;
            }

            // Allow the native to log the error of no app guid.
            NativeApi.StartWithAppGuid("");
        }

        // Shuts down the SDK and optionally deletes all local SDK data.
        // NOTE: Care should be taken when using this method as deleting the SDK data will make it reset back to a first install state.
        public void Shutdown(bool deleteData)
        {
            // Clear registered app guids and partner name.
            RegisteredEditorAppGuid = null;
            RegisteredAndroidAppGuid = null;
            RegisteredIosAppGuid = null;
            RegisteredWebGlAppGuid = null;
            RegisteredMacOsAppGuid = null;
            RegisteredLinuxAppGuid = null;
            RegisteredWindowsAppGuid = null;
            RegisteredUwpAppGuid = null;
            RegisteredFallbackAppGuid = null;
            RegisteredPartnerName = null;

            // Clear callbacks
            SingletonHandler.Instance.Shutdown();

            // Shutdown the native
            NativeApi.Shutdown(deleteData);
        }

        // Returns the Kochava Install ID via a callback.
        public void RetrieveInstallId(Action<string> callback)
        {
            if (callback == null)
            {
                Util.Log("Warn: Invalid Callback");
                return;
            }

            NativeApi.RetrieveInstallId(callback);
        }
        
        // Retrieves the latest install attribution data from the server.
        public void RetrieveInstallAttribution(Action<KochavaMeasurementInstallAttribution> callback)
        {
            if (callback == null)
            {
                Util.Log("Warn: Invalid Callback");
                return;
            }

            NativeApi.RetrieveInstallAttribution(callback);
        }

        // Process a launch deeplink using the default 10 second timeout.
        public void ProcessDeeplink(string path, Action<KochavaMeasurementDeeplink> callback)
        {
            if (callback == null)
            {
                Util.Log("Warn: Invalid Callback");
                return;
            }

            NativeApi.ProcessDeeplink(path, callback);
        }

        // Process a launch deeplink using a custom timeout in seconds.
        public void ProcessDeeplinkWithOverrideTimeout(string path, double timeout, Action<KochavaMeasurementDeeplink> callback)
        {
            if (callback == null)
            {
                Util.Log("Warn: Invalid Callback");
                return;
            }

            NativeApi.ProcessDeeplinkWithOverrideTimeout(path, timeout, callback);
        }
        
        // (Android and iOS Only) Register the push token.
        public void RegisterPushToken(byte[] tokenBytes)
        {
            RegisterPushToken(BitConverter.ToString(tokenBytes ?? Array.Empty<byte>()).Replace("-", ""));
        }

        // (Android and iOS Only) Register the push token.
        public void RegisterPushToken(string token)
        {
            NativeApi.RegisterPushToken(token);
        }

        // (Android and iOS Only) Set the push enabled state.
        public void SetPushEnabled(bool enabled)
        {
            NativeApi.SetPushEnabled(enabled);
        }
        
        // Registers a default parameter on every event.
        public void RegisterDefaultEventStringParameter(string name, string value)
        {
            NativeApi.RegisterDefaultEventStringParameter(name, value);
        }

        // Registers a default parameter on every event.
        public void RegisterDefaultEventBoolParameter(string name, bool? value)
        {
            NativeApi.RegisterDefaultEventBoolParameter(name, value);
        }

        // Registers a default parameter on every event.
        public void RegisterDefaultEventNumberParameter(string name, double? value)
        {
            NativeApi.RegisterDefaultEventNumberParameter(name, value);
        }

        // Registers a default user_id value on every event.
        public void RegisterDefaultEventUserId(string value)
        {
            NativeApi.RegisterDefaultEventUserId(value);
        }

        // Send an event.
        public void SendEvent(string name)
        {
            NativeApi.SendEvent(name);
        }

        // Send an event with string data.
        public void SendEventWithString(string name, string data)
        {
            NativeApi.SendEventWithString(name, data);
        }

        // Send an event with dictionary data.
        public void SendEventWithDictionary(string name, JObject data)
        {
            NativeApi.SendEventWithDictionary(name, data);
        }

        // Send an event object (Called via Event.send().
        public void SendEventWithEvent(KochavaMeasurementEvent standardEvent)
        {
            if (standardEvent == null)
            {
                Util.Log("Warn: Invalid Event");
                return;
            }

            NativeApi.SendEventWithEvent(standardEvent);
        }

        // Build and return an event using a Standard Event Type.
        public KochavaMeasurementEvent BuildEventWithEventType(KochavaMeasurementEventType eventType)
        {
            return new KochavaMeasurementEvent(eventType);
        }

        // Build and return an event using a custom name.
        public KochavaMeasurementEvent BuildEventWithEventName(string eventName)
        {
            return new KochavaMeasurementEvent(eventName);
        }

        // Set the platform event handler
        public void SetPlatformEventHandler(Action<KochavaMeasurementPlatformEvent> callback)
        {
            SingletonHandler.Instance.SetPlatformEventHandler(callback);
        }
    }

    #endregion

    // Internal Kochava SDK
    namespace Internal
    {
        // Internal Singleton Handler for all MonoBehaviour actions.
        internal class SingletonHandler : MonoBehaviour
        {
            // Singleton instance
            private static readonly object SingletonLock = new object();
            private static volatile SingletonHandler SingletonInstance;

            // Measurement instance.
            private NativeApi NativeApi;
            internal KochavaMeasurement Measurement;

            // Callbacks
            private static readonly object CallbackLock = new object();
            private readonly Dictionary<string, Action<string>> StringRequests = new Dictionary<string, Action<string>>();
            private readonly Dictionary<string, string> StringResponses = new Dictionary<string, string>();
            private readonly Dictionary<string, Action<KochavaMeasurementInstallAttribution>> InstallAttributionRequests = new Dictionary<string, Action<KochavaMeasurementInstallAttribution>>();
            private readonly Dictionary<string, KochavaMeasurementInstallAttribution> InstallAttributionResponses = new Dictionary<string, KochavaMeasurementInstallAttribution>();
            private readonly Dictionary<string, Action<KochavaMeasurementDeeplink>> DeeplinkRequests = new Dictionary<string, Action<KochavaMeasurementDeeplink>>();
            private readonly Dictionary<string, KochavaMeasurementDeeplink> DeeplinkResponses = new Dictionary<string, KochavaMeasurementDeeplink>();
            private readonly List<KochavaMeasurementPlatformEvent> PlatformEvents = new List<KochavaMeasurementPlatformEvent>();
            private Action<KochavaMeasurementPlatformEvent> PlatformEventsHandler;
            private readonly List<KochavaMeasurementInit> InitCompletedEvents = new List<KochavaMeasurementInit>();
            private Action<KochavaMeasurementInit> InitCompletedHandler;
            
#if KVA_NETSTD
            private readonly List<KochavaNetStd.NativeRequest> NetStdNativeRequests = new List<KochavaNetStd.NativeRequest>();
#endif

            // Singleton instance. This creates a Unity GameObject called KochavaMeasurement that is set to survive level loading.
            public static SingletonHandler Instance
            {
                get
                {
                    if (SingletonInstance != null) return SingletonInstance;
                    lock (SingletonLock)
                    {
                        if (SingletonInstance != null) return SingletonInstance;
                        
                        // Create the Kochava Game Object.
                        var kochavaMeasurementGameObject = new GameObject("KochavaMeasurement");
                        DontDestroyOnLoad(kochavaMeasurementGameObject);

                        // Create Singleton Instance on that game object.
                        SingletonInstance = kochavaMeasurementGameObject.AddComponent<SingletonHandler>();

                        // Create Native API Handler for the current platform.
#if KVA_ANDROID
                        SingletonInstance.NativeApi = kochavaMeasurementGameObject.AddComponent<NativeAndroid>();
#elif KVA_IOS
                        SingletonInstance.NativeApi = kochavaMeasurementGameObject.AddComponent<NativeIos>();
#else
                        SingletonInstance.NativeApi = kochavaMeasurementGameObject.AddComponent<NativeDotNet>();
#endif
                        SingletonInstance.Measurement = new KochavaMeasurement(SingletonInstance.NativeApi);
                    }

                    return SingletonInstance;
                }
            }

            // Process actions from native SDKs that must occur on the Unity main thread.
            private void Update()
            {
                // Call the nativeApi update handler.
                NativeApi.Update();
                
                // Process each callback handler.
                ProcessStringCallbacks();
                ProcessInstallAttributionCallbacks();
                ProcessDeeplinkCallbacks();
                ProcessPlatformEventCallbacks();
                ProcessInitCompletedCallbacks();
#if KVA_NETSTD
                ProcessNetStdNativeRequests();
#endif
            }

            // Shutdown and clear callbacks.
            internal void Shutdown()
            {
                lock (CallbackLock)
                {
                    StringRequests.Clear();
                    StringResponses.Clear();
                    InstallAttributionRequests.Clear();
                    InstallAttributionResponses.Clear();
                    DeeplinkRequests.Clear();
                    DeeplinkResponses.Clear();
                    PlatformEvents.Clear();
                    PlatformEventsHandler = null;
                    InitCompletedEvents.Clear();
                    InitCompletedHandler = null;
#if KVA_NETSTD
                    NetStdNativeRequests.Clear();
#endif
                }
            }

            // Add a string request with callback. Use the returned requestId to match the response.
            internal string AddStringRequest(Action<string> callback)
            {
                var requestId = Guid.NewGuid().ToString();
                lock (CallbackLock)
                {
                    StringRequests.Add(requestId, callback);
                }

                return requestId;
            }

            // Add a string response to callback. Use the generated requestId from the request to match up.
            internal void AddStringResponse(string requestId, string value)
            {
                lock (CallbackLock)
                {
                    StringResponses.Add(requestId, value);
                }
            }

            // Process String Responses. Must be called from the Unity Update thread.
            private void ProcessStringCallbacks()
            {
                lock (CallbackLock)
                {
                    foreach (var entry in StringResponses)
                    {
                        StringRequests.TryGetValue(entry.Key, out var callback);
                        if (callback == null) continue;
                        callback(entry.Value);
                        StringRequests.Remove(entry.Key);
                    }

                    StringResponses.Clear();
                }
            }
            
            // Callback method for a processed string when using UnitySendMessage (such as from iOS native layer). Name and location cannot change.
            private void NativeStringListener(string msg)
            {
                // Parse the response
                var response = JObject.Parse(msg);
                var id = (string) response["id"] ?? "";
                var value = (string) response["value"] ?? "";

                // Queue the response
                AddStringResponse(id, value);
            }
            
            // Add an install attribution request with callback. Use the returned requestId to match the response.
            internal string AddInstallAttributionRequest(Action<KochavaMeasurementInstallAttribution> callback)
            {
                var requestId = Guid.NewGuid().ToString();
                lock (CallbackLock)
                {
                    InstallAttributionRequests.Add(requestId, callback);
                }

                return requestId;
            }

            // Add an install attribution response to callback. Use the generated requestId from the request to match up.
            internal void AddInstallAttributionResponse(string requestId, KochavaMeasurementInstallAttribution value)
            {
                lock (CallbackLock)
                {
                    InstallAttributionResponses.Add(requestId, value);
                }
            }

            // Process Install Attribution Responses. Must be called from the Unity Update thread.
            private void ProcessInstallAttributionCallbacks()
            {
                lock (CallbackLock)
                {
                    foreach (var entry in InstallAttributionResponses)
                    {
                        InstallAttributionRequests.TryGetValue(entry.Key, out var callback);
                        if (callback == null) continue;
                        callback(entry.Value);
                        InstallAttributionRequests.Remove(entry.Key);
                    }

                    InstallAttributionResponses.Clear();
                }
            }
            
            // Callback method for retrieved install attribution when using UnitySendMessage (such as from iOS native layer). Name and location cannot change.
            private void NativeInstallAttributionListener(string msg)
            {
                // Parse the response
                var response = JObject.Parse(msg);
                var id = (string) response["id"] ?? "";
                var value = JObject.Parse((string) response["value"] ?? "{}");
                var attribution = new KochavaMeasurementInstallAttribution(value);

                // Queue the response
                AddInstallAttributionResponse(id, attribution);
            }
            
            // Add a deeplink request with callback. Use the returned requestId to match the response.
            internal string AddDeeplinkRequest(Action<KochavaMeasurementDeeplink> callback)
            {
                var requestId = Guid.NewGuid().ToString();
                lock (CallbackLock)
                {
                    DeeplinkRequests.Add(requestId, callback);
                }

                return requestId;
            }

            // Add a deeplink response to callback. Use the generated requestId from the request to match up.
            internal void AddDeeplinkResponse(string requestId, KochavaMeasurementDeeplink value)
            {
                lock (CallbackLock)
                {
                    DeeplinkResponses.Add(requestId, value);
                }
            }

            // Process Deeplink Responses. Must be called from the Unity Update thread.
            private void ProcessDeeplinkCallbacks()
            {
                lock (CallbackLock)
                {
                    foreach (var entry in DeeplinkResponses)
                    {
                        DeeplinkRequests.TryGetValue(entry.Key, out var callback);
                        if (callback == null) continue;
                        callback(entry.Value);
                        DeeplinkRequests.Remove(entry.Key);
                    }

                    DeeplinkResponses.Clear();
                }
            }
            
            // Callback method for a processed deeplink when using UnitySendMessage (such as from iOS native layer). Name and location cannot change.
            private void NativeDeeplinkListener(string msg)
            {
                // Parse the response
                var response = JObject.Parse(msg);
                var id = (string) response["id"] ?? "";
                var value = JObject.Parse((string) response["value"] ?? "{}");
                var deeplink = new KochavaMeasurementDeeplink(value);

                // Queue the response
                AddDeeplinkResponse(id, deeplink);
            }
            
            // Add a platform event response to callback.
            internal void AddPlatformEventResponse(KochavaMeasurementPlatformEvent platformEvent)
            {
                lock (CallbackLock)
                {
                    PlatformEvents.Add(platformEvent);
                }
            }

            // Set the platform event handler
            internal void SetPlatformEventHandler(Action<KochavaMeasurementPlatformEvent> callback)
            {
                lock (CallbackLock)
                {
                    PlatformEventsHandler = callback;
                }
            }

            // Process Platform Events. Must be called from the Unity Update thread.
            private void ProcessPlatformEventCallbacks()
            {
                lock (CallbackLock)
                {
                    foreach (var entry in PlatformEvents)
                    {
                        PlatformEventsHandler?.Invoke(entry);
                    }

                    PlatformEvents.Clear();
                }
            }
            
            // Callback method for a generic platform event when using UnitySendMessage (such as from iOS native layer). Name and location cannot change.
            private void NativePlatformEventListener(string msg)
            {
                // Parse the response
                var response = JObject.Parse(msg);
                var platformEvent = new KochavaMeasurementPlatformEvent(response);

                // Queue the response
                AddPlatformEventResponse(platformEvent);
            }
            
            // Add a init completed response to callback.
            internal void AddInitCompletedResponse(KochavaMeasurementInit init)
            {
                lock (CallbackLock)
                {
                    InitCompletedEvents.Add(init);
                }
            }

            // Set the init completed handler
            internal void SetInitCompletedHandler(Action<KochavaMeasurementInit> callback)
            {
                lock (CallbackLock)
                {
                    InitCompletedHandler = callback;
                }
            }

            // Process Init Completed Events. Must be called from the Unity Update thread.
            private void ProcessInitCompletedCallbacks()
            {
                lock (CallbackLock)
                {
                    foreach (var entry in InitCompletedEvents)
                    {
                        InitCompletedHandler?.Invoke(entry);
                    }

                    InitCompletedEvents.Clear();
                }
            }
            
            // Callback method for a init completed event when using UnitySendMessage (such as from iOS native layer). Name and location cannot change.
            private void NativeInitCompletedListener(string msg)
            {
                // Parse the response
                var response = JObject.Parse(msg);
                var init = new KochavaMeasurementInit(response);

                // Queue the response
                AddInitCompletedResponse(init);
            }

#if KVA_NETSTD
            // Add a Net Standard Native Request to the queue.
            internal void AddNetStdNativeRequest(KochavaNetStd.NativeRequest nativeRequest)
            {
                lock (CallbackLock)
                {
                    NetStdNativeRequests.Add(nativeRequest);
                }
            }

            // Process Net Std Native Requests. Must be called from the Unity Update thread.
            private void ProcessNetStdNativeRequests()
            {
                lock (CallbackLock)
                {
                    foreach (var nativeRequest in NetStdNativeRequests)
                    {
                        switch (nativeRequest.Action)
                        {
                            // Gather the specified datapoint.
                            case KochavaNetStd.NativeRequest.NativeRequestType.GatherDatapoint:
                                ProcessNetStdNativeRequestGatherDatapoint(nativeRequest);
                                break;
                            // Write or Delete a key/value string to disk.
                            case KochavaNetStd.NativeRequest.NativeRequestType.WriteStringToDisk:
                                ProcessNetStdNativeRequestWriteStringToDisk(nativeRequest);
                                break;
                            // Read a key/value string from disk.
                            case KochavaNetStd.NativeRequest.NativeRequestType.GetStringFromDisk:
                                ProcessNetStdNativeRequestGetStringFromDisk(nativeRequest);
                                break;
                            // Delete all persisted data.
                            case KochavaNetStd.NativeRequest.NativeRequestType.ErasePersistedData:
                                ProcessNetStdNativeRequestErasePersistedData(nativeRequest);
                                break;
                            // Migrate from legacy Unity Net (if applicable).
                            case KochavaNetStd.NativeRequest.NativeRequestType.AttemptRestoreKochavaDeviceIdBackup:
                                ProcessNetStdNativeRequestAttemptRestoreKochavaDeviceIdBackup(nativeRequest);
                                break;
                            // Perform a network request.
                            case KochavaNetStd.NativeRequest.NativeRequestType.NetworkRequest:
                                StartCoroutine(ProcessNetStdNativeRequestNetworkRequest(nativeRequest));
                                break;
                            // Unknown command.
                            default:
                                nativeRequest.Fulfill();
                                break;
                        }
                    }

                    NetStdNativeRequests.Clear();
                }
            }
            
            // Gather Net Std data points.
            private void ProcessNetStdNativeRequestGatherDatapoint(KochavaNetStd.NativeRequest nativeRequest)
            {
                // Gather common data points.
                if (nativeRequest.Key == "package") nativeRequest.Fulfill(GetPackage());
                if (nativeRequest.Key == "platform") nativeRequest.Fulfill(GetPlatform());
                if (nativeRequest.Key == "device") nativeRequest.Fulfill(SystemInfo.deviceModel);
                if (nativeRequest.Key == "disp_w") nativeRequest.Fulfill(Screen.width);
                if (nativeRequest.Key == "disp_h") nativeRequest.Fulfill(Screen.height);
                if (nativeRequest.Key == "screen_dpi") nativeRequest.Fulfill(Screen.dpi);
                if (nativeRequest.Key == "os_version") nativeRequest.Fulfill(SystemInfo.operatingSystem);
                if (nativeRequest.Key == "battery_level") nativeRequest.Fulfill(SystemInfo.batteryLevel);
                if (nativeRequest.Key == "battery_status") nativeRequest.Fulfill(GetBatteryStatus());
                if (nativeRequest.Key == "architecture") nativeRequest.Fulfill(GetArchitecture());
                if (nativeRequest.Key == "device_orientation") nativeRequest.Fulfill(GetOrientation());
                if (nativeRequest.Key == "app_version") nativeRequest.Fulfill(Application.version);
                if (nativeRequest.Key == "app_short_string") nativeRequest.Fulfill(Application.version);
                if (nativeRequest.Key == "app_name") nativeRequest.Fulfill(Application.productName);
                if (nativeRequest.Key == "language") nativeRequest.Fulfill(Application.systemLanguage.ToString());
                if (nativeRequest.Key == "form_factor") nativeRequest.Fulfill(GetDeviceType());
                if (nativeRequest.Key == "network_conn_type") nativeRequest.Fulfill(GetNetworkConnType());
                if (nativeRequest.Key == "iab_usp") nativeRequest.Fulfill(PlayerPrefs.GetString("IABUSPrivacy_String", null));
                if (nativeRequest.Key == "user_agent") nativeRequest.Fulfill(GetUserAgent());

#if KVA_UWP
                // Gather the waid (advertising ID) only for the UWP platform.
                if (nativeRequest.Key == "waid")
                {
                    Application.RequestAdvertisingIdentifierAsync((advertisingId, trackingEnabled, error) => { nativeRequest.Fulfill(advertisingId); });
                }

                if (nativeRequest.Key == "device_limit_tracking")
                {
                    Application.RequestAdvertisingIdentifierAsync((advertisingId, trackingEnabled, error) => { nativeRequest.Fulfill(trackingEnabled); });
                }
#endif
                nativeRequest.Fulfill();
            }

            // Write Net Std data to storage.
            private void ProcessNetStdNativeRequestWriteStringToDisk(KochavaNetStd.NativeRequest nativeRequest)
            {
                if (nativeRequest.Value == null)
                {
                    PlayerPrefs.DeleteKey(nativeRequest.Key);
                }
                else
                {
                    PlayerPrefs.SetString(nativeRequest.Key, nativeRequest.Value.ToString());
                }

                nativeRequest.Fulfill();
            }
            
            // Read Net Std data from storage.
            private void ProcessNetStdNativeRequestGetStringFromDisk(KochavaNetStd.NativeRequest nativeRequest)
            {
                nativeRequest.Fulfill(PlayerPrefs.GetString(nativeRequest.Key, null));
            }
            
            // Erase all Net Std data from storage.
            private void ProcessNetStdNativeRequestErasePersistedData(KochavaNetStd.NativeRequest nativeRequest)
            {
                var keys = JArray.Parse(nativeRequest.Key);
                foreach (var key in keys)
                {
                    PlayerPrefs.DeleteKey(key.ToObject<string>());
                }

                nativeRequest.Fulfill();
            }
            
            // Net Std migrate storage from legacy Unity.
            private void ProcessNetStdNativeRequestAttemptRestoreKochavaDeviceIdBackup(KochavaNetStd.NativeRequest nativeRequest)
            {
                // Attempt to read the legacy net sdk's persisted storage.
                var profileSerialized = PlayerPrefs.GetString("kou__profile", null);
                if (string.IsNullOrEmpty(profileSerialized))
                {
                    nativeRequest.Fulfill();
                    return;
                }

                // Parse the profile.
                var profile = JObject.Parse(profileSerialized);
                var deviceId = profile["kochavaDeviceID"]?.ToObject<string>() ?? "";
                var installSent = profile["initialComplete"]?.ToObject<bool>() ?? false;

                // If no migration data then fulfill empty.
                if (string.IsNullOrEmpty(deviceId))
                {
                    nativeRequest.Fulfill();
                    return;
                }

                // Fulfill with the migration data.
                var migrationData = new JObject();
                migrationData["kochava_device_id"] = deviceId;
                migrationData["install_sent"] = installSent;
                nativeRequest.Fulfill(migrationData.ToString());
            }

            // Perform a Net Std network request.
            private IEnumerator ProcessNetStdNativeRequestNetworkRequest(KochavaNetStd.NativeRequest nativeRequest)
            {
                // Parse the request properties.
                var value = JObject.Parse(nativeRequest.Key ?? "{}");
                var url = Util.OptString(value["url"], "");
                var method = Util.OptBool(value["isGET"], false) ? "GET" : "POST";
                var body = Util.OptString(value["jsonContentBody"], "");
                var userAgent = Util.OptString(value["userAgent"], "");

                // Return as error if there is no url.
                if (url == "")
                {
                    nativeRequest.Fulfill( Tuple.Create(false, ""));
                    yield break;
                }

                // Build the network request.
                var www = new UnityWebRequest(url, method);
                www.timeout = 20;
                www.downloadHandler = new DownloadHandlerBuffer();
                www.disposeDownloadHandlerOnDispose = true;
#if !KVA_WEBGL
                // Note: The user agent cannot be overridden on the WebGL platform.
                if(!string.IsNullOrEmpty(userAgent))
                {
                    www.SetRequestHeader("User-Agent", userAgent);
                }
#endif
                if(!string.IsNullOrEmpty(body))
                {
                    www.uploadHandler = new UploadHandlerRaw (System.Text.Encoding.UTF8.GetBytes (body));
                    www.disposeUploadHandlerOnDispose = true;
                    www.SetRequestHeader("Content-Type", "application/json");
                }

                // Perform the network request
                yield return www.SendWebRequest();

                // Check for error
#if UNITY_2020_1_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
                {
                    nativeRequest.Fulfill( Tuple.Create(false, ""));
                    www.Dispose();
                    yield break;
                }
#else
                if (www.isNetworkError || www.isHttpError)
                {
                    nativeRequest.Fulfill( Tuple.Create(false, ""));
                    www.Dispose();
                    yield break;
                }
#endif
            
                // Fulfill with the response string.
                nativeRequest.Fulfill( Tuple.Create(true, www.downloadHandler.text ?? ""));
                www.Dispose();
            }
#endif

            // Returns the current platform.
            private static string GetPlatform()
            {
#if UNITY_EDITOR
                return "UnityEditor";
#elif UNITY_WEBGL
                return "WebGL";
#elif UNITY_STANDALONE_OSX
                return "MacOSX";
#elif UNITY_STANDALONE_LINUX
                return "Linux";
#elif UNITY_STANDALONE_WIN
                return "WindowsDesktop";
#elif UNITY_WII
                return "Wii";
#elif UNITY_PS4
                return "PS4";
#elif UNITY_XBOXONE
                return "XboxOne";
#elif UNITY_TIZEN
                return "Tizen";
#elif UNITY_TVOS
                return "tvos";
#elif UNITY_IOS
                return "ios";
#elif UNITY_ANDROID
                return "android";
#elif UNITY_WSA
                return "windows";
#else
                return Application.platform.ToString();
#endif
            }

            // Returns a generated user agent.
            private static string GetUserAgent()
            {
                return "Mozilla/5.0 (" + SystemInfo.operatingSystem + ")";
            }

            // Returns the application package name.
            // If the platform supports a bundle/package directly that is returned. Otherwise a package name is generated.
            private static string GetPackage()
            {
                if (!string.IsNullOrEmpty(Application.identifier))
                {
                    return Application.identifier;
                }

                return (Application.companyName + "." + Application.productName + "." + GetPlatform()).ToLowerInvariant().Replace(" ", "");
            }

            // Returns the device CPU architecture.
            private static string GetArchitecture()
            {
                return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            }

            // Returns the current device screen orientation.
            private static string GetOrientation()
            {
                if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
                {
                    return "landscape";
                }

                if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown ||
                    (Screen.width < Screen.height))
                {
                    return "portrait";
                }

                return "landscape";
            }

            // Returns the device type or form factor.
            private static string GetDeviceType()
            {
                var deviceType = SystemInfo.deviceType;
                switch (deviceType)
                {
                    case DeviceType.Handheld:
                        return "handheld";
                    case DeviceType.Desktop:
                        return "desktop";
                    case DeviceType.Console:
                        return "console";
                    default:
                        return "unknown";
                }
            }

            // Returns the current network connection type or none if not connected.
            private static string GetNetworkConnType()
            {
                var reachability = Application.internetReachability;
                switch (reachability)
                {
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        return "cellular";
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        return "wifi";
                    default:
                        return "none";
                }
            }

            // Returns the current battery status. Will be unknown on devices that are not battery powered.
            private static string GetBatteryStatus()
            {
                var batteryStatus = SystemInfo.batteryStatus;
                switch (batteryStatus)
                {
                    case BatteryStatus.Charging:
                        return "charging";
                    case BatteryStatus.Discharging:
                        return "discharging";
                    case BatteryStatus.Full:
                        return "full";
                    case BatteryStatus.NotCharging:
                        return "not_charging";
                    default:
                        return "unknown";
                }
            }
        }

        // Native SDK API Interface
        internal abstract class NativeApi : MonoBehaviour
        {
            // Unity update tick.
            internal virtual void Update()
            {
                // Empty default implementation.
            }
            
            // Reserved function, only use if directed to by your Client Success Manager.
            internal virtual void ExecuteAdvancedInstruction(string name, string value)
            {
                LogApiUnavailable("ExecuteAdvancedInstruction");
            }

            // Sets the log level. This should be set prior to starting the SDK.
            internal virtual void SetLogLevel(KochavaMeasurementLogLevel logLevel)
            {
                LogApiUnavailable("SetLogLevel");
            }

            // Sets the sleep state.
            internal virtual void SetSleep(bool sleep)
            {
                LogApiUnavailable("SetSleep");
            }

            // Sets if app level advertising tracking should be limited.
            internal virtual void SetAppLimitAdTracking(bool appLimitAdTracking)
            {
                LogApiUnavailable("SetAppLimitAdTracking");
            }

            // Register a custom device identifier for install attribution.
            internal virtual void RegisterCustomDeviceIdentifier(string name, string value)
            {
                LogApiUnavailable("RegisterCustomDeviceIdentifier");
            }
            
            // Register a custom value to be included in SDK payloads.
            internal virtual void RegisterCustomStringValue(string name, string value)
            {
                LogApiUnavailable("RegisterCustomStringValue");
            }

            // Register a custom value to be included in SDK payloads.
            internal virtual void RegisterCustomBoolValue(string name, bool? value)
            {
                LogApiUnavailable("RegisterCustomBoolValue");
            }

            // Register a custom value to be included in SDK payloads.
            internal virtual void RegisterCustomNumberValue(string name, double? value)
            {
                LogApiUnavailable("RegisterCustomNumberValue");
            }

            // Registers an Identity Link that allows linking different identities together in the form of key and value pairs.
            internal virtual void RegisterIdentityLink(string name, string value)
            {
                LogApiUnavailable("RegisterIdentityLink");
            }

            // (Android Only) Enable the Instant App feature by setting the instant app guid.
            internal virtual void EnableAndroidInstantApps(string instantAppGuid)
            {
                LogApiUnavailable("EnableAndroidInstantApps");
            }

            // (iOS Only) Enables App Clips by setting the Container App Group Identifier for App Clips data migration.
            internal virtual void EnableIosAppClips(string containerAppGroupIdentifier)
            {
                LogApiUnavailable("EnableIosAppClips");
            }

            // (iOS Only) Enables App Tracking Transparency.
            internal virtual void EnableIosAtt()
            {
                LogApiUnavailable("EnableIosAtt");
            }

            // (iOS Only) Sets the amount of time in seconds to wait for App Tracking Transparency Authorization. Default 30 seconds.
            internal virtual void SetIosAttAuthorizationWaitTime(double waitTime)
            {
                LogApiUnavailable("SetIosAttAuthorizationWaitTime");
            }

            // (iOS Only) Sets if the SDK should automatically request App Tracking Transparency Authorization on start. Default true.
            internal virtual void SetIosAttAuthorizationAutoRequest(bool autoRequest)
            {
                LogApiUnavailable("SetIosAttAuthorizationAutoRequest");
            }

            // Register a privacy profile, creating or overwriting an existing pofile.
            internal virtual void RegisterPrivacyProfile(string name, string[] keys)
            {
                LogApiUnavailable("RegisterPrivacyProfile");
            }

            // Enable or disable an existing privacy profile.
            internal virtual void SetPrivacyProfileEnabled(string name, bool enabled)
            {
                LogApiUnavailable("SetPrivacyProfileEnabled");
            }

            // Register a deeplink wrapper domain for enhanced deeplink ESP integrations.
            internal virtual void RegisterDeeplinkWrapperDomain(string domain)
            {
                LogApiUnavailable("RegisterDeeplinkWrapperDomain");
            }
            
            // Set the init completed callback listener.
            internal virtual void SetInitCompletedListener(Action<KochavaMeasurementInit> callback)
            {
                LogApiUnavailable("SetInitCompletedListener");
            }
            
            // Set if consent has been explicitly opted in or out by the user.
            internal virtual void SetIntelligentConsentGranted(bool granted)
            {
                LogApiUnavailable("SetIntelligentConsentGranted");
            }

            // Returns if the SDK is currently started.
            internal virtual bool GetStarted()
            {
                LogApiUnavailable("GetStarted");
                return true;
            }

            // Start the SDK with an App Guid.
            internal virtual void StartWithAppGuid(string appGuid)
            {
                LogApiUnavailable("StartWithAppGuid");
            }

            // Start the SDK with an Partner Name.
            internal virtual void StartWithPartnerName(string partnerName)
            {
                LogApiUnavailable("StartWithPartnerName");
            }

            // Shuts down the SDK and optionally deletes all local SDK data.
            // NOTE: Care should be taken when using this method as deleting the SDK data will make it reset back to a first install state.
            internal virtual void Shutdown(bool deleteData)
            {
                LogApiUnavailable("Shutdown");
            }

            // Returns the Kochava Install ID via a callback.
            internal virtual void RetrieveInstallId(Action<string> callback)
            {
                LogApiUnavailable("RetrieveInstallId");
            }

            // Retrieves the latest install attribution data from the server.
            internal virtual void RetrieveInstallAttribution(Action<KochavaMeasurementInstallAttribution> callback)
            {
                LogApiUnavailable("RetrieveInstallAttribution");
            }

            // Process a launch deeplink using the default 10 second timeout.
            internal virtual void ProcessDeeplink(string path, Action<KochavaMeasurementDeeplink> callback)
            {
                LogApiUnavailable("ProcessDeeplink");
            }

            // Process a launch deeplink using a custom timeout in seconds.
            internal virtual void ProcessDeeplinkWithOverrideTimeout(string path, double timeout, Action<KochavaMeasurementDeeplink> callback)
            {
                LogApiUnavailable("ProcessDeeplinkWithOverrideTimeout");
            }

            // (Android and iOS Only) Set the push token.
            internal virtual void RegisterPushToken(string token)
            {
                LogApiUnavailable("RegisterPushToken");
            }

            // (Android and iOS Only) Set the push enabled state.
            internal virtual void SetPushEnabled(bool enabled)
            {
                LogApiUnavailable("SetPushEnabled");
            }
            
            // Registers a default parameter on every event.
            internal virtual void RegisterDefaultEventStringParameter(string name, string value)
            {
                LogApiUnavailable("RegisterDefaultEventStringParameter");
            }

            // Registers a default parameter on every event.
            internal virtual void RegisterDefaultEventBoolParameter(string name, bool? value)
            {
                LogApiUnavailable("RegisterDefaultEventBoolParameter");
            }

            // Registers a default parameter on every event.
            internal virtual void RegisterDefaultEventNumberParameter(string name, double? value)
            {
                LogApiUnavailable("RegisterDefaultEventNumberParameter");
            }

            // Registers a default user_id value on every event.
            internal virtual void RegisterDefaultEventUserId(string value)
            {
                LogApiUnavailable("RegisterDefaultEventUserId");
            }

            // Send an event.
            internal virtual void SendEvent(string name)
            {
                LogApiUnavailable("SendEvent");
            }

            // Send an event with string data.
            internal virtual void SendEventWithString(string name, string data)
            {
                LogApiUnavailable("SendEventWithString");
            }

            // Send an event with dictionary data.
            internal virtual void SendEventWithDictionary(string name, JObject data)
            {
                LogApiUnavailable("SendEventWithDictionary");
            }

            // Send an event object (Called via Event.send().
            internal virtual void SendEventWithEvent(KochavaMeasurementEvent standardEvent)
            {
                LogApiUnavailable("SendEventWithEvent");
            }

            // Returns the log level as a string.
            internal string GetLogLevelString(KochavaMeasurementLogLevel logLevel)
            {
                switch (logLevel)
                {
                    case KochavaMeasurementLogLevel.None:
                        return "none";
                    case KochavaMeasurementLogLevel.Error:
                        return "error";
                    case KochavaMeasurementLogLevel.Warn:
                        return "warn";
                    case KochavaMeasurementLogLevel.Info:
                        return "info";
                    case KochavaMeasurementLogLevel.Debug:
                        return "debug";
                    case KochavaMeasurementLogLevel.Trace:
                        return "trace";
                    default:
                        return "info";
                }
            }

            // Logs that the particular API Method name is unavailable on this platorm.
            private static void LogApiUnavailable(string apiName)
            {
                Util.Log("Warn: " + apiName + " API is not available on this platform.");
            }
        }

        // Common utility methods
        internal class Util
        {
            // Log a message with the Kochava prefix.
            public static void Log(string message)
            {
                Debug.Log("KVA/Measurement: " + message);
            }
            
            public static JObject OptJObject(JToken item)
            {
                return item?.ToObject<JObject>();
            }

            public static JObject OptJObject(JToken item, JObject defaultValue)
            {
                return OptJObject(item) ?? defaultValue;
            }
            
            public static string OptString(JToken item)
            {
                return item?.ToObject<string>();
            }

            public static string OptString(JToken item, string defaultValue)
            {
                return OptString(item) ?? defaultValue;
            }

            public static bool? OptBool(JToken item)
            {
                return item?.ToObject<bool?>();
            }

            public static bool OptBool(JToken item, bool defaultValue)
            {
                return OptBool(item) ?? defaultValue;
            }
        }

        #region Android

#if KVA_ANDROID
        // API access the native Android SDK
        internal class NativeAndroid : NativeApi
        {
            // State
            private AndroidJavaObject AndroidContext;
            private AndroidJavaObject AndroidMeasurement;
            private AndroidJavaObject AndroidEvents;
            private AndroidJavaObject AndroidEngagement;

            // Internal install attribution handler
            private class AndroidInstallAttributionHandler : AndroidJavaProxy
            {
                private readonly string RequestId;

                public AndroidInstallAttributionHandler(string requestId) : base("com.kochava.tracker.attribution.RetrievedInstallAttributionListener")
                {
                    RequestId = requestId;
                }

                // Implementation of Android Native SDK callback. Do not change signature.
                void onRetrievedInstallAttribution(AndroidJavaObject attributionJava)
                {
                    var serializedJson = attributionJava.Call<AndroidJavaObject>("toJson").Call<string>("toString");
                    var attribution = new KochavaMeasurementInstallAttribution(JObject.Parse(serializedJson));
                    SingletonHandler.Instance.AddInstallAttributionResponse(RequestId, attribution);
                }
            }

            // Internal deeplink handler
            private class AndroidDeeplinkHandler : AndroidJavaProxy
            {
                private readonly string RequestId;

                public AndroidDeeplinkHandler(string requestId) : base("com.kochava.tracker.deeplinks.ProcessedDeeplinkListener")
                {
                    RequestId = requestId;
                }

                // Implementation of Android Native SDK callback. Do not change signature.
                void onProcessedDeeplink(AndroidJavaObject deeplinkJava)
                {
                    var serializedJson = deeplinkJava.Call<AndroidJavaObject>("toJson").Call<string>("toString");
                    var deeplink = new KochavaMeasurementDeeplink(JObject.Parse(serializedJson));
                    SingletonHandler.Instance.AddDeeplinkResponse(RequestId, deeplink);
                }
            }
            
            // Internal init handler
            private class AndroidInitHandler : AndroidJavaProxy
            {
                public AndroidInitHandler() : base("com.kochava.tracker.init.CompletedInitListener")
                {
                }

                // Implementation of Android Native SDK callback. Do not change signature.
                void onCompletedInit(AndroidJavaObject initJava)
                {
                    var serializedJson = initJava.Call<AndroidJavaObject>("toJson").Call<string>("toString");
                    var init = new KochavaMeasurementInit(JObject.Parse(serializedJson));
                    SingletonHandler.Instance.AddInitCompletedResponse(init);
                }
            }

            // Retrieve the instances of the Android Context and the Native SDK Singletons.
            internal NativeAndroid()
            {
                // Retrieve the Android Application Context.
                using (var androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var androidActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidContext = androidActivity.Call<AndroidJavaObject>("getApplicationContext");
                }

                // Retrieve the Android Measurement Singleton
                using (var measurementClass = new AndroidJavaClass("com.kochava.tracker.Tracker"))
                {
                    AndroidMeasurement = measurementClass.CallStatic<AndroidJavaObject>("getInstance");
                }

                // Retrieve the Android Events Singleton
                using (var eventsClass = new AndroidJavaClass("com.kochava.tracker.events.Events"))
                {
                    AndroidEvents = eventsClass.CallStatic<AndroidJavaObject>("getInstance");
                }

                // Retrieve the Android Engagement Singleton
                using (var engagementClass = new AndroidJavaClass("com.kochava.tracker.engagement.Engagement"))
                {
                    AndroidEngagement = engagementClass.CallStatic<AndroidJavaObject>("getInstance");
                }
            }

            // Reserved function, only use if directed to by your Client Success Manager.
            internal override void ExecuteAdvancedInstruction(string name, string value)
            {
                AndroidMeasurement.Call("executeAdvancedInstruction", name, value);
            }

            // Sets the log level. This should be set prior to starting the SDK.
            internal override void SetLogLevel(KochavaMeasurementLogLevel logLevel)
            {
                using (var logLevelClass = new AndroidJavaClass("com.kochava.tracker.log.LogLevel"))
                {
                    var logLevelString = GetLogLevelString(logLevel);
                    AndroidMeasurement.Call("setLogLevel", logLevelClass.CallStatic<AndroidJavaObject>("fromString", logLevelString));
                }
            }

            // Sets the sleep state.
            internal override void SetSleep(bool sleep)
            {
                AndroidMeasurement.Call("setSleep", sleep);
            }

            // Sets if app level advertising tracking should be limited.
            internal override void SetAppLimitAdTracking(bool appLimitAdTracking)
            {
                AndroidMeasurement.Call("setAppLimitAdTracking", appLimitAdTracking);
            }

            // Register a custom device identifier for install attribution.
            internal override void RegisterCustomDeviceIdentifier(string name, string value)
            {
                AndroidMeasurement.Call("registerCustomDeviceIdentifier", name, value);
            }
            
            // Register a custom value to be included in SDK payloads.
            internal override void RegisterCustomStringValue(string name, string value)
            {
                AndroidMeasurement.Call("registerCustomStringValue", name, value);
            }

            // Register a custom value to be included in SDK payloads.
            internal override void RegisterCustomBoolValue(string name, bool? value)
            {
                AndroidMeasurement.Call("registerCustomBoolValue", name, value != null ? new AndroidJavaObject("java.lang.Boolean", (bool) value) : null);
            }

            // Register a custom value to be included in SDK payloads.
            internal override void RegisterCustomNumberValue(string name, double? value)
            {
                AndroidMeasurement.Call("registerCustomNumberValue", name, value != null ? new AndroidJavaObject("java.lang.Double", (double) value) : null);
            }

            // Registers an Identity Link that allows linking different identities together in the form of key and value pairs.
            internal override void RegisterIdentityLink(string name, string value)
            {
                AndroidMeasurement.Call("registerIdentityLink", name, value);
            }

            // (Android Only) Enable the Instant App feature by setting the instant app guid.
            internal override void EnableAndroidInstantApps(string instantAppGuid)
            {
                AndroidMeasurement.Call("enableInstantApps", instantAppGuid);
            }

            // Register a privacy profile, creating or overwriting an existing pofile.
            internal override void RegisterPrivacyProfile(string name, string[] keys)
            {
                AndroidMeasurement.Call("registerPrivacyProfile", name, keys);
            }

            // Enable or disable an existing privacy profile.
            internal override void SetPrivacyProfileEnabled(string name, bool enabled)
            {
                AndroidMeasurement.Call("setPrivacyProfileEnabled", name, enabled);
            }

            // Register a deeplink wrapper domain for enhanced deeplink ESP integrations.
            internal override void RegisterDeeplinkWrapperDomain(string domain)
            {
                AndroidMeasurement.Call("registerDeeplinkWrapperDomain", domain);
            }
            
            // Set the init completed callback listener.
            internal override void SetInitCompletedListener(Action<KochavaMeasurementInit> callback)
            {
                SingletonHandler.Instance.SetInitCompletedHandler(callback);
                AndroidMeasurement.Call("setCompletedInitListener", callback != null ? new AndroidInitHandler() : null);
            }
            
            // Set if consent has been explicitly opted in or out by the user.
            internal override void SetIntelligentConsentGranted(bool granted)
            {
                AndroidMeasurement.Call("setIntelligentConsentGranted", granted);
            }

            // Returns if the SDK is currently started.
            internal override bool GetStarted()
            {
                return AndroidMeasurement.Call<bool>("isStarted");
            }

            // Start the SDK with an App Guid.
            internal override void StartWithAppGuid(string appGuid)
            {
                AndroidMeasurement.Call("startWithAppGuid", AndroidContext, appGuid);
            }

            // Start the SDK with a Partner Name.
            internal override void StartWithPartnerName(string partnerName)
            {
                AndroidMeasurement.Call("startWithPartnerName", AndroidContext, partnerName);
            }

            // Shuts down the SDK and optionally deletes all local SDK data.
            // NOTE: Care should be taken when using this method as deleting the SDK data will make it reset back to a first install state.
            internal override void Shutdown(bool deleteData)
            {
                AndroidMeasurement.Call("shutdown", AndroidContext, deleteData);
            }

            // Returns the Kochava Install ID via a callback.
            internal override void RetrieveInstallId(Action<string> callback)
            {
                var requestId = SingletonHandler.Instance.AddStringRequest(callback);
                var value = AndroidMeasurement.Call<string>("getDeviceId");
                SingletonHandler.Instance.AddStringResponse(requestId, value);
            }

            // Retrieves the latest install attribution data from the server.
            internal override void RetrieveInstallAttribution(Action<KochavaMeasurementInstallAttribution> callback)
            {
                var requestId = SingletonHandler.Instance.AddInstallAttributionRequest(callback);
                AndroidMeasurement.Call("retrieveInstallAttribution", new AndroidInstallAttributionHandler(requestId));
            }

            // Process a launch deeplink using the default 10 second timeout.
            internal override void ProcessDeeplink(string path, Action<KochavaMeasurementDeeplink> callback)
            {
                var requestId = SingletonHandler.Instance.AddDeeplinkRequest(callback);
                AndroidMeasurement.Call("processDeeplink", path, new AndroidDeeplinkHandler(requestId));
            }

            // Process a launch deeplink using a custom timeout in seconds.
            internal override void ProcessDeeplinkWithOverrideTimeout(string path, double timeout, Action<KochavaMeasurementDeeplink> callback)
            {
                var requestId = SingletonHandler.Instance.AddDeeplinkRequest(callback);
                AndroidMeasurement.Call("processDeeplink", path, timeout, new AndroidDeeplinkHandler(requestId));
            }

            // (Android and iOS Only) Set the push token.
            internal override void RegisterPushToken(string token)
            {
                AndroidEngagement.Call("registerPushToken", token);
            }

            // (Android and iOS Only) Set the push enabled state.
            internal override void SetPushEnabled(bool enabled)
            {
                AndroidEngagement.Call("setPushEnabled", enabled);
            }
            
            // Registers a default parameter on every event.
            internal override void RegisterDefaultEventStringParameter(string name, string value)
            {
                AndroidEvents.Call("registerDefaultStringParameter", name, value);
            }

            // Registers a default parameter on every event.
            internal override void RegisterDefaultEventBoolParameter(string name, bool? value)
            {
                AndroidEvents.Call("registerDefaultBoolParameter", name, value != null ? new AndroidJavaObject("java.lang.Boolean", (bool) value) : null);
            }

            // Registers a default parameter on every event.
            internal override void RegisterDefaultEventNumberParameter(string name, double? value)
            {
                AndroidEvents.Call("registerDefaultNumberParameter", name, value != null ? new AndroidJavaObject("java.lang.Double", (double) value) : null);
            }

            // Registers a default user_id value on every event.
            internal override void RegisterDefaultEventUserId(string value)
            {
                AndroidEvents.Call("registerDefaultUserId", value);
            }
            
            // Send an event.
            internal override void SendEvent(string name)
            {
                AndroidEvents.Call("send", name);
            }

            // Send an event with string data.
            internal override void SendEventWithString(string name, string data)
            {
                AndroidEvents.Call("sendWithString", name, data);
            }

            // Send an event with dictionary data.
            internal override void SendEventWithDictionary(string name, JObject data)
            {
                AndroidJavaObject jsonData = null;
                if (data != null)
                {
                    jsonData = new AndroidJavaObject("org.json.JSONObject", data.ToString());
                }

                AndroidEvents.Call("sendWithDictionary", name, jsonData);
            }

            // Send an event object (Called via Event.send().
            internal override void SendEventWithEvent(KochavaMeasurementEvent standardEvent)
            {
                using (var eventClass = new AndroidJavaClass("com.kochava.tracker.events.Event"))
                {
                    // Create the event.
                    var myEvent = eventClass.CallStatic<AndroidJavaObject>("buildWithEventName", standardEvent.GetEventName());

                    // Add the event data.
                    var eventData = new AndroidJavaObject("org.json.JSONObject", standardEvent.GetEventData().ToString());
                    myEvent.Call<AndroidJavaObject>("mergeCustomDictionary", eventData);

                    // Add the google receipt if present.
                    if (standardEvent.GetAndroidGooglePlayReceiptData() != null && standardEvent.GetAndroidGooglePlayReceiptSignature() != null)
                    {
                        myEvent.Call<AndroidJavaObject>("setGooglePlayReceipt", standardEvent.GetAndroidGooglePlayReceiptData(), standardEvent.GetAndroidGooglePlayReceiptSignature());
                    }

                    // Send the event.
                    myEvent.Call("send");
                }
            }
        }
#endif

        #endregion

        #region iOS

#if KVA_IOS
        // API access for the native iOS SDK
        internal class NativeIos : NativeApi
        {
            // Reserved function, only use if directed to by your Client Success Manager.
            internal override void ExecuteAdvancedInstruction(string name, string value)
            {
                iosNativeExecuteAdvancedInstruction(name, value);
            }

            // Sets the log level. This should be set prior to starting the SDK.
            internal override void SetLogLevel(KochavaMeasurementLogLevel logLevel)
            {
                iosNativeSetLogLevel(GetLogLevelString(logLevel));
            }

            // Sets the sleep state.
            internal override void SetSleep(bool sleep)
            {
                iosNativeSetSleep(sleep);
            }

            // Sets if app level advertising tracking should be limited.
            internal override void SetAppLimitAdTracking(bool appLimitAdTracking)
            {
                iosNativeSetAppLimitAdTracking(appLimitAdTracking);
            }

            // Register a custom device identifier for install attribution.
            internal override void RegisterCustomDeviceIdentifier(string name, string value)
            {
                iosNativeRegisterCustomDeviceIdentifier(name, value);
            }

            // Register a custom value to be included in SDK payloads.
            internal override void RegisterCustomStringValue(string name, string value)
            {
                iosNativeRegisterCustomStringValue(name, value);
            }

            // Register a custom value to be included in SDK payloads.
            internal override void RegisterCustomBoolValue(string name, bool? value)
            {
                iosNativeRegisterCustomBoolValue(name, value ?? false, value != null);
            }

            // Register a custom value to be included in SDK payloads.
            internal override void RegisterCustomNumberValue(string name, double? value)
            {
                iosNativeRegisterCustomNumberValue(name, value ?? 0.0, value != null);
            }

            // Registers an Identity Link that allows linking different identities together in the form of key and value pairs.
            internal override void RegisterIdentityLink(string name, string value)
            {
                iosNativeRegisterIdentityLink(name, value);
            }

            // (iOS Only) Enables App Clips by setting the Container App Group Identifier for App Clips data migration.
            internal override void EnableIosAppClips(string containerAppGroupIdentifier)
            {
                iosNativeEnableIosAppClips(containerAppGroupIdentifier);
            }

            // (iOS Only) Enables App Tracking Transparency.
            internal override void EnableIosAtt()
            {
                iosNativeEnableIosAtt();
            }

            // (iOS Only) Sets the amount of time in seconds to wait for App Tracking Transparency Authorization. Default 30 seconds.
            internal override void SetIosAttAuthorizationWaitTime(double waitTime)
            {
                iosNativeSetIosAttAuthorizationWaitTime(waitTime);
            }

            // (iOS Only) Sets if the SDK should automatically request App Tracking Transparency Authorization on start. Default true.
            internal override void SetIosAttAuthorizationAutoRequest(bool autoRequest)
            {
                iosNativeSetIosAttAuthorizationAutoRequest(autoRequest);
            }

            // Register a privacy profile, creating or overwriting an existing pofile.
            internal override void RegisterPrivacyProfile(string name, string[] keys)
            {
                string keysSerialized = null;
                if (keys != null)
                {
                    keysSerialized = JArray.FromObject(keys).ToString();
                }

                iosNativeRegisterPrivacyProfile(name, keysSerialized);
            }

            // Enable or disable an existing privacy profile.
            internal override void SetPrivacyProfileEnabled(string name, bool enabled)
            {
                iosNativeSetPrivacyProfileEnabled(name, enabled);
            }

            // Register a deeplink wrapper domain for enhanced deeplink ESP integrations.
            internal override void RegisterDeeplinkWrapperDomain(string domain)
            {
                iosNativeRegisterDeeplinkWrapperDomain(domain);
            }
            
            // Set the init completed callback listener.
            internal override void SetInitCompletedListener(Action<KochavaMeasurementInit> callback)
            {
                SingletonHandler.Instance.SetInitCompletedHandler(callback);
                iosNativeSetInitCompletedListener(callback != null);
            }
            
            // Set if consent has been explicitly opted in or out by the user.
            internal override void SetIntelligentConsentGranted(bool granted)
            {
                iosNativeSetIntelligentConsentGranted(granted);
            }

            // Returns if the SDK is currently started.
            internal override bool GetStarted()
            {
                return iosNativeGetStarted();
            }

            // Start the SDK with an App Guid.
            internal override void StartWithAppGuid(string appGuid)
            {
                iosNativeStartWithAppGuid(appGuid);
            }

            // Start the SDK with an Partner Name.
            internal override void StartWithPartnerName(string partnerName)
            {
                iosNativeStartWithPartnerName(partnerName);
            }

            // Shuts down the SDK and optionally deletes all local SDK data.
            // NOTE: Care should be taken when using this method as deleting the SDK data will make it reset back to a first install state.
            internal override void Shutdown(bool deleteData)
            {
                iosNativeShutdown(deleteData);
            }

            // Returns the Kochava Install ID via a callback.
            internal override void RetrieveInstallId(Action<string> callback)
            {
                var requestId = SingletonHandler.Instance.AddStringRequest(callback);
                var value = iosNativeRetrieveInstallId();
                SingletonHandler.Instance.AddStringResponse(requestId, value);
            }

            // Retrieves the latest install attribution data from the server.
            internal override void RetrieveInstallAttribution(Action<KochavaMeasurementInstallAttribution> callback)
            {
                var requestId = SingletonHandler.Instance.AddInstallAttributionRequest(callback);
                iosNativeRetrieveInstallAttribution(requestId);
            }

            // Process a launch deeplink using the default 10 second timeout.
            internal override void ProcessDeeplink(string path, Action<KochavaMeasurementDeeplink> callback)
            {
                var requestId = SingletonHandler.Instance.AddDeeplinkRequest(callback);
                iosNativeProcessDeeplink(path, requestId);
            }

            // Process a launch deeplink using a custom timeout in seconds.
            internal override void ProcessDeeplinkWithOverrideTimeout(string path, double timeout, Action<KochavaMeasurementDeeplink> callback)
            {
                var requestId = SingletonHandler.Instance.AddDeeplinkRequest(callback);
                iosNativeProcessDeeplinkWithOverrideTimeout(path, timeout, requestId);
            }

            // (Android and iOS Only) Set the push token.
            internal override void RegisterPushToken(string token)
            {
                iosNativeRegisterPushToken(token);
            }

            // (Android and iOS Only) Set the push enabled state.
            internal override void SetPushEnabled(bool enabled)
            {
                iosNativeSetPushEnabled(enabled);
            }

            // Registers a default parameter on every event.
            internal override void RegisterDefaultEventStringParameter(string name, string value)
            {
                iosNativeRegisterDefaultEventStringParameter(name, value);
            }

            // Registers a default parameter on every event.
            internal override void RegisterDefaultEventBoolParameter(string name, bool? value)
            {
                iosNativeRegisterDefaultEventBoolParameter(name, value ?? false, value != null);
            }

            // Registers a default parameter on every event.
            internal override void RegisterDefaultEventNumberParameter(string name, double? value)
            {
                iosNativeRegisterDefaultEventNumberParameter(name, value ?? 0.0, value != null);
            }

            // Registers a default user_id value on every event.
            internal override void RegisterDefaultEventUserId(string value)
            {
                iosNativeRegisterDefaultEventUserId(value);
            }

            // Send an event.
            internal override void SendEvent(string name)
            {
                iosNativeSendEvent(name);
            }

            // Send an event with string data.
            internal override void SendEventWithString(string name, string data)
            {
                iosNativeSendEventWithString(name, data);
            }

            // Send an event with dictionary data.
            internal override void SendEventWithDictionary(string name, JObject data)
            {
                string dataSerialized = null;
                if (data != null)
                {
                    dataSerialized = data.ToString();
                }

                iosNativeSendEventWithDictionary(name, dataSerialized);
            }

            // Send an event object (Called via Event.send().
            internal override void SendEventWithEvent(KochavaMeasurementEvent standardEvent)
            {
                var json = new JObject();
                json.Add("name", standardEvent.GetEventName());
                json.Add("data", standardEvent.GetEventData());
                json.Add("iosAppStoreReceiptBase64String", standardEvent.GetIosAppStoreReceiptBase64String());
                iosNativeSendEventWithEvent(json.ToString());
            }

            // .mm layer interface. These methods are found in the KochavaWrapper.mm file.
            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeExecuteAdvancedInstruction(string name, string value);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSetLogLevel(string logLevel);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSetSleep(bool sleep);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSetAppLimitAdTracking(bool appLimitAdTracking);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterCustomDeviceIdentifier(string name, string value);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterCustomStringValue(string name, string value);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterCustomBoolValue(string name, bool value, bool isValidValue);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterCustomNumberValue(string name, double value, bool isValidValue);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterIdentityLink(string name, string value);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeEnableIosAppClips(string identifier);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeEnableIosAtt();

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSetIosAttAuthorizationWaitTime(double waitTime);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSetIosAttAuthorizationAutoRequest(bool autoRequest);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterPrivacyProfile(string name, string keysSerialized);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSetPrivacyProfileEnabled(string name, bool enabled);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterDeeplinkWrapperDomain(string domain);
            
            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSetInitCompletedListener(bool setListener);
            
            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSetIntelligentConsentGranted(bool granted);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern bool iosNativeGetStarted();

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeStartWithAppGuid(string appGuid);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeStartWithPartnerName(string partnerName);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeShutdown(bool deleteData);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern string iosNativeRetrieveInstallId();

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRetrieveInstallAttribution(string requestId);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeProcessDeeplink(string path, string requestId);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeProcessDeeplinkWithOverrideTimeout(string path, double timeout, string requestId);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterPushToken(string token);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSetPushEnabled(bool enabled);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterDefaultEventStringParameter(string name, string value);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterDefaultEventBoolParameter(string name, bool value, bool isValidValue);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterDefaultEventNumberParameter(string name, double value, bool isValidValue);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeRegisterDefaultEventUserId(string value);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSendEvent(string name);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSendEventWithString(string name, string data);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSendEventWithDictionary(string name, string dataSerialized);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeSendEventWithEvent(string eventSerialized);
        }
#endif

        #endregion

        #region NetStd

#if KVA_NETSTD
        // API access for the native .NET SDK. This is used for all platforms that are not Android, iOS, or WebGL.
        internal class NativeDotNet : NativeApi
        {
            private KochavaNetStd.Tracker Measurement;

            internal NativeDotNet()
            {
                InitializeMeasurement();
            }

            // Create a new instance of the Measurement and initialize logging.
            private void InitializeMeasurement()
            {
                // Create the Measurement instance.
                if (Measurement == null)
                {
                    Measurement = new KochavaNetStd.Tracker(nativeRequest =>
                    {
                        // Queue known actions to run on the Unity Thread.
                        if (nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.GatherDatapoint
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.WriteStringToDisk
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.GetStringFromDisk
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.ErasePersistedData
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.AttemptRestoreKochavaDeviceIdBackup
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.NetworkRequest)
                        {
                            SingletonHandler.Instance.AddNetStdNativeRequest(nativeRequest);
                        }
                        // Immediately fulfill any other actions.
                        else
                        {
                            nativeRequest.Fulfill();
                        }
                    });
                }

                // Initialize logging.
                KochavaNetStd.Global.PrettyPrintJson = true;
                SetLogLevel(KochavaMeasurementLogLevel.Info);
            }

            // Unity update tick
            internal override void Update()
            {
                Measurement.Update();
            }

            // Reserved function, only use if directed to by your Client Success Manager.
            internal override void ExecuteAdvancedInstruction(string name, string value)
            {
                Measurement.ExecuteAdvancedInstruction(name, value);
            }

            // Sets the log level. This should be set prior to starting the SDK.
            internal override void SetLogLevel(KochavaMeasurementLogLevel logLevel)
            {
                var nativeLogLevel = KochavaNetStd.Global.LogLevel.Info;
                switch (logLevel)
                {
                    case KochavaMeasurementLogLevel.None:
                        nativeLogLevel = KochavaNetStd.Global.LogLevel.None;
                        break;
                    case KochavaMeasurementLogLevel.Error:
                        nativeLogLevel = KochavaNetStd.Global.LogLevel.Error;
                        break;
                    case KochavaMeasurementLogLevel.Warn:
                        nativeLogLevel = KochavaNetStd.Global.LogLevel.Warn;
                        break;
                    case KochavaMeasurementLogLevel.Info:
                        nativeLogLevel = KochavaNetStd.Global.LogLevel.Info;
                        break;
                    case KochavaMeasurementLogLevel.Debug:
                        nativeLogLevel = KochavaNetStd.Global.LogLevel.Debug;
                        break;
                    case KochavaMeasurementLogLevel.Trace:
                        nativeLogLevel = KochavaNetStd.Global.LogLevel.Trace;
                        break;
                }

                KochavaNetStd.Global.InitializeLogging(Debug.Log, nativeLogLevel);
            }

            // Sets the sleep state.
            internal override void SetSleep(bool sleep)
            {
                Measurement.Sleep = sleep;
            }

            // Sets if app level advertising tracking should be limited.
            internal override void SetAppLimitAdTracking(bool appLimitAdTracking)
            {
                Measurement.SetAppLimitAdTracking(appLimitAdTracking);
            }

            // Register a custom device identifier for install attribution.
            internal override void RegisterCustomDeviceIdentifier(string name, string value)
            {
                Measurement.RegisterCustomDeviceIdentifier(name, value);
            }

            // Registers an Identity Link that allows linking different identities together in the form of key and value pairs.
            internal override void RegisterIdentityLink(string name, string value)
            {
                Measurement.RegisterIdentityLink(name, value);
            }

            // Register a privacy profile, creating or overwriting an existing pofile.
            internal override void RegisterPrivacyProfile(string name, string[] keys)
            {
                Measurement.RegisterPrivacyProfile(name, new List<string>(keys ?? Array.Empty<string>()));
            }

            // Enable or disable an existing privacy profile.
            internal override void SetPrivacyProfileEnabled(string name, bool enabled)
            {
                Measurement.SetPrivacyProfileEnabled(name, enabled);
            }

            // Returns if the SDK is currently started.
            internal override bool GetStarted()
            {
                return Measurement.GetStarted();
            }

            // Start the SDK with an App Guid.
            internal override void StartWithAppGuid(string appGuid)
            {
                var config = new KochavaNetStd.HostConfiguration();
                config.AppGuid = appGuid;
                config.AccumulateSessionDurationPassively = true;
                Measurement.StartWithConfiguration(config);
            }

            // Start the SDK with an Partner Name.
            internal override void StartWithPartnerName(string partnerName)
            {
                var config = new KochavaNetStd.HostConfiguration();
                config.PartnerName = partnerName;
                config.AccumulateSessionDurationPassively = true;
                Measurement.StartWithConfiguration(config);
            }

            // Shuts down the SDK and optionally deletes all local SDK data.
            // NOTE: Care should be taken when using this method as deleting the SDK data will make it reset back to a first install state.
            internal override void Shutdown(bool deleteData)
            {
                Measurement.Shutdown(deleteData);
                InitializeMeasurement();
            }

            // Returns the Kochava Install ID.
            internal override void RetrieveInstallId(Action<string> callback)
            {
                var requestId = SingletonHandler.Instance.AddStringRequest(callback);
                Measurement.GetDeviceId(value =>
                {
                    SingletonHandler.Instance.AddStringResponse(requestId, value ?? "");
                });
            }

            // Retrieves the latest install attribution data from the server.
            internal override void RetrieveInstallAttribution(Action<KochavaMeasurementInstallAttribution> callback)
            {
                var requestId = SingletonHandler.Instance.AddInstallAttributionRequest(callback);
                Measurement.RetrieveAttribution(value =>
                {
                    SingletonHandler.Instance.AddInstallAttributionResponse(requestId, new KochavaMeasurementInstallAttribution(value.Retrieved, value.RawResults, value.Attributed, value.FirstInstall));
                });
            }

            // Process a launch deeplink using the default 10 second timeout.
            internal override void ProcessDeeplink(string path, Action<KochavaMeasurementDeeplink> callback)
            {
                var requestId = SingletonHandler.Instance.AddDeeplinkRequest(callback);
                Measurement.ProcessDeeplink(path, value =>
                {
                    SingletonHandler.Instance.AddDeeplinkResponse(requestId, new KochavaMeasurementDeeplink(value.Destination, value.RawResults));
                });
            }

            // Process a launch deeplink using a custom timeout in seconds.
            internal override void ProcessDeeplinkWithOverrideTimeout(string path, double timeout, Action<KochavaMeasurementDeeplink> callback)
            {
                var requestId = SingletonHandler.Instance.AddDeeplinkRequest(callback);
                Measurement.ProcessDeeplink(path, value =>
                {
                    SingletonHandler.Instance.AddDeeplinkResponse(requestId, new KochavaMeasurementDeeplink(value.Destination, value.RawResults));
                }, timeout);
            }

            // Send an event.
            internal override void SendEvent(string name)
            {
                var myEvent = new KochavaNetStd.StandardEvent(name);
                Measurement.SendEvent(myEvent);
            }

            // Send an event with string data.
            internal override void SendEventWithString(string name, string data)
            {
                var myEvent = new KochavaNetStd.StandardEvent(name, data);
                Measurement.SendEvent(myEvent);
            }

            // Send an event with dictionary data.
            internal override void SendEventWithDictionary(string name, JObject data)
            {
                var myEvent = new KochavaNetStd.StandardEvent(name, data);
                Measurement.SendEvent(myEvent);
            }

            // Send an event object (Called via Event.send().
            internal override void SendEventWithEvent(KochavaMeasurementEvent standardEvent)
            {
                var myEvent = new KochavaNetStd.StandardEvent(standardEvent.GetEventName(), standardEvent.GetEventData());
                Measurement.SendEvent(myEvent);
            }
        }
#endif

        #endregion
    }
}