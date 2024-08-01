using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Treasure
{
    public class HeaderLogo : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private AspectRatioFitter iconAspectRatioFitter;
        [SerializeField] private LayoutElement logoLayoutElement;
        [SerializeField] private TMP_Text nameText;

        private void Start()
        {
            nameText.text = TDK.Instance.AppSettingsData.title;
            ApplySprite(TDK.Instance.AppSettingsData.icon);
        }

        private void ApplySprite(Sprite sprite)
        {
            iconImage.sprite = sprite;
            var aspect = sprite.texture.width / (float)sprite.texture.height;
            iconAspectRatioFitter.aspectRatio = aspect;
            logoLayoutElement.preferredWidth = Mathf.Clamp(aspect * logoLayoutElement.preferredHeight,
                logoLayoutElement.preferredHeight, logoLayoutElement.preferredHeight * 2);
        }

        public string GetCurrentNameText() {
            return nameText.text;
        }
    }
}
