using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class HeaderLogo : MonoBehaviour
    {
        [SerializeField] private AppSettingsData appSettingsData;
        [Space]
        [SerializeField] private Image iconImage;
        [SerializeField] private AspectRatioFitter iconAspectRatioFitter;
        [SerializeField] private LayoutElement logoLayoutElement;
        [SerializeField] private TMP_Text nameText;

        private void Start()
        {
            iconImage.sprite = appSettingsData.icon;
            nameText.text = appSettingsData.title;

            var aspect = appSettingsData.icon.texture.width / (float) appSettingsData.icon.texture.height;
            iconAspectRatioFitter.aspectRatio = aspect;
            logoLayoutElement.preferredWidth = Mathf.Clamp(aspect * logoLayoutElement.preferredHeight, 
                logoLayoutElement.preferredHeight, logoLayoutElement.preferredHeight * 2);
        }
    }
}
