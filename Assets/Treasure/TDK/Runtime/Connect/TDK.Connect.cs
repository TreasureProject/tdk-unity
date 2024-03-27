using UnityEngine;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Connect Connect { get; private set; }

        /// <summary>
        /// Initialize the Connect module
        /// </summary>
        private void InitConnect()
        {
            Connect = new Connect();
        }
    }

    public class Connect
    {
        public Connect() { }

        public void Show()
        {
            TDKConnectUIManager.Instance.Show();
        }
    }
}
