using UnityEngine;

namespace Treasure
{
    public class UIRotateConstant : MonoBehaviour
    {
        [SerializeField] RectTransform rectTransform;
        [SerializeField] float speed = 10;

        private void Update()
        {
            rectTransform.rotation *= Quaternion.Euler(0, 0, speed * Time.deltaTime);
        }
    }
}
