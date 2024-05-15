using Treasure;
using UnityEngine;

public class IdentityUI : MonoBehaviour
{
    public void OnStartUserSessionBtn()
    {
        _ = TDK.Identity.StartUserSession();
    }

    public void OnEndUserSessionBtn()
    {
        TDK.Identity.EndUserSession();
    }
}
