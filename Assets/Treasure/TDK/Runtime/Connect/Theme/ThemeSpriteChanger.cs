using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class ThemeSpriteChanger : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite lightSprite;
        [SerializeField] private Sprite darkSprite;

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged += OnThemeChanged;

            OnThemeChanged(ThemeManager.Instance.Theme);
        }

        private void OnDisable()
        {
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(Theme theme)
        {
            image.sprite = theme == Theme.Light ? lightSprite : darkSprite;
        }
    }
}
