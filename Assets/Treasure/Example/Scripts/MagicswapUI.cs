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
    public Button SwapButton;
    public Button RefreshMetadataButton;

    MagicswapRoute magicswapRoute;

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
            var magicAddress = await TDK.Common.GetContractAddress(Treasure.Contract.Magic);
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

    public async void OnApproveBtn() {
        var amount = BigInteger.Parse("1000000000000000000000"); // 1000 MAGIC
        var magicswapRouterAddress = await TDK.Common.GetContractAddress(Treasure.Contract.MagicswapV2Router);
        try
        {
            InfoText.text = $"Approving {Utils.ToEth(amount.ToString())} MAGIC for Magicswap...";
            var transaction = await TDK.Common.ApproveERC20(Treasure.Contract.Magic, magicswapRouterAddress, amount);
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
                nftsOut = new List<SwapBody.NFTInput> {
                    new() {
                        id = magicswapRoute.tokenOut.collectionTokenIds[0],
                        quantity = 1,
                    }
                },
                path = magicswapRoute.path,
                isExactOut = false,
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

    public async void RefreshMetadata() {
        try
        {
            var walletConnected = await TDK.Connect.IsWalletConnected();
            ApproveButton.interactable = walletConnected && TDK.Identity.IsAuthenticated;
            SwapButton.interactable = walletConnected && TDK.Identity.IsAuthenticated;
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
            var magicAddress = await TDK.Common.GetContractAddress(Treasure.Contract.Magic);
            var magicswapRouterAddress = await TDK.Common.GetContractAddress(Treasure.Contract.MagicswapV2Router);
            var treasuresAddress = await TDK.Common.GetContractAddress(Treasure.Contract.Treasures);
            var magicContract = TDKServiceLocator.GetService<TDKThirdwebService>().SDK.GetContract(magicAddress);
            var treasuresContract = TDKServiceLocator.GetService<TDKThirdwebService>().SDK.GetContract(treasuresAddress);
            var balance = await magicContract.ERC20.Balance();
            var allowance = await magicContract.ERC20.Allowance(magicswapRouterAddress);
            var text = $@"Magic balance:
{Utils.ToEth(balance.value)}
Router allowance:
{Utils.ToEth(allowance.value)}";

            if (magicswapRoute != null) {
                if (magicswapRoute.tokenOut.isNFT) {
                    var tokenIds = magicswapRoute.tokenOut.collectionTokenIds;
                    var treasures = new List<(string id, BigInteger balance)>();
                    foreach (var tokenId in tokenIds) {
                        var treasureBalance = await treasuresContract.ERC1155.Balance(tokenId);
                        if (treasureBalance > 0) {
                            treasures.Add((tokenId, treasureBalance));
                        }
                    }
                    if (treasures.Count > 0) {
                        text += $"\n{magicswapRoute.tokenOut.name}:\n";
                        text += string.Join("\n", treasures.ConvertAll((t) => $"#{t.id}: {t.balance}"));
                    }
                }
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
