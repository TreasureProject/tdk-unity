//
//  KochavaMeasurement (Unity)
//
//  Copyright (c) 2013 - 2023 Kochava, Inc. All rights reserved.
//

#pragma mark - Import
#import <KochavaNetworking/KochavaNetworking.h>
#import <KochavaMeasurement/KochavaMeasurement.h>

#pragma mark - Util

// Interface for the kochavaMeasurementUtil
@interface KochavaMeasurementUtil : NSObject

@end

// Common utility functions used by all of the wrappers.
// Any changes to the methods in here must be propagated to the other wrappers.
@implementation KochavaMeasurementUtil

// Log a message to the console.
+ (void)log:(nonnull NSString *)message {
    NSLog(@"KVA/Measurement: %@", message);
}

// Attempts to read an NSDictionary and returns nil if not one.
+ (nullable NSDictionary *)readNSDictionary:(nullable id)valueId {
    return [[NSDictionary class] performSelector:@selector(kva_from:) withObject:valueId];
}

// Attempts to read an NSArray and returns nil if not one.
+ (nullable NSArray *)readNSArray:(nullable id)valueId {
    return [[NSArray class] performSelector:@selector(kva_from:) withObject:valueId];
}

// Attempts to read an NSNumber and returns nil if not one.
+ (nullable NSNumber *)readNSNumber:(nullable id)valueId {
    return [[NSNumber class] performSelector:@selector(kva_from:) withObject:valueId];
}

// Attempts to read an NSString and returns nil if not one.
+ (nullable NSString *)readNSString:(nullable id)valueId {
    return [NSString kva_from:valueId];
}

// Attempts to read an NSObject and returns nil if not one.
+ (nullable NSObject *)readNSObject:(nullable id)valueId {
    return [valueId isKindOfClass:NSNull.self] ? nil : valueId;
}

// Converts an NSNumber to a double with fallback to a default value.
+ (double)convertNumberToDouble:(nullable NSNumber *)number defaultValue:(double)defaultValue {
    if(number != nil) {
        return [number doubleValue];
    }
    return defaultValue;
}

// Converts an NSNumber to a bool with fallback to a default value.
+ (BOOL)convertNumberToBool:(nullable NSNumber *)number defaultValue:(BOOL)defaultValue {
    if(number != nil) {
        return [number boolValue];
    }
    return defaultValue;
}

// Converts the deeplink result into an NSDictionary.
+ (nonnull NSDictionary *)convertDeeplinkToDictionary:(nonnull KVADeeplink *)deeplink {
    NSObject *object = [deeplink kva_toContext:KVAContext.host];
    return [object isKindOfClass:NSDictionary.class] ? (NSDictionary *)object : @{};
}

// Converts the install attribution result into an NSDictionary.
+ (nonnull NSDictionary *)convertInstallAttributionToDictionary:(nonnull KVAMeasurement_Attribution_Result *)installAttribution {
    if (KVAMeasurement.shared.startedBool) {
        NSObject *object = [installAttribution kva_toContext:KVAContext.host];
        return [object isKindOfClass:NSDictionary.class] ? (NSDictionary *)object : @{};
    } else {
        return @{
                @"retrieved": @(NO),
                @"raw": @{},
                @"attributed": @(NO),
                @"firstInstall": @(NO),
        };
    }
}

// Converts the config result into an NSDictionary.
+ (nonnull NSDictionary *)convertConfigToDictionary:(nonnull KVANetworking_Config *)config {
    return @{
            @"consentGdprApplies": @(config.consentGDPRAppliesBool),
    };
}

// Serialize an NSDictionary into a json serialized NSString.
+ (nullable NSString *)serializeJsonObject:(nullable NSDictionary *)dictionary {
    return [NSString kva_stringFromJSONObject:dictionary prettyPrintBool:NO];
}

// Parse a json serialized NSString into an NSArray.
+ (nullable NSArray *)parseJsonArray:(nullable NSString *)string {
    NSObject *object = [string kva_serializedJSONObjectWithPrintErrorsBool:YES];
    return ([object isKindOfClass:NSArray.class] ? (NSArray *) object : nil);
}

// Parse a json serialized NSString into an NSDictionary.
+ (nullable NSDictionary *)parseJsonObject:(nullable NSString *)string {
    NSObject *object = [string kva_serializedJSONObjectWithPrintErrorsBool:YES];
    return [object isKindOfClass:NSDictionary.class] ? (NSDictionary *) object : nil;
}

