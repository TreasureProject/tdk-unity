using UnityEngine;
using Treasure;
using System.Collections.Generic;

public class PayUI : MonoBehaviour
{
    public void OnGetQuoteBtn()
    {
        Debug.Log("OnGetQuoteBtn");

        TDK.Pay.GetFiatQuote("USD", ChainId.Arbitrum, "0.0025", true);
    }

    public void OnBuyBtn()
    {
        Debug.Log("OnBuyBtn");

        TDK.Pay.Buy();
    }

    public void OnGetStatusBtn()
    {
        Debug.Log("OnGetStatusBtn");

        TDK.Pay.GetStatus();
    }

    public void OnGetHistoryBtn()
    {
        Debug.Log("OnGetQuoteBtnCrypto");

        TDK.Pay.GetHistory();
    }

    public void OnGetSupportedCurrencies()
    {
        Debug.Log("OnGetQuoteBtnCrypto");

        TDK.Pay.GetSupportedCurrencies();
    }
}
