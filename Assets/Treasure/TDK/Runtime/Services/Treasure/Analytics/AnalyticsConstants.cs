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
        public const int PERSISTENT_MAX_RETRIES = 5;

        // properties
        public const string CARTRIDGE_TAG = "cartridge_tag";
        public const string PROP_NAME = "name";
        public const string PROP_ID = "id";
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
        public const string PROP_TDK_FLAVOUR = "tdk_flavour";
        public const string PROP_TDK_VERSION = "tdk_version";
        public const string PROP_APP_ENVIRONMENT = "app_environment";
        public const string PROP_SMART_ACCOUNT = "smart_account";
        public const string PROP_CHAIN_ID = "chain_id";
        public const string PROP_TYPE = "type";
        public const string PROP_SESSION_ID = "session_id";
        public const string PROP_PERMIT_ADDRESS = "permits_address";
        public const string PROP_PERMIT_TOKEN_ID = "permits_token_id";
        public const string PROP_AMOUNT = "amount";
        public const string PROP_CHARACTERS_ADDRESS = "characters_address";
        public const string PROP_TOKEN_IDS = "token_ids";
        public const string PROP_CONTRACT_ADDRESS = "contract_address";
        public const string PROP_ARGS = "args";
        public const string PROP_REQUEST_IDS= "request_ids";
        public const string PROP_ENGINE_TX = "engin_tx";

        // events
        public const string EVT_APP_START = "app_start";
        public const string EVT_CONNECT_BTN = "connect_btn";
        public const string EVT_TREASURECONNECT_CONNECTED = "tc_connected";
        public const string EVT_TREASURECONNECT_DISCONNECTED = "tc_disconnected";
        public const string EVT_TREASURECONNECT_OTP_FAILED = "tc_otpfailed";
        public const string EVT_TREASURECONNECT_UI_LOGIN = "tc_ui_login";
        public const string EVT_TREASURECONNECT_UI_CONFIRM = "tc_ui_confirm";
        public const string EVT_TREASURECONNECT_UI_ACCOUNT = "tc_ui_account";

        // events - bridgeworld
        public const string EVT_BRIDGEWORLD_NFTS_STAKE = "bworld_nfts_stake";
        public const string EVT_BRIDGEWORLD_NFTS_UNSTAKE = "bworld_nfts_unstake";
        public const string EVT_BRIDGEWORLD_NFT_STAKE_BATCH = "bworld_nfts_stake_batch";
        public const string EVT_BRIDGEWORLD_NFT_UNSTAKE_BATCH = "bworld_nfts_unstake_batch";
        public const string EVT_BRIDGEWORLD_DEPOSIT = "bworld_deposit";
        public const string EVT_BRIDGEWORLD_WITHDRAW_MAGIC = "bworld_withdraw_magic";
        public const string EVT_BRIDGEWORLD_WITHDRAW_MAGIC_ALL = "bworld_withdraw_magic_all";
        public const string EVT_BRIDGEWORLD_REWARDS_CLAIM_MAGIC = "bworld_rewards_claim_magic";
        public const string EVT_BRIDGEWORLD_CORRUPTION_REMOVAL_START = "bworld_corruption_removal_start";
        public const string EVT_BRIDGEWORLD_CORRUPTION_REMOVAL_END = "bworld_corruption_removal_end";
    }
}