using System.Collections;
using UnityEngine;

namespace Treasure
{
    public class CanvasGroupAlphaAnim : MonoBehaviour
    {
        [SerializeField] private float timeAll = 0.25f;
        [SerializeField] private float delayWait = 0;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Reset()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            FadeIn();
        }

        public void FadeIn()
        {
            StartCoroutine(AlphaFade(0f, 1f));
        }

        public void FadeOut(bool disappear = false)
        {
            if (disappear)
            {
                StartCoroutine(AlphaFadeDisappear(1f, 0f));
            }
            else
            {
                StartCoroutine(AlphaFade(1f, 0f));
            }
        }

        public CanvasGroup ReturnCanvasGroup()
        {
            return canvasGroup;
        }

        IEnumerator AlphaFade(float from, float to)
        {
            float time = 0f;
            canvasGroup.alpha = from;
            yield return new WaitForSecondsRealtime(delayWait);
            canvasGroup.interactable = true;
            while (time <= timeAll)
            {
                time += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, time / timeAll);
                yield return null;
            }
            canvasGroup.alpha = to;
        }

        IEnumerator AlphaFadeDisappear(float from, float to)
        {
            float time = 0f;
            while (time <= timeAll)
            {
                time += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, time / timeAll);
                yield return null;
            }
            canvasGroup.alpha = to;
            gameObject.SetActive(false);
        }
    }
}