// Parse a NSString into a NSURL and logs a warning on failure.
+ (nullable NSURL *)parseNSURL:(nullable NSString *)string {
    NSURL *url = [NSURL URLWithString:string];
    if (url == nil && string.length > 0) {
        [KochavaMeasurementUtil log:@"Warn: parseNSURL invalid input, not a valid URL"];
    }
    return url;
}

// Builds and sends an event given an event info dictionary.
+ (void)buildAndSendEvent:(nullable NSDictionary *)eventInfo {
    if(eventInfo == nil) {
        return;
    }
    NSString *name = [KochavaMeasurementUtil readNSString:eventInfo[@"name"]];
    NSDictionary *data = [KochavaMeasurementUtil readNSDictionary:eventInfo[@"data"]];
    NSString *iosAppStoreReceiptBase64String = [KochavaMeasurementUtil readNSString:eventInfo[@"iosAppStoreReceiptBase64String"]];
    if (name.length > 0) {
        KVAEvent *event = [[KVAEvent alloc] initCustomWithEventName:name];
        if (data != nil) {
            event.infoDictionary = data;
        }
        if (iosAppStoreReceiptBase64String.length > 0) {
            event.appStoreReceiptBase64EncodedString = iosAppStoreReceiptBase64String;
        }
        [event send];
    } else {
        [KochavaMeasurementUtil log:@"Warn: sendEventWithEvent invalid input"];
    }
}

@end

#pragma mark - UnityUtil

// Copy a string in a way that can be passed to the C# layer without being freed too early.
// The mono layer will automatically free this after it is received there.
char *autonomousStringCopy(const char *string) {
    if (string == NULL) {
        return NULL;
    }

    char *res = (char *) malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

// Interface for the KochavaMeasurementPlugin
@interface KochavaMeasurementPlugin : NSObject

@end

// Native Wrapper to use for several methods.
@implementation KochavaMeasurementPlugin

// Set the logging parameters before any other access to the SDK.
+ (void) initialize {
    KVALog.shared.osLogEnabledBool = false;
    KVALog.shared.printLinesIndividuallyBool = true;
}

// Converts a C String to an NSString and return nil if the input is NULL.
+ (nullable NSString *)convertCStringToNSString:(nullable const char *)cString {
    if(cString == NULL) {
        return nil;
    }
    return [NSString stringWithUTF8String:cString];
}

@end

#pragma mark - Methods

// Subscribe to generic platform events. This should be done prior to starting the SDK.
void subscribeToPlatformEvents() {
    KVAMeasurement.shared.adNetwork.conversion.closure_didUpdatePostbackValue = ^(KVAMeasurement_AdNetwork_Conversion *_Nonnull conversion, KVAMeasurement_AdNetwork_Conversion_Result *_Nonnull result) {
        NSObject *valueObject = [result kva_toContext:KVAContext.host];
        NSDictionary *value = [KochavaMeasurementUtil readNSDictionary:valueObject] ?: @{};
        NSString *response = [KochavaMeasurementUtil serializeJsonObject:@{
                @"name": @"adNetworkConversionDidUpdateValue",
                @"value": value
        }];

        // send this message back to the host app, which must always have a game object and listener method with these names
        const char *a = "KochavaMeasurement";
        const char *b = "NativePlatformEventListener";
        UnitySendMessage(a, b, autonomousStringCopy([response UTF8String]));
    };

    KVAMeasurement.shared.adNetwork.closure_didRegisterAppForAttribution = ^(KVAMeasurement_AdNetwork *_Nonnull adNetwork) {
        NSString *response = [KochavaMeasurementUtil serializeJsonObject:@{
                @"name": @"adNetworkDidRegisterAppForAttribution",
                @"value": @{}
        }];

        // send this message back to the host app, which must always have a game object and listener method with these names
        const char *a = "KochavaMeasurement";
        const char *b = "NativePlatformEventListener";
        UnitySendMessage(a, b, autonomousStringCopy([response UTF8String]));
    };
}

// void executeAdvancedInstruction(string name, string value)
void iosNativeExecuteAdvancedInstruction(const char *nameUtf8, const char *valueUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *value = [KochavaMeasurementPlugin convertCStringToNSString:valueUtf8];

    [KVAMeasurement.shared.networking executeAdvancedInstructionWithUniversalIdentifier:name parameter:value prerequisiteTaskIdentifierArray:nil];
}

// void setLogLevel(LogLevel logLevel)
void iosNativeSetLogLevel(const char *logLevelUtf8) {
    NSString *logLevel = [KochavaMeasurementPlugin convertCStringToNSString:logLevelUtf8];

    KVALog.shared.level = [KVALog_Level from:logLevel];
}

// void setSleep(bool sleep)
void iosNativeSetSleep(bool sleep) {
    KVAMeasurement.shared.sleepBool = sleep;
}

// void setAppLimitAdTracking(bool appLimitAdTracking)
void iosNativeSetAppLimitAdTracking(bool appLimitAdTracking) {
    KVAMeasurement.shared.appLimitAdTracking.boolean = appLimitAdTracking;
}

// void registerCustomDeviceIdentifier(string name, string value)
void iosNativeRegisterCustomDeviceIdentifier(const char *nameUtf8, const char *valueUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *value = [KochavaMeasurementPlugin convertCStringToNSString:valueUtf8];
    
    [KVACustomIdentifier registerWithName:name identifier:value];
}

// void registerCustomStringValue(string name, string value)
void iosNativeRegisterCustomStringValue(const char *nameUtf8, const char *valueUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *value = [KochavaMeasurementPlugin convertCStringToNSString:valueUtf8];
    
    [KVACustomValue registerWithName:name value:value];
}

// void registerCustomBoolValue(string name, bool value)
void iosNativeRegisterCustomBoolValue(const char *nameUtf8, bool valueBool, bool isValidValue) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSNumber *value = [NSNumber numberWithBool:valueBool];
    
    [KVACustomValue registerWithName:name value:(isValidValue ? value : nil)];
}

