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
            var token0 = magicswapPool.token0;
            var token1 = magicswapPool.token1;
            var reserve0 = BigInteger.Parse(token0.reserve);
            var reserve1 = BigInteger.Parse(token1.reserve);
            
            // for one token we should know the amount we want to add (amount1 = 1 in this case)
            var amount1 = new BigInteger(1).AdjustDecimals(0, token1.decimals);
            // for the other token we calculate it based on the previous amount and the pool reserves
            var amount0 = TDK.Magicswap.GetQuote(
                amount1,
                reserve1,
                reserve0
            );

            string amount0Body = null;
            string amount0MinBody = null;
            List<NFTInput> nfts0Body = null;
            if (token0.isNFT)
            {
                nfts0Body = new List<NFTInput>() {
                    new() {
                        id = token0.collectionTokenIds[0],
                        quantity = (int) amount0.AdjustDecimals(token0.decimals, 0),
                    }
                };
            }
            else
            {
                amount0Body = amount0.ToString();
                amount0MinBody = TDK.Magicswap.GetAmountMin(amount0, 0.01).ToString();
            }
            
            string amount1Body = null;
            string amount1MinBody = null;
            List<NFTInput> nfts1Body = null;
            if (token1.isNFT)
            {
                nfts1Body = new List<NFTInput>() {
                    new() {
                        id = token1.collectionTokenIds[0],
                        quantity = (int) amount1.AdjustDecimals(token1.decimals, 0),
                    }
                };
            }
            else
            {
                amount1Body = amount1.ToString();
                amount1MinBody = TDK.Magicswap.GetAmountMin(amount1, 0.01).ToString();
            }

            var addLiquidityBody = new AddLiquidityBody
            {
                amount0 = amount0Body,
                amount0Min = amount0MinBody,
                amount1 = amount1Body,
                amount1Min = amount1MinBody,
                nfts0 = nfts0Body,
                nfts1 = nfts1Body,
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
            var token0 = magicswapPool.token0;
            var token1 = magicswapPool.token1;
            var totalSupply = BigInteger.Parse(magicswapPool.totalSupply);
            var reserve0 = BigInteger.Parse(token0.reserve);
            var reserve1 = BigInteger.Parse(token1.reserve);

            BigInteger amountLPWei;
            MagicswapToken desiredNftToken = null;
            if (token0.isNFT)
            {
                desiredNftToken = token0;
            }
            if (token1.isNFT)
            {
                // higher reserve = higher yield. in nft-nft, ensure lower yield token is used to
                // calculate the lp amount to avoid the case where you get 0.9 nfts which it rounds to 0
                var token0YieldsMore = reserve0 > reserve1; 
                if (desiredNftToken == null || token0YieldsMore)
                {
                    desiredNftToken = token1;
                }
            }
            if (desiredNftToken != null)
            {
                // calculate LP amount to get the desired amount of nfts (1 in this example).
                var nftsDesired = new BigInteger(1).AdjustDecimals(0, desiredNftToken.decimals);
                amountLPWei = BigInteger.DivRem(
                    nftsDesired * totalSupply,
                    BigInteger.Parse(desiredNftToken.reserve),
                    out BigInteger reminder
                );
                if (reminder > 0) amountLPWei += 1;
            }
            else
            {
                // LP amount could otherwise be inputted by the user.
                amountLPWei = BigInteger.Parse(Utils.ToWei("2.5"));
            }

            var amount0 = amountLPWei * reserve0 / totalSupply;
            var amount1 = amountLPWei * reserve1 / totalSupply;

            BigInteger amount0Min;
            BigInteger amount1Min;
            List<NFTInput> nfts0 = null;
            List<NFTInput> nfts1 = null;

            if (token0.isNFT)
            {
                var amount0FlooredNoDecimals = amount0.AdjustDecimals(token0.decimals, 0);
                amount0Min = amount0FlooredNoDecimals.AdjustDecimals(0, token0.decimals);
                nfts0 = new List<NFTInput>() {
                    new() {
                        id = token0.collectionTokenIds[0],
                        quantity = (int) amount0FlooredNoDecimals,
                    }
                };
            }
            else
            {
                amount0Min = TDK.Magicswap.GetAmountMin(amount0, 0.01);
            }

            if (token1.isNFT)
            {
                var amount1FlooredNoDecimals = amount1.AdjustDecimals(token1.decimals, 0);
                amount1Min = amount1FlooredNoDecimals.AdjustDecimals(0, token1.decimals);
                nfts1 = new List<NFTInput>() {
                    new() {
                        id = token1.collectionTokenIds[0],
                        quantity = (int) amount1FlooredNoDecimals,
                    }
                };
            }
            else
            {
                amount1Min = TDK.Magicswap.GetAmountMin(amount1, 0.01);
            }
            
            var removeLiquidityBody = new RemoveLiquidityBody
            {
                amountLP = amountLPWei.ToString(),
                amount0Min = amount0Min.ToString(),
                amount1Min = amount1Min.ToString(),
                nfts0 = nfts0,
                nfts1 = nfts1,
            };
            bodyJson = JsonConvert.SerializeObject(removeLiquidityBody, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            bodyJson += $"\nRequired LP: {Utils.ToEth(amountLPWei.ToString())}";
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
