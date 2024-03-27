using UnityEngine;

namespace Treasure
{
	public class SafeAreaAdjustment : MonoBehaviour
	{

		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private Canvas canvas;
		[Space]
		[SerializeField] private bool adjustTop = true;
		[SerializeField] private bool adjustBottom = false;

		private void Start()
		{
			ApplySafeArea();
		}

		private void Reset()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		private void ApplySafeArea()
		{
			if (rectTransform == null)
				return;
			if (canvas == null)
				canvas = rectTransform.GetComponentInParent<Canvas>();
			var safeArea = Screen.safeArea;

			var anchorMin = safeArea.position;
			var anchorMax = safeArea.position + safeArea.size;
			anchorMin.x /= canvas.pixelRect.width;
			anchorMin.y /= canvas.pixelRect.height;
			anchorMax.x /= canvas.pixelRect.width;
			anchorMax.y /= canvas.pixelRect.height;

			if (adjustBottom)
				rectTransform.anchorMin = anchorMin;
			if (adjustTop)
				rectTransform.anchorMax = anchorMax;
		}
	}
}

