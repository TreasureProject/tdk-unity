using System;
using System.Collections.Generic;
using System.Numerics;
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

    void Start()
    {
        RefreshMetadata();
    }
    
    public async void OnGetPoolDetailsBtn()
    {
        InfoText.text = "Fetching pool details...";
        try
        {
            var magicToTreasuresPoolId = "0x0626699bc82858c16ae557b2eaad03a58cfcc8bd";
            var poolData = await TDK.Magicswap.GetPoolById(magicToTreasuresPoolId);
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
        try
        {
            InfoText.text = "Fetching route...";
            var magicAddress = TDK.Common.GetContractAddress(Contract.Magic);
            var routeData = await TDK.Magicswap.GetRoute(
                tokenInId: magicAddress,
                tokenOutId: "0xd30e91d5cd201d967c908d9e74f6cea9efe35e06", // from pool fetch
                amount: "1",
                isExactOut: true
            );
            InfoText.text = JsonConvert.SerializeObject(routeData, Formatting.Indented);
            magicswapRoute = routeData;
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

    public async void OnApproveMagicBtn() {
        var amount = BigInteger.Parse(Utils.ToWei("1000")); // 1000 MAGIC
        var magicswapRouterAddress = TDK.Common.GetContractAddress(Contract.MagicswapV2Router);
        try
        {
            InfoText.text = $"Approving {Utils.ToEth(amount.ToString())} MAGIC for Magicswap...";
            var transaction = await TDK.Common.ApproveERC20(Contract.Magic, magicswapRouterAddress, amount);
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

    public async void OnApproveTreasuresBtn() {
        var magicswapRouterAddress = TDK.Common.GetContractAddress(Contract.MagicswapV2Router);
        try
        {
            InfoText.text = $"Approving Treasures for Magicswap...";
            var transaction = await TDK.Common.ApproveERC1155(Contract.Treasures, magicswapRouterAddress);
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

    public async void OnApproveLPBtn() {
        if (magicswapPool == null) {
            InfoText.text = "Must get pool details before approving LP!";
            return;
        }
        var callTargets = TDK.AppConfig.GetCallTargets();
        if (!callTargets.Contains(magicswapPool.id)) {
            InfoText.text = "`callTargets` in TDKConfig must include the pool id!";
            InfoText.text += "\nPool id: " + magicswapPool.id;
            return;
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

    public async void OnSwapBtn() {
        if (magicswapRoute == null) {
            InfoText.text = "Must get route before swap!";
            return;
        }
        var bodyJson = "";
        try
        {
            // magicswapRoute.amountIn = (BigInteger.Parse(magicswapRoute.amountIn) + 100000000).ToString();
            var swapBody = new SwapBody {
                tokenInId = magicswapRoute.tokenIn.id,
                tokenOutId = magicswapRoute.tokenOut.id,
                amountIn = magicswapRoute.amountIn,
                nftsOut = new List<NFTInput> {
                    new() {
                        id = magicswapRoute.tokenOut.collectionTokenIds[0],
                        quantity = 1,
                    }
                },
                path = magicswapRoute.path,
                isExactOut = true,
            };
            bodyJson = JsonConvert.SerializeObject(swapBody, Formatting.Indented, new JsonSerializerSettings {
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

    public async void OnAddLiquidityBtn() {
        if (magicswapPool == null) {
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
            var amountA = new BigInteger(1);
            var reserveA = BigInteger.Parse(tokenA.reserve);
            var reserveB = BigInteger.Parse(tokenB.reserve);
            var amountB = TDK.Magicswap.GetQuote(
                amountA.AdjustDecimals(0, tokenA.decimals),
                reserveA,
                reserveB
            );

            var addLiquidityBody = new AddLiquidityBody {
                amount0 = amountB.ToString(),
                amount0Min = TDK.Magicswap.GetAmountMin(amountB, 0.01).ToString(),
                nfts1 = new List<NFTInput>() {
                    new() {
                        id = tokenA.collectionTokenIds[0],
                        quantity = (int)amountA,
                    }
                }
            };
            bodyJson = JsonConvert.SerializeObject(addLiquidityBody, Formatting.Indented, new JsonSerializerSettings {
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

    public async void OnRemoveLiquidityBtn() {
        if (magicswapPool == null) {
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

            var removeLiquidityBody = new RemoveLiquidityBody {
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
            bodyJson = JsonConvert.SerializeObject(removeLiquidityBody, Formatting.Indented, new JsonSerializerSettings {
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

    public async void RefreshMetadata() {
        try
        {
            var walletConnected = await TDK.Connect.IsWalletConnected();
            var enableButtons = walletConnected && TDK.Identity.IsAuthenticated;
            ApproveButton.interactable = enableButtons;
            SwapButton.interactable = enableButtons;
            AddLiquidityButton.interactable = enableButtons;
            ApproveTreasuresButton.interactable = enableButtons;
            ApproveLPButton.interactable = enableButtons;
            RemoveLiquidityButton.interactable = enableButtons;

            if (!walletConnected) {
                MetadataText.text = "Connect Wallet first (Connect tab)";
                return;
            }
            if (!TDK.Identity.IsAuthenticated) {
                MetadataText.text = "Start User Session first (Identity tab)";
                return;
            }
            MetadataText.text = "Loading metadata...";
            
            RefreshMetadataButton.interactable = false;
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            var magicAddress = TDK.Common.GetContractAddress(Contract.Magic);
            var magicswapRouterAddress = TDK.Common.GetContractAddress(Contract.MagicswapV2Router);
            var treasuresAddress = TDK.Common.GetContractAddress(Contract.Treasures);
            var magicContract = await ThirdwebContract.Create(thirdwebService.Client, magicAddress, TDK.Connect.GetChainIdAsInt());
            var treasuresContract = await ThirdwebContract.Create(thirdwebService.Client, treasuresAddress, TDK.Connect.GetChainIdAsInt());
            var magicBalance = await magicContract.ERC20_BalanceOf(TDK.Connect.Address);
            var magicAllowance = await magicContract.ERC20_Allowance(TDK.Connect.Address, magicswapRouterAddress);
            var treasuresAreApproved = await treasuresContract.Read<bool>("isApprovedForAll", TDK.Connect.Address, magicswapRouterAddress);

            var text = $@"<b>- Magic -</b>
Balance: {Utils.ToEth(magicBalance.ToString())}
Allowance: {Utils.ToEth(magicAllowance.ToString())}
<b>- Treasures -</b>
Approved for all: {treasuresAreApproved}";

            if (magicswapRoute != null) {
                if (magicswapRoute.tokenOut.isNFT) {
                    var tokenIds = magicswapRoute.tokenOut.collectionTokenIds;
                    var treasures = new List<(string id, BigInteger balance)>();
                    foreach (var tokenId in tokenIds) {
                        var treasureBalance = await treasuresContract.Read<BigInteger>("balanceOf", TDK.Connect.Address, tokenId);
                        if (treasureBalance > 0) {
                            treasures.Add((tokenId, treasureBalance));
                        }
                    }
                    if (treasures.Count > 0) {
                        text += $"\nOwned: ";
                        text += string.Join("\n", treasures.ConvertAll((t) => $"#{t.id}: {t.balance}"));
                    }
                }
            }

            if (magicswapPool != null) {
                var lpContract = await ThirdwebContract.Create(thirdwebService.Client, magicswapPool.id, TDK.Connect.GetChainIdAsInt());
                var lpBalance = await lpContract.ERC20_BalanceOf(TDK.Connect.Address);
                var lpAllowance = await lpContract.ERC20_Allowance(TDK.Connect.Address, magicswapRouterAddress);
                text += "\n<b>- Pool LP -</b>";
                text += $"\nBalance: {Utils.ToEth(lpBalance.ToString())}";
                text += $"\nAllowance: {Utils.ToEth(lpAllowance.ToString())}";
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
