namespace Treasure
{
    public static class AnalyticsConstants
    {
        // values
        public const int MAX_VOLATILE_EVENTS_CACHE_COUNT = 50;
        public const int MAX_VOLATILE_EVENTS_CACHE_SIZE_KB =  64 * 1024;
        public const int VOLATILE_EVENT_FLUSH_TIME_SECONDS =  10;
        public const string API_ENDPOINT =  "http://api-analytics.treasure.lol";
        public const string CACHE_DIRECTORY_NAME = "Treasure";

        // properties
        public const string PROP_EVENT_NAME = "event_name";
        public const string PROP_EVENT_ID = "event_id";
        public const string PROP_EVENT_PROPERTIES = "event_properties";
    }
}