// void registerCustomNumberValue(string name, number value)
void iosNativeRegisterCustomNumberValue(const char *nameUtf8, double valueDouble, bool isValidValue) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSNumber *value = [NSNumber numberWithDouble:valueDouble];
    
    [KVACustomValue registerWithName:name value:(isValidValue ? value : nil)];
}

// void registerIdentityLink(string name, string value)
void iosNativeRegisterIdentityLink(const char *nameUtf8, const char *valueUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *value = [KochavaMeasurementPlugin convertCStringToNSString:valueUtf8];

    [KVAIdentityLink registerWithName:name identifier:value];
}

// void enableIosAppClips(string identifier)
void iosNativeEnableIosAppClips(const char *identifierUtf8) {
    NSString *identifier = [KochavaMeasurementPlugin convertCStringToNSString:identifierUtf8];

    KVAAppGroups.shared.generalAppGroupIdentifier = identifier;
}

// void enableIosAtt()
void iosNativeEnableIosAtt() {
    KVAMeasurement.shared.appTrackingTransparency.enabledBool = true;
}

// void setIosAttAuthorizationWaitTime(double waitTime)
void iosNativeSetIosAttAuthorizationWaitTime(double waitTime) {
    KVAMeasurement.shared.appTrackingTransparency.authorizationStatusWaitTimeInterval = waitTime;
}

// void setIosAttAuthorizationAutoRequest(bool autoRequest)
void iosNativeSetIosAttAuthorizationAutoRequest(bool autoRequest) {
    KVAMeasurement.shared.appTrackingTransparency.autoRequestTrackingAuthorizationBool = autoRequest;
}

// void registerPrivacyProfile(string name, string[] keys)
void iosNativeRegisterPrivacyProfile(const char *nameUtf8, const char *keysSerializedUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *keysSerialized = [KochavaMeasurementPlugin convertCStringToNSString:keysSerializedUtf8];
    NSArray *keys = [KochavaMeasurementUtil parseJsonArray:keysSerialized];

    [KVAPrivacyProfile registerWithName:name datapointKeyArray:keys];
}

// void setPrivacyProfileEnabled(string name, bool enabled)
void iosNativeSetPrivacyProfileEnabled(const char *nameUtf8, bool enabled) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];

    [KVAMeasurement.shared.privacy setEnabledBoolForProfileName:name enabledBool:enabled];
}

// Register a deeplink wrapper domain for enhanced deeplink ESP integrations.
void iosNativeRegisterDeeplinkWrapperDomain(const char *domainUtf8) {
    NSString *domain = [KochavaMeasurementPlugin convertCStringToNSString:domainUtf8];

    [KVADeeplink_Wrapper registerWithDomain:domain];
}

// void setInitCompletedListener(bool setListener)
void iosNativeSetInitCompletedListener(bool setListener) {
    if(setListener) {
        KVAMeasurement.shared.config.closure_didComplete = ^(KVANetworking_Config * _Nonnull config) {
            NSDictionary *configDictionary = [KochavaMeasurementUtil convertConfigToDictionary:config];
            NSString *configString = [KochavaMeasurementUtil serializeJsonObject:configDictionary] ?: @"{}";

            // send this message back to the C# layer, which must always have a game object and listener method with these names.
            const char *a = "KochavaMeasurement";
            const char *b = "NativeInitCompletedListener";
            UnitySendMessage(a, b, autonomousStringCopy([configString UTF8String]));
        };
    } else {
        KVAMeasurement.shared.config.closure_didComplete = nil;
    }
}

