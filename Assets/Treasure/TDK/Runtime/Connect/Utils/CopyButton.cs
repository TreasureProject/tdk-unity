using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
    public class CopyButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [Space]
        [SerializeField] private Sprite copyIcon;
        [SerializeField] private Sprite checkIcon;

        private string _textToCopy = "";

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        public void SetTextToCopy(string text)
        {
            _textToCopy = text;
        }

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                ClipboardHelper.Copy(_textToCopy);
                icon.sprite = checkIcon;
            });
        }

        private void OnEnable()
        {
            icon.sprite = copyIcon;
        }
    }
}
