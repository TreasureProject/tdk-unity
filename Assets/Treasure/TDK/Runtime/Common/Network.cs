namespace Treasure
{
    public enum ChainId : int
    {
        Arbitrum = 42161,
        ArbitrumSepolia = 421614
    }

    public static class ChainData
    {
        public static class Arbitrum
        {
            public static ChainId id = ChainId.Arbitrum;
            public static string blockExplorerUrl = "https://sepolia.arbiscan.io";
        }

        public static class ArbitrumSepolia
        {
            public static ChainId id = ChainId.ArbitrumSepolia;
            public static string blockExplorerUrl = "https://arbiscan.io";
        }
    }
}
