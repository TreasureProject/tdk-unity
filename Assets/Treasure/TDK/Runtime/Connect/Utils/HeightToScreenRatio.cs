using UnityEngine;

namespace Treasure
{
    public class HeightToScreenRatio : MonoBehaviour
    {
        [SerializeField] private RectTransform targetRect;
        [SerializeField] [Range(0f,1f)] private float ratio = 0.3f;

        private void OnEnable()
        {
            var resolution = new Vector2(Screen.width, Screen.height);
            targetRect.sizeDelta = new Vector2(targetRect.sizeDelta.x, resolution.y * ratio);
        }
    }
}
