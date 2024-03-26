using UnityEngine;

namespace Treasure
{
    [CreateAssetMenu(fileName = "AppSettingsData", menuName = "ScriptableObjects/AppSettingsData")]
    public class AppSettingsData : ScriptableObject
    {
        public Sprite icon;
        public string title;
        [Space]
        public LoginSettings loginSettings;
        [Header("Confirmation settings")]
        public bool hasCodeToConfirmEmail = true;
    }
}
