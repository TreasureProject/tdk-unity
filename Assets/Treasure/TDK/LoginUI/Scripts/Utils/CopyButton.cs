using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class CopyButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text texttoCopy;
        [Space]
        [SerializeField] private Sprite copyIcon;
        [SerializeField] private Sprite checkIcon;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                GUIUtility.systemCopyBuffer = texttoCopy.text;
                icon.sprite = checkIcon;
            });
        }

        private void OnEnable()
        {
            icon.sprite = copyIcon;
        }
    }
}
