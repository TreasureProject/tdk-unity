using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Treasure
{
    public class HeaderLogo : MonoBehaviour
    {
        [SerializeField] private AppSettingsData appSettingsData;
        [Space]
        [SerializeField] private Image iconImage;
        [SerializeField] private AspectRatioFitter iconAspectRatioFitter;
        [SerializeField] private LayoutElement logoLayoutElement;
        [SerializeField] private TMP_Text nameText;

        [Space]
        public bool UseCache = false;

        private async void Start()
        {           
            var project = await TDK.Identity.GetProject();

            StartCoroutine(DownloadImage(project.icon));
            nameText.text = project.name;
        }

        private void ApplySprite(Sprite sprite)
        {
            iconImage.sprite = sprite;
            var aspect = sprite.texture.width / (float)sprite.texture.height;
            iconAspectRatioFitter.aspectRatio = aspect;
            logoLayoutElement.preferredWidth = Mathf.Clamp(aspect * logoLayoutElement.preferredHeight,
                logoLayoutElement.preferredHeight, logoLayoutElement.preferredHeight * 2);
        }

        protected IEnumerator DownloadImage(string url)
        {
            if (string.IsNullOrEmpty(url))
                yield break;

            var fileName = Path.GetFileName(url);
            if (UseCache && PlayerPrefs.GetInt(fileName, 0) == 1)
            {
                string filePath = Path.Combine(Application.persistentDataPath, fileName);
                Debug.Log(filePath);
                if (File.Exists(filePath))
                {
                    byte[] textureBytes = File.ReadAllBytes(filePath);
                    Texture2D loadedTexture = new Texture2D(2, 2);
                    loadedTexture.LoadImage(textureBytes);

                    var sprite = Sprite.Create(loadedTexture, new Rect(0.0f, 0.0f, loadedTexture.width, loadedTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    ApplySprite(sprite);
                    yield break;
                } 
            }

            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error Downloading Image");
                }
                else
                {
                    var tex = DownloadHandlerTexture.GetContent(www);

                    if (UseCache)
                    {
                        var name = Path.GetFileName(url);
                        SaveTexture(tex, name);
                    }

                    if (tex == null)
                    {
                        Debug.LogError("Error Downloading Image");
                    }
                    else
                    {
                        var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        ApplySprite(sprite);
                    }
                }
            }
        }

        private void SaveTexture(Texture2D textureToSave, string fileName)
        {           
            byte[] textureBytes = textureToSave.EncodeToPNG();
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(filePath, textureBytes);
            PlayerPrefs.SetInt(fileName, 1);
        }
    }
}
