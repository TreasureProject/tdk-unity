using Newtonsoft.Json;
using TMPro;
using Treasure;
using UnityEngine;

public class MagicswapUI : MonoBehaviour
{
    public TMP_Text InfoText;
    
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
            InfoText.text = "Error:\n" + ex.Message;
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
        }
        catch (System.Exception ex)
        {
            InfoText.text = "Error:\n" + ex.Message;
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
            InfoText.text = "Error:\n" + ex.Message;
            throw;
        }
    }
}
