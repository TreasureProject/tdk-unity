using Treasure;
using UnityEngine;

public class IdentityUI : MonoBehaviour
{
    [SerializeField] private InputPopUp promptDialogPrefab;

    public void OnStartUserSessionBtn()
    {
        _ = TDK.Identity.StartUserSession();
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
        var chainId = await TDK.Connect.GetChainId();
        await TDK.Identity.ValidateUserSession(chainId, value);
    }
}
