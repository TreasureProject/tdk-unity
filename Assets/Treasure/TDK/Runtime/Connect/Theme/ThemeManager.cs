using UnityEngine;

namespace Treasure
{
    public enum Theme
    {
        Light, Dark
    }

    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance;

        [SerializeField] private ThemeData themeData;
        [SerializeField] private Theme theme = Theme.Light;

        public Theme Theme => theme;
        public static ThemeData ThemeData => Instance.themeData;

        public delegate void ThemeChanged(Theme theme);
        public static event ThemeChanged OnThemeChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public void ChangeTheme()
        {
            theme = (theme == Theme.Light) ? Theme.Dark : Theme.Light;
            OnThemeChanged?.Invoke(theme);
        }
    }
}
