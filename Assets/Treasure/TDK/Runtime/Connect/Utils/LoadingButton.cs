using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure
{
	[RequireComponent(typeof(Button))]
	public class LoadingButton : MonoBehaviour
	{
		[SerializeField] private Button button;
		[SerializeField] private Transform icon;
		[SerializeField] private GameObject spinnerPrefab;
		[SerializeField] private bool isLoadOnClick;
		[SerializeField] private bool isChangeTextColor = true;
		[SerializeField] private TextMeshProUGUI text;
		[SerializeField] private Image iconImage;

		private GameObject _spinner;
		private Color _initialTextColor;
		private bool _isIconEnabledInitially;
		private bool _isInitialized;

		public Button Button => button;

		private void Awake()
		{
			_initialTextColor = text.color;
			_isIconEnabledInitially = icon.gameObject.activeSelf;

			Initialize();

			Button.onClick.AddListener(HandleButtonClick);
		}

		private void Initialize()
		{
			if (_isInitialized) return;
			_spinner = Instantiate(spinnerPrefab, icon);
			_spinner.SetActive(false);
			_isInitialized = true;
		}

		private void HandleButtonClick()
		{
			if (isLoadOnClick) SetLoading(true);
		}

		public void SetLoading(bool loading, bool handleButtonIntractable = true)
		{
			Initialize();
			if (handleButtonIntractable)
				Button.interactable = !loading;

			iconImage.enabled = !loading && _isIconEnabledInitially;
			icon.gameObject.SetActive(loading || _isIconEnabledInitially);

			_spinner.SetActive(loading);

			if (isChangeTextColor) text.color = loading ? Button.colors.disabledColor : _initialTextColor;
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (!Button) button = GetComponent<Button>();
			if (!text) text = GetComponentInChildren<TextMeshProUGUI>();
			if (!iconImage && icon) iconImage = icon.GetComponent<Image>();
		}
#endif
	}
}

