using UnityEngine;
using WalletConnectUnity.UI;

namespace Treasure
{
    public class TDKWalletConnectWrapper : MonoBehaviour
    {
        [SerializeField] private GameObject walletConnectModalPrefab;

        private void Awake()
        {
            if (TDK.AppConfig.EnableWalletLogin)
            {
                Instantiate(walletConnectModalPrefab);
                
                // Originally the orientation tracker was in the Modal gameObject in the prefab,
                // which caused a warning with DontDestroyOnLoad since its not a top level object.
                // The prefab was edited to remove that component and its being added here at top level instead
                var orientationTrackerHolder = new GameObject("OrientationTracker");
                orientationTrackerHolder.AddComponent<OrientationTracker>();
            }
        }
    }
}
