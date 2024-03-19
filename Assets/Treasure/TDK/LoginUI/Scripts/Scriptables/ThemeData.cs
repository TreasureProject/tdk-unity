using System.Collections.Generic;
using UnityEngine;

namespace Treasure
{
    public enum UiColorType
    {
        modalBg, buttonBgColor, buttonBorderColor, buttonIconColor, headerText, infoText, iconBgColor,
        inputFieldBgColor, inputFieldBorderColor, inputFieldTextColor,
        toggleColor, labelBgColor, labelTextColor
    }

    [System.Serializable]
    public class ThemeColor
    {
        public UiColorType type;
        public Color lightColor;
        public Color darkColor;
    }

    [CreateAssetMenu(fileName = "ThemeData", menuName = "ScriptableObjects/ThemeData")]
    public class ThemeData : ScriptableObject
    {
        [SerializeField] private List<ThemeColor> data = new List<ThemeColor>();

        public ThemeColor GetThemeColor(UiColorType type)
        {
            return data.Find(x => x.type == type);
        }
    }
}