// void setIntelligentConsentGranted(bool granted)
void iosNativeSetIntelligentConsentGranted(bool granted) {
    KVAMeasurement.shared.privacy.intelligentConsent.grantedBoolNumber = [NSNumber numberWithBool:granted];
}

// bool getStarted()
bool iosNativeGetStarted() {
    return KVAMeasurement.shared.startedBool;
}

// void startWithAppGuid(string appGuid)
void iosNativeStartWithAppGuid(const char *appGuidUtf8) {
    NSString *appGuid = [KochavaMeasurementPlugin convertCStringToNSString:appGuidUtf8];

    // Subscribe to various platform events.
    subscribeToPlatformEvents();

    // Start
    [KVAMeasurement.shared startWithAppGUIDString:appGuid];
}

// void startWithPartnerName(string partnerName)
void iosNativeStartWithPartnerName(const char *partnerNameUtf8) {
    NSString *partnerName = [KochavaMeasurementPlugin convertCStringToNSString:partnerNameUtf8];

    // Subscribe to various platform events.
    subscribeToPlatformEvents();

    // Start
    [KVAMeasurement.shared startWithPartnerNameString:partnerName];
}

// void shutdown(bool deleteData)
void iosNativeShutdown(bool deleteData) {
    [KochavaMeasurement_Product.shared shutdownWithDeleteLocalDataBool:deleteData];
}

// string retrieveInstallId()
char *iosNativeRetrieveInstallId() {
    if (KVAMeasurement.shared.startedBool) {
        return autonomousStringCopy([KVAMeasurement.shared.installIdentifier.string ?: @"" UTF8String]);
    } else {
        return autonomousStringCopy([@"" UTF8String]);
    }
}

// void retrieveInstallAttribution(Callback<InstallAttribution> callback)
void iosNativeRetrieveInstallAttribution(const char *requestIdUtf8) {
    NSString *requestId = [KochavaMeasurementPlugin convertCStringToNSString:requestIdUtf8];

    [KVAMeasurement.shared.attribution retrieveResultWithClosure_didComplete:^(KVAMeasurement_Attribution_Result *attribution) {
        NSDictionary *attributionDictionary = [KochavaMeasurementUtil convertInstallAttributionToDictionary:attribution];
        NSString *attributionString = [KochavaMeasurementUtil serializeJsonObject:attributionDictionary];
        NSString *response = [KochavaMeasurementUtil serializeJsonObject:@{
                @"id": requestId,
                @"value": attributionString
        }] ?: @"";
        // send this message back to the C# layer, which must always have a game object and listener method with these names.
        const char *a = "KochavaMeasurement";
        const char *b = "NativeInstallAttributionListener";
        UnitySendMessage(a, b, autonomousStringCopy([response UTF8String]));
    }];
}

// void processDeeplink(string path, Callback<Deeplink> callback)
void iosNativeProcessDeeplink(const char *pathUtf8, const char *requestIdUtf8) {
    NSURL *path = [KochavaMeasurementUtil parseNSURL:[KochavaMeasurementPlugin convertCStringToNSString:pathUtf8]];
    NSString *requestId = [KochavaMeasurementPlugin convertCStringToNSString:requestIdUtf8];

    [KVADeeplink processWithURL:path closure_didComplete:^(KVADeeplink *_Nonnull deeplink) {
        NSDictionary *deeplinkDictionary = [KochavaMeasurementUtil convertDeeplinkToDictionary:deeplink];
        NSString *deeplinkString = [KochavaMeasurementUtil serializeJsonObject:deeplinkDictionary];
        NSString *response = [KochavaMeasurementUtil serializeJsonObject:@{
                @"id": requestId,
                @"value": deeplinkString
        }] ?: @"";
        // send this message back to the C# layer, which must always have a game object and listener method with these names.
        const char *a = "KochavaMeasurement";
        const char *b = "NativeDeeplinkListener";
        UnitySendMessage(a, b, autonomousStringCopy([response UTF8String]));
    }];
}

