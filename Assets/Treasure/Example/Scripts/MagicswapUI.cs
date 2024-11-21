using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thirdweb;
using TMPro;
using Treasure;
using UnityEngine;
using UnityEngine.UI;

public class MagicswapUI : MonoBehaviour
{
    public TMP_Text InfoText;
    public TMP_Text MetadataText;
    public Button ApproveButton;
    public Button ApproveTreasuresButton;
    public Button ApproveLPButton;
    public Button SwapButton;
    public Button AddLiquidityButton;
    public Button RemoveLiquidityButton;
    public Button RefreshMetadataButton;

    MagicswapRoute magicswapRoute;
    MagicswapPool magicswapPool;
    bool isMagicswapRouteInverted;

    class MagicswapUITestValues {
        public string poolId;
        public string tokenNameA;
        public string tokenNameB;
        public string tokenIdA;
        public string tokenIdB;
        public bool isNftA;
        public bool isNftB;
        public bool isEthA = false;
        public bool isEthB = false;
        public string nftForApprovalA;
        public string nftForApprovalB;
        public string amount;
        public bool isExactOutAToB;
        public bool isExactOutBToA;
    }

    Dictionary<ChainId, MagicswapUITestValues> uiTestValuesMap = new Dictionary<ChainId, MagicswapUITestValues>
    {
        {
            ChainId.ArbitrumSepolia, new MagicswapUITestValues
            {
                poolId = "0x0626699bc82858c16ae557b2eaad03a58cfcc8bd", // magic to treasures pool
                tokenNameA = "Magic",
                tokenNameB = "Treasures",
                tokenIdA = "0x55d0cf68a1afe0932aff6f36c87efa703508191c", // magic
                tokenIdB = "0xd30e91d5cd201d967c908d9e74f6cea9efe35e06", // from pool fetch
                isNftA = false,
                isNftB = true,
                amount = "1",
                isExactOutAToB = true, // `amount` is the amount out (output = 1 nft)
                isExactOutBToA = false, // `amount` is the amount in (input = 1 nft)
                nftForApprovalB = "0xfe592736200d7545981397ca7a8e896ac0c166d4", // treasures
            }
        },
        {
            ChainId.TreasureTopaz, new MagicswapUITestValues
            {
                poolId = "0x9c61210b8c8ea450bd3fdbd7a7c1208206d18b7b", // usdc to wmagic pool
                tokenNameA = "Usdc",
                tokenNameB = "Magic",
                tokenIdA = "0x99b9ed17bb37768bb1a3cb6d91b15834eb7c2185", // usdc
                tokenIdB = "0x095ded714d42cbd5fb2e84a0ffbfb140e38dc9e1", // wmagic
                isNftA = false,
                isNftB = false,
                isEthB = true,
                amount = "5",
                isExactOutAToB = false, // `amount` is the amount out
                isExactOutBToA = false, // `amount` is the amount out
            }
        }
    };

    MagicswapUITestValues UITestValues => uiTestValuesMap[TDK.Connect.ChainId];

    void Start()
    {
        RefreshMetadata();
    }

    public async void OnGetPoolDetailsBtn()
    {
        InfoText.text = "Fetching pool details...";
        try
        {
            var poolId = UITestValues.poolId;
            var poolData = await TDK.Magicswap.GetPoolById(poolId);
            InfoText.text = JsonConvert.SerializeObject(poolData, Formatting.Indented);
            magicswapPool = poolData;
        }
        catch (Exception ex)
        {
            InfoText.text = "Error: " + ex.Message;
            throw;
        }
    }

    public async void OnGetRouteBtn()
    {
        await GetRoute(invertRoute: false);
    }

    public async void OnGetInvertedRouteBtn()
    {
        await GetRoute(invertRoute: true);
    }

    private async Task GetRoute(bool invertRoute)
    {
        try
        {
            InfoText.text = "Fetching route...";
            var routeData = await TDK.Magicswap.GetRoute(
                tokenInId: invertRoute ? UITestValues.tokenIdB : UITestValues.tokenIdA,
                tokenOutId: invertRoute ? UITestValues.tokenIdA : UITestValues.tokenIdB,
                amount: UITestValues.amount,
                isExactOut: invertRoute ? UITestValues.isExactOutBToA : UITestValues.isExactOutAToB
            );
            InfoText.text = JsonConvert.SerializeObject(routeData, Formatting.Indented);
            magicswapRoute = routeData;
            isMagicswapRouteInverted = invertRoute;
        }
        catch (Exception ex)
        {
            InfoText.text = "Error: " + ex.Message;
            throw;
        }
    }

