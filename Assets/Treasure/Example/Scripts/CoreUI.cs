using System.Collections.Generic;
using UnityEngine;

namespace Treasure
{
    public class CoreUI : MonoBehaviour
    {
        [SerializeField] private InputPopUp inputPopUpPrefab;
        [SerializeField] private DropDownPopUp dropDownPopUpPrefab;

        public void ShowInputPopUp()
        {
            var inputPopup = Instantiate(inputPopUpPrefab, transform.GetComponentInParent<Canvas>().transform);
            inputPopup.Show(
                "Input pop up"
                , "Hello please input your data in text filed"
                , OnInputPopupSubmit);
        }

        public void ShowDropDownPopUp()
        {
            var dropDown = Instantiate(dropDownPopUpPrefab, transform.GetComponentInParent<Canvas>().transform);
            List<string> dropDownOptions = new List<string> { "Option A", "Option B", "Option C", "Option D", "Option E" };
            dropDown.Show(
                "DropDown pop up"
                , "Hello please select your option from drop down menu"
                , OnDropdownPopupSubmit
                , dropDownOptions);
        }

        private void OnInputPopupSubmit(string data)
        {
            Debug.Log(data);
        }

        private void OnDropdownPopupSubmit(int data)
        {
            Debug.Log(data);
        }
    }
}
