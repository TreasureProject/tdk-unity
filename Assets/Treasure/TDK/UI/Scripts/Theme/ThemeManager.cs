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

        private Theme theme = Theme.Light;
        public Theme Theme => theme;

        public delegate void ThemeChanged(Theme theme);
        public static event ThemeChanged OnThemeChanged;
        public static ThemeData ThemeData => Instance.themeData;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
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
