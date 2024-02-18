namespace Treasure
{
    public static class AnalyticsConstants
    {
        public const string API_ENDPOINT =  "http://api-analytics.treasure.lol";

        // event cache values
        public const int MAX_CACHE_EVENT_COUNT = 50;
        public const int MAX_CACHE_SIZE_KB =  64 * 1024;
        public const int CACHE_FLUSH_TIME_SECONDS =  10;
        
        // persistent store values
        public const string PERSISTENT_DIRECTORY_NAME = "Treasure";
        public const float PERSISTENT_CHECK_INTERVAL_SECONDS = 30f;
        public const int PERSISTENT_MAX_RETRIES = 5;

        // properties
        public const string PROP_EVENT_NAME = "event_name";
        public const string PROP_EVENT_ID = "event_id";
        public const string PROP_EVENT_PROPERTIES = "event_properties";
    }
}