// void processDeeplinkWithOverrideTimeout(string path, double timeout, Callback<Deeplink> callback)
void iosNativeProcessDeeplinkWithOverrideTimeout(const char *pathUtf8, double timeout, const char *requestIdUtf8) {
    NSURL *path = [KochavaMeasurementUtil parseNSURL:[KochavaMeasurementPlugin convertCStringToNSString:pathUtf8]];
    NSString *requestId = [KochavaMeasurementPlugin convertCStringToNSString:requestIdUtf8];

    [KVADeeplink processWithURL:path timeoutTimeInterval:timeout closure_didComplete:^(KVADeeplink *_Nonnull deeplink) {
        NSDictionary *deeplinkDictionary = [KochavaMeasurementUtil convertDeeplinkToDictionary:deeplink];
        NSString *deeplinkString = [KochavaMeasurementUtil serializeJsonObject:deeplinkDictionary];
        NSString *response = [KochavaMeasurementUtil serializeJsonObject:@{
                @"id": requestId,
                @"value": deeplinkString
        }] ?: @"";
        // send this message back to the C# layer, which must always have a game object and listener method with these names.
        const char *a = "KochavaMeasurement";
        const char *b = "NativeDeeplinkListener";
        UnitySendMessage(a, b, autonomousStringCopy([response UTF8String]));
    }];
}

// void registerPushToken(string token)
void iosNativeRegisterPushToken(const char *tokenUtf8) {
    NSString *token = [KochavaMeasurementPlugin convertCStringToNSString:tokenUtf8];
    [KVAPushNotificationsToken registerWithDataHexString:token];
}

// void setPushEnabled(bool enabled)
void iosNativeSetPushEnabled(bool enabled) {
    KVAMeasurement.shared.pushNotifications.enabledBool = enabled;
}

// void registerDefaultEventStringParameter(string name, string value)
void iosNativeRegisterDefaultEventStringParameter(const char *nameUtf8, const char *valueUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *value = [KochavaMeasurementPlugin convertCStringToNSString:valueUtf8];
    
    [KVAEvent_DefaultParameter registerWithName:name value:value];
}

// void registerDefaultEventBoolParameter(string name, bool value)
void iosNativeRegisterDefaultEventBoolParameter(const char *nameUtf8, bool valueBool, bool isValidValue) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSNumber *value = [NSNumber numberWithBool:valueBool];
    
    [KVAEvent_DefaultParameter registerWithName:name value:(isValidValue ? value : nil)];
}

// void registerDefaultEventNumberParameter(string name, number value)
void iosNativeRegisterDefaultEventNumberParameter(const char *nameUtf8, double valueDouble, bool isValidValue) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSNumber *value = [NSNumber numberWithDouble:valueDouble];
    
    [KVAEvent_DefaultParameter registerWithName:name value:(isValidValue ? value : nil)];
}

// void registerDefaultEventUserId(string name, string value)
void iosNativeRegisterDefaultEventUserId(const char *valueUtf8) {
    NSString *value = [KochavaMeasurementPlugin convertCStringToNSString:valueUtf8];
    
    [KVAEvent_DefaultParameter registerWithUserIdString:value];
}

// void sendEvent(string name)
void iosNativeSendEvent(const char *nameUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];

    if (name.length > 0) {
        [KVAEvent sendCustomWithEventName:name];
    } else {
        [KochavaMeasurementUtil log:@"Warn: sendEvent invalid input"];
    }
}

// void sendEventWithString(string name, string data)
void iosNativeSendEventWithString(const char *nameUtf8, const char *dataUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *data = [KochavaMeasurementPlugin convertCStringToNSString:dataUtf8];

    if (name.length > 0) {
        [KVAEvent sendCustomWithEventName:name infoString:data];
    } else {
        [KochavaMeasurementUtil log:@"Warn: sendEventWithString invalid input"];
    }
}

// void sendEventWithDictionary(string name, object data)
void iosNativeSendEventWithDictionary(const char *nameUtf8, const char *dataSerializedUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *dataSerialized = [KochavaMeasurementPlugin convertCStringToNSString:dataSerializedUtf8];
    NSDictionary *data = [KochavaMeasurementUtil parseJsonObject:dataSerialized];

    if (name.length > 0) {
        [KVAEvent sendCustomWithEventName:name infoDictionary:data];
    } else {
        [KochavaMeasurementUtil log:@"Warn: sendEventWithDictionary invalid input"];
    }
}

// void sendEventWithEvent(Event event)
void iosNativeSendEventWithEvent(const char *eventSerializedUtf8) {
    NSString *eventSerialized = [KochavaMeasurementPlugin convertCStringToNSString:eventSerializedUtf8];
    NSDictionary *eventInfo = [KochavaMeasurementUtil parseJsonObject:eventSerialized];

    [KochavaMeasurementUtil buildAndSendEvent:eventInfo];
}
