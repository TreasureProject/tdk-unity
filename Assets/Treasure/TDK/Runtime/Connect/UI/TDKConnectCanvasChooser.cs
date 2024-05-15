using UnityEngine;

namespace Treasure
{
    public class TDKConnectCanvasChooser : MonoBehaviour
    {
        [SerializeField] private GameObject portraitCanvas;
        [SerializeField] private GameObject lanscapeCanvas;

        private void Awake()
        {
            var deviceOriantation = DetectDeviceOrientation();
            switch (deviceOriantation)
            {
                case DeviceOrientation.Portrait:
                    Instantiate(portraitCanvas);
                    break;
                case DeviceOrientation.LandscapeLeft:
                    Instantiate(lanscapeCanvas);
                    break;
                default:
                    break;
            }
        }

        private DeviceOrientation DetectDeviceOrientation()
        {
            var _resolution = new Vector2(Screen.width, Screen.height);
            if (_resolution.y > _resolution.x)
            {
                return DeviceOrientation.Portrait;
            }
            else
            {
                return DeviceOrientation.LandscapeLeft;
            }
        }
    }
}
