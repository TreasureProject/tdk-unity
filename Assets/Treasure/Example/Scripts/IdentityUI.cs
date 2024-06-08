using System;
using Treasure;
using UnityEngine;

public class IdentityUI : MonoBehaviour
{
    [SerializeField] private InputPopUp promptDialogPrefab;

    public async void OnStartUserSessionBtn()
    {
        try
        {
            _ = await TDK.Identity.StartUserSession();
        }
        catch (Exception e)
        {
            TDKLogger.LogError($"Error starting user session: {e.Message}");
        }
    }

    public void OnEndUserSessionBtn()
    {
        TDK.Identity.EndUserSession();
    }

    public void OnValidateUserSessionBtn()
    {
        Instantiate(promptDialogPrefab, transform.GetComponentInParent<Canvas>().transform)
            .Show("Validate User Session", "Enter an auth token below to validate:", OnAuthTokenSubmit);
    }

    public async void OnAuthTokenSubmit(string value)
    {
        try
        {
            var chainId = await TDK.Connect.GetChainId();
            await TDK.Identity.ValidateUserSession(chainId, value);
        }
        catch (Exception e)
        {
            TDKLogger.LogError($"Error validating user session: {e.Message}");
        }
    }
}
