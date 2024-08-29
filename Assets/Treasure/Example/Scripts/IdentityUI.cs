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
            // TODO improve error handling generally:
            // - doing `Debug.LogError(e)` gives a better error trace
            // - not catching at all makes the stack trace "file + line links" clickable
            // - could try TDKLogger.LogException here
            TDKLogger.LogError($"Error starting user session: {e.Message}");
        }
    }

    public void OnEndUserSessionBtn()
    {
        _ = TDK.Identity.EndUserSession();
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
