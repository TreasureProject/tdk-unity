using UnityEngine;
using Treasure;
using System.Collections.Generic;

public class PayUI : MonoBehaviour
{
    public void OnGetQuoteBtn()
    {
        Debug.Log("OnGetQuoteBtn");

        TDK.Pay.GetFiatQuote("USD", ChainId.TreasureRuby, 100, true);
    }
}
