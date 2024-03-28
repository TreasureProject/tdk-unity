using System.Collections;
using UnityEngine;

namespace Treasure
{
    public class ModalBase : MonoBehaviour
    {
        [SerializeField] private RectTransform modalRect;
        [SerializeField] private float animeTime = 0.3f;

        private void OnEnable()
        {
            Show();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            if (modalRect == null)
                modalRect = GetComponent<RectTransform>();

            modalRect.anchoredPosition = new Vector2(0, -modalRect.sizeDelta.y);
            StartCoroutine(AnimateRect(Vector2.zero));
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        IEnumerator AnimateRect(Vector2 target)
        {
            float time = 0;
            var startPos = modalRect.anchoredPosition;
            while (time < animeTime)
            {
                modalRect.anchoredPosition = Vector2.Lerp(startPos, target, time / animeTime);
                yield return null;
                time += Time.deltaTime;
            }

            modalRect.anchoredPosition = target;
        }
    }
}