    public async void OnGetAllPoolsBtn()
    {
        try
        {
            InfoText.text = "Fetching all pools...";
            var poolsData = await TDK.Magicswap.GetAllPools();
            InfoText.text = JsonConvert.SerializeObject(poolsData, Formatting.Indented);
        }
        catch (Exception ex)
        {
            InfoText.text = "Error: " + ex.Message;
            throw;
        }
    }

    public async void OnApproveTokenABtn() {
        if (UITestValues.isNftA)
        {
            await OnApproveERC1155(UITestValues.nftForApprovalA, UITestValues.tokenNameA);
        }
        else if (UITestValues.isEthA)
        {
            InfoText.text = "Approval of native token not needed";
        }
        else
        {
            await OnApproveERC20(UITestValues.tokenIdA, UITestValues.tokenNameA);
        }
    }

    public async void OnApproveTokenBBtn() {
        if (UITestValues.isNftB)
        {
            await OnApproveERC1155(UITestValues.nftForApprovalB, UITestValues.tokenNameB);
        }
        else if (UITestValues.isEthB)
        {
            InfoText.text = "Approval of native token not needed";
        }
        else
        {
            await OnApproveERC20(UITestValues.tokenIdB, UITestValues.tokenNameB);
        }
    }

    public async Task OnApproveERC20(string tokenId, string tokenName)
    {
        var amount = BigInteger.Parse(Utils.ToWei("1000")); // approve 1000 units
        var magicswapRouterAddress = TDK.Common.GetContractAddress(Contract.MagicswapV2Router);
        try
        {
            InfoText.text = $"Approving {Utils.ToEth(amount.ToString())} {tokenName} for Magicswap...";
            var transaction = await TDK.Common.ApproveERC20(tokenId, magicswapRouterAddress, amount);
            var responseJson = JsonConvert.SerializeObject(transaction, Formatting.Indented);
            InfoText.text = $"Response: {responseJson}";
            RefreshMetadata();
        }
        catch (Exception ex)
        {
            InfoText.text = $"Error: {ex.Message}";
            throw;
        }
    }

    public async Task OnApproveERC1155(string tokenId, string tokenName)
    {
        var magicswapRouterAddress = TDK.Common.GetContractAddress(Contract.MagicswapV2Router);
        try
        {
            InfoText.text = $"Approving {tokenName} for Magicswap...";
            var transaction = await TDK.Common.ApproveERC1155(tokenId, magicswapRouterAddress);
            var responseJson = JsonConvert.SerializeObject(transaction, Formatting.Indented);
            InfoText.text = $"Response: {responseJson}";
            RefreshMetadata();
        }
        catch (Exception ex)
        {
            InfoText.text = $"Error: {ex.Message}";
            throw;
        }
    }

    public async void OnApproveLPBtn()
    {
        if (magicswapPool == null)
        {
            InfoText.text = "Must get pool details before approving LP!";
            return;
        }
        var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
        var usingZkSyncChain = await thirdwebService.IsZkSyncChain(TDK.Connect.ChainIdNumber);
        if (!usingZkSyncChain)
        {
            var callTargets = TDK.AppConfig.GetCallTargets();
            if (!callTargets.Contains(magicswapPool.id))
            {
                InfoText.text = "`callTargets` in TDKConfig must include the pool id!";
                InfoText.text += "\nPool id: " + magicswapPool.id;
                return;
            }
        }
        var amount = BigInteger.Parse(Utils.ToWei("20"));
        var magicswapRouterAddress = TDK.Common.GetContractAddress(Contract.MagicswapV2Router);
        try
        {
            InfoText.text = $"Approving {Utils.ToEth(amount.ToString())} LP for Magicswap...";
            var transaction = await TDK.Common.ApproveERC20(magicswapPool.id, magicswapRouterAddress, amount);
            var responseJson = JsonConvert.SerializeObject(transaction, Formatting.Indented);
            InfoText.text = $"Response: {responseJson}";
            RefreshMetadata();
        }
        catch (Exception ex)
        {
            InfoText.text = $"Error: {ex.Message}";
            throw;
        }
    }

