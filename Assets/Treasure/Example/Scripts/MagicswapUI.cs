using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using Thirdweb;
using TMPro;
using Treasure;
using UnityEngine;
using UnityEngine.UI;

// TODO show magic balance
// TODO show approved amount
// TODO show treasures balance
// TODO nft-nft test?
// TODO nft-token test?
public class MagicswapUI : MonoBehaviour
{
    public TMP_Text InfoText;
    public Button swapButton;

    MagicswapRoute magicswapRoute;
    
    public async void OnGetPoolDetailsBtn()
    {
        InfoText.text = "Fetching pool details...";
        try
        {
            var poolData = await TDK.Magicswap.GetPoolById("0x0626699bc82858c16ae557b2eaad03a58cfcc8bd");
            InfoText.text = JsonConvert.SerializeObject(poolData, Formatting.Indented);
        }
        catch (System.Exception ex)
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
            var routeData = await TDK.Magicswap.GetRoute(
                tokenInId: "0x55d0cf68a1afe0932aff6f36c87efa703508191c",
                tokenOutId: "0xd30e91d5cd201d967c908d9e74f6cea9efe35e06",
                amount: "1",
                isExactOut: true
            );
            InfoText.text = JsonConvert.SerializeObject(routeData, Formatting.Indented);
            magicswapRoute = routeData;
            swapButton.interactable = true;
        }
        catch (System.Exception ex)
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
        catch (System.Exception ex)
        {
            InfoText.text = "Error: " + ex.Message;
            throw;
        }
    }

    public async void OnApproveBtn() {
        var amount = BigInteger.Parse("1000000000000000000000"); // 1000 MAGIC
        var magicswapRouterAddress = Constants.ContractAddresses[await TDK.Connect.GetChainId()][Treasure.Contract.MagicswapRouter];
        try
        {
            InfoText.text = $"Approving {Utils.ToEth(amount.ToString())} MAGIC for Magicswap...";
            var transaction = await TDK.Common.ApproveERC20(Treasure.Contract.Magic, magicswapRouterAddress, amount);
            var responseJson = JsonConvert.SerializeObject(transaction, Formatting.Indented);
            InfoText.text = $"Response: {responseJson}";
        }
        catch (System.Exception ex)
        {
            InfoText.text = $"Error: {ex.Message}";
            throw;
        }
    }

    public async void OnSwapBtn() {
        var bodyJson = "";
        try
        {
            var swapBody = new SwapBody {
                tokenInId = magicswapRoute.tokenIn.id,
                tokenOutId = magicswapRoute.tokenOut.id,
                amountIn = (BigInteger.Parse(magicswapRoute.amountIn) + 1).ToString(), // TODO is this expected?
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
        }
        catch (System.Exception ex)
        {
            InfoText.text = $"Error: {ex.Message}\nRequest body: {bodyJson}";
            throw;
        }
    }
}
