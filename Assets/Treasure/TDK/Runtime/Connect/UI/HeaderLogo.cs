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
        [SerializeField] private TMP_Text nameText;

        private void Start()
        {
            iconImage.sprite = appSettingsData.icon;
            nameText.text = appSettingsData.title;
        }
    }
}