    public async void OnSwapBtn()
    {
        if (magicswapRoute == null)
        {
            InfoText.text = "Must get route before swap!";
            return;
        }
        var bodyJson = "";
        try
        {
            // magicswapRoute.amountIn = (BigInteger.Parse(magicswapRoute.amountIn) + 100000000).ToString();
            var swapBody = new SwapBody
            {
                tokenInId = magicswapRoute.tokenIn.id,
                tokenOutId = magicswapRoute.tokenOut.id,
                amountIn = magicswapRoute.tokenIn.isNFT ? null : magicswapRoute.amountIn,
                amountOut = magicswapRoute.tokenOut.isNFT ? null : magicswapRoute.amountOut,
                nftsIn = !magicswapRoute.tokenIn.isNFT ? null : new List<NFTInput> {
                    new() {
                        id = magicswapRoute.tokenIn.collectionTokenIds[0],
                        quantity = (int) BigInteger.Parse(magicswapRoute.amountIn).AdjustDecimals(18, 0)
                    }
                },
                nftsOut = !magicswapRoute.tokenOut.isNFT ? null : new List<NFTInput> {
                    new() {
                        id = magicswapRoute.tokenOut.collectionTokenIds[0],
                        quantity = (int) BigInteger.Parse(magicswapRoute.amountOut).AdjustDecimals(18, 0)
                    }
                },
                path = magicswapRoute.path,
                isExactOut = isMagicswapRouteInverted ? UITestValues.isExactOutBToA : UITestValues.isExactOutAToB,
            };
            bodyJson = JsonConvert.SerializeObject(swapBody, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            InfoText.text = $"Performing swap...\nRequest body: {bodyJson}";
            var transaction = await TDK.Magicswap.Swap(swapBody);
            var responseJson = JsonConvert.SerializeObject(transaction, Formatting.Indented);
            InfoText.text = $"Response: {responseJson}\nRequest body: {bodyJson}";
            RefreshMetadata();
        }
        catch (Exception ex)
        {
            InfoText.text = $"Error: {ex.Message}\nRequest body: {bodyJson}";
            throw;
        }
    }

    public async void OnAddLiquidityBtn()
    {
        if (magicswapPool == null)
        {
            InfoText.text = "Must get pool details before adding liquidity!";
            return;
        }
        var bodyJson = "";
        try
        {
            // for tokenA we should know the amount we want to add (amountA = 1 in this case)
            // for tokenB we can calculate amountB based on amountA and the pool reserves
            var tokenA = magicswapPool.token1;
            var tokenB = magicswapPool.token0;
            var amountA = new BigInteger(1).AdjustDecimals(0, tokenA.decimals); // TODO make this configurable?
            var reserveA = BigInteger.Parse(tokenA.reserve);
            var reserveB = BigInteger.Parse(tokenB.reserve);
            var amountB = TDK.Magicswap.GetQuote(
                amountA,
                reserveA,
                reserveB
            );

            string amount0 = null;
            string amount0Min = null;
            List<NFTInput> nfts0 = null;
            if (tokenB.isNFT)
            {
                nfts0 = new List<NFTInput>() {
                    new() {
                        id = tokenB.collectionTokenIds[0],
                        quantity = (int) amountB.AdjustDecimals(tokenB.decimals, 0),
                    }
                };
            }
            else
            {
                amount0 = amountB.ToString();
                amount0Min = TDK.Magicswap.GetAmountMin(amountB, 0.01).ToString();
            }
            
            string amount1 = null;
            string amount1Min = null;
            List<NFTInput> nfts1 = null;
            if (tokenA.isNFT)
            {
                nfts1 = new List<NFTInput>() {
                    new() {
                        id = tokenA.collectionTokenIds[0],
                        quantity = (int) amountB.AdjustDecimals(tokenB.decimals, 0),
                    }
                };
            }
            else
            {
                amount1 = amountA.ToString();
                amount1Min = TDK.Magicswap.GetAmountMin(amountA, 0.01).ToString();
            }

            var addLiquidityBody = new AddLiquidityBody
            {
                amount0 = amount0,
                amount0Min = amount0Min,
                amount1 = amount1,
                amount1Min = amount1Min,
                nfts0 = nfts0,
                nfts1 = nfts1,
            };
            bodyJson = JsonConvert.SerializeObject(addLiquidityBody, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            InfoText.text = $"Adding liquidity...\nRequest body: {bodyJson}";
            var transaction = await TDK.Magicswap.AddLiquidity(magicswapPool.id, addLiquidityBody);
            var responseJson = JsonConvert.SerializeObject(transaction, Formatting.Indented);
            InfoText.text = $"Response: {responseJson}\nRequest body: {bodyJson}";
            RefreshMetadata();
        }
        catch (Exception ex)
        {
            InfoText.text = $"Error: {ex.Message}\nRequest body: {bodyJson}";
            throw;
        }
    }

    // TODO make it work with erc20-erc20
    public async void OnRemoveLiquidityBtn()
    {
        if (magicswapPool == null)
        {
            InfoText.text = "Must get pool details before removing liquidity!";
            return;
        }
        var bodyJson = "";
        try
        {
            var reserve0 = BigInteger.Parse(magicswapPool.token0.reserve);
            var reserve1 = BigInteger.Parse(magicswapPool.token1.reserve);
            var totalSupply = BigInteger.Parse(magicswapPool.totalSupply);

            var treasuresDesired = new BigInteger(1).AdjustDecimals(0, magicswapPool.token1.decimals);
            // calculate LP amount to get the desired amount of treasures (1).
            // LP amount could otherwise be inputted by the user.
            var amountLPWei = BigInteger.DivRem(treasuresDesired * totalSupply, reserve1, out BigInteger reminder);
            if (reminder > 0) amountLPWei += 1;

            var amount0 = amountLPWei * reserve0 / totalSupply;
            var amount1 = amountLPWei * reserve1 / totalSupply; // should be 1 treasure from our LP calculation

            // floor amount1 since token1 is nft
            var amount1FlooredNoDecimals = amount1.AdjustDecimals(magicswapPool.token1.decimals, 0);

            // use helper for amount0 since its ERC20
            var amount0Min = TDK.Magicswap.GetAmountMin(amount0, 0.01);
            // dont use helper for amount1 since its NFT, use floored value with decimals instead
            var amount1Min = amount1FlooredNoDecimals.AdjustDecimals(0, magicswapPool.token1.decimals);

            var removeLiquidityBody = new RemoveLiquidityBody
            {
                amountLP = amountLPWei.ToString(),
                amount0Min = amount0Min.ToString(),
                amount1Min = amount1Min.ToString(),
                nfts1 = new List<NFTInput>() {
                    new() {
                        id = magicswapPool.token1.collectionTokenIds[0],
                        quantity = (int)amount1FlooredNoDecimals,
                    }
                }
            };
            bodyJson = JsonConvert.SerializeObject(removeLiquidityBody, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            bodyJson += $"\nRequired LP for 1 treasure: {Utils.ToEth(amountLPWei.ToString())}";
            InfoText.text = $"Removing liquidity...\nRequest body: {bodyJson}";
            var transaction = await TDK.Magicswap.RemoveLiquidity(magicswapPool.id, removeLiquidityBody);
            var responseJson = JsonConvert.SerializeObject(transaction, Formatting.Indented);
            InfoText.text = $"Response: {responseJson}\nRequest body: {bodyJson}";
            RefreshMetadata();
        }
        catch (Exception ex)
        {
            InfoText.text = $"Error: {ex.Message}\nRequest body: {bodyJson}";
            throw;
        }
    }

    public async void RefreshMetadata()
    {
        try
        {
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            var usingZkSyncChain = await thirdwebService.IsZkSyncChain(TDK.Connect.ChainIdNumber);
            
            var walletConnected = await TDK.Connect.IsWalletConnected() || TDK.Identity.IsUsingTreasureLauncher;
            var enableButtons = walletConnected && (TDK.Identity.IsAuthenticated || usingZkSyncChain);
            ApproveButton.interactable = enableButtons;
            SwapButton.interactable = enableButtons;
            AddLiquidityButton.interactable = enableButtons;
            ApproveTreasuresButton.interactable = enableButtons;
            ApproveLPButton.interactable = enableButtons;
            RemoveLiquidityButton.interactable = enableButtons;

            if (!walletConnected)
            {
                MetadataText.text = "Connect Wallet first (Connect tab)";
                return;
            }
            if (!TDK.Identity.IsAuthenticated && !usingZkSyncChain)
            {
                MetadataText.text = "Start User Session first (Identity tab)";
                return;
            }
            MetadataText.text = "Loading metadata...";

            RefreshMetadataButton.interactable = false;
            
            var text = $"<b>- {UITestValues.tokenNameA} (A) -</b>\n";
            if (UITestValues.isNftA)
            {
                var isApproved = await TDK.Magicswap.IsERC1155Approved(UITestValues.nftForApprovalA, TDK.Identity.Address);
                text += $"Approved: {isApproved}\n";
            }
            else if (UITestValues.isEthA)
            {
                var balance = await TDK.Common.GetFormattedNativeBalance();
                text += $"Balance: {balance}\n";
            }
            else
            {
                var balance = await TDK.Common.GetFormattedERC20Balance(UITestValues.tokenIdA, TDK.Identity.Address, 18);
                var allowance = await TDK.Magicswap.GetERC20Allowance(UITestValues.tokenIdA, TDK.Identity.Address);
                text += $"Balance: {balance}\n";
                text += $"Allowance: {Utils.ToEth(allowance.ToString())}\n";
            }
            text += $"<b>- {UITestValues.tokenNameB} (B) -</b>\n";
            if (UITestValues.isNftB)
            {
                var isApproved = await TDK.Magicswap.IsERC1155Approved(UITestValues.nftForApprovalB, TDK.Identity.Address);
                text += $"Approved: {isApproved}\n";
            }
            else if (UITestValues.isEthB)
            {
                var balance = await TDK.Common.GetFormattedNativeBalance();
                text += $"Balance: {balance}\n";
            }
            else
            {
                var balance = await TDK.Common.GetFormattedERC20Balance(UITestValues.tokenIdB, TDK.Identity.Address, 18);
                var allowance = await TDK.Magicswap.GetERC20Allowance(UITestValues.tokenIdB, TDK.Identity.Address);
                text += $"Balance: {balance}\n";
                text += $"Allowance: {Utils.ToEth(allowance.ToString())}\n";
            }

            // TODO show nfts regardless of route direction
            if (magicswapRoute != null && magicswapRoute.tokenOut.isNFT)
            {
                var nftsContractB = await TDK.Common.GetContract(UITestValues.nftForApprovalB);
                var tokenIds = magicswapRoute.tokenOut.collectionTokenIds;
                var nfts = new List<(string id, BigInteger balance)>();
                foreach (var tokenId in tokenIds)
                {
                    var nftBalance = await nftsContractB.ERC1155_BalanceOf(TDK.Identity.Address, BigInteger.Parse(tokenId));
                    if (nftBalance > 0)
                    {
                        nfts.Add((tokenId, nftBalance));
                    }
                }
                if (nfts.Count > 0)
                {
                    text += $"<b>Owned ({UITestValues.tokenNameB}):</b>\n";
                    text += string.Join("\n", nfts.ConvertAll((t) => $"#{t.id}: {t.balance}")) + "\n";
                }
            }

            if (magicswapPool != null)
            {
                var lpBalanceTask = TDK.Common.GetFormattedERC20Balance(magicswapPool.id, TDK.Identity.Address, 18);
                var lpAllowanceTask = TDK.Magicswap.GetERC20Allowance(magicswapPool.id, TDK.Identity.Address);
                await Task.WhenAll(lpBalanceTask, lpAllowanceTask);
                text += "<b>- Pool LP -</b>\n";
                text += $"Balance: {lpBalanceTask.Result}\n";
                text += $"Allowance: {Utils.ToEth(lpAllowanceTask.Result.ToString())}\n";
            }

            MetadataText.text = text;
        }
        catch (Exception ex)
        {
            MetadataText.text = "Error: " + ex.Message;
            throw;
        }
        finally
        {
            RefreshMetadataButton.interactable = true;
        }

    }
}
