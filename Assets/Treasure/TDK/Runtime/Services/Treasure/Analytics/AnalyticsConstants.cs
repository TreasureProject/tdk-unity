namespace Treasure
{
    public static class AnalyticsConstants
    {
        // event cache values
        public const int MAX_CACHE_EVENT_COUNT = 10;
        public const int MAX_CACHE_SIZE_KB = 64;
        public const int CACHE_FLUSH_TIME_SECONDS = 10;
        
        // persistent store values
        public const string PERSISTENT_DIRECTORY_NAME = "AnalyticsStore";
        public const int PERSISTENT_CHECK_INTERVAL_SECONDS = 60;
        public const int PERSISTENT_MAX_RETRIES = 5;

        // properties
        public const string CARTRIDGE_TAG = "cartridge_tag";
        public const string PROP_NAME = "name";
        public const string PROP_ID = "id";
        public const string PROP_VERSION = "version";
        public const string PROP_TIME_LOCAL = "time_local";
        public const string PROP_TIME_SERVER = "time_server";
        public const string PROP_PROPERTIES = "properties";
        public const string PROP_DEVICE = "device";
        public const string PROP_DEVICE_NAME = "device_name";
        public const string PROP_DEVICE_MODEL = "device_model";
        public const string PROP_DEVICE_TYPE = "device_type";
        public const string PROP_DEVICE_UNIQUE_ID = "device_unique_id";
        public const string PROP_DEVICE_OS = "device_os";
        public const string PROP_DEVICE_OS_FAMILY = "device_os_family";
        public const string PROP_DEVICE_CPU = "device_cpu";
        public const string PROP_APP = "app";
        public const string PROP_APP_IDENTIFIER = "app_identifier";
        public const string PROP_APP_IS_EDITOR = "app_is_editor";
        public const string PROP_APP_VERSION = "app_version";
        public const string PROP_APP_ENVIRONMENT = "app_environment";
        public const string PROP_SMART_ACCOUNT = "smart_account";
        public const string PROP_CHAIN_ID = "chain_id";
        public const string PROP_TYPE = "type";
        public const string PROP_SESSION_ID = "session_id";

        // events
        public const string EVT_APP_START = "app_start";
        public const string EVT_CONNECT_BTN = "connect_btn";
        public const string EVT_CONNECT_CONNECTED = "connect_connected";
        public const string EVT_CONNECT_DISCONNECTED = "connect_disconnected";
    }
}