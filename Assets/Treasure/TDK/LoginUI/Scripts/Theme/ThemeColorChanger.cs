using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class ThemeColorChanger : MonoBehaviour
    {
        [SerializeField] private Graphic graphic;
        [SerializeField] private UiColorType type;

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
            var themeColor = ThemeManager.ThemeData.GetThemeColor(type);
            graphic.color = theme == Theme.Light ? themeColor.lightColor : themeColor.darkColor;
        }
    }
}
