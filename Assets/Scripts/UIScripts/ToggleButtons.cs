using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace ConnectinnoGames.UIScripts
{
    public class ToggleButtons : MonoBehaviour
    {
        private float maxX;

        [SerializeField] private RectTransform toggleSwitchField;
        [SerializeField] private Image fillAmount;
        [SerializeField] private RectTransform swithchButtonRect;
        private void Awake()
        {
            maxX = toggleSwitchField.sizeDelta.x /2;
        }

        /// <summary>
        /// Handle Togle Buton Pressed
        /// </summary>
        /// <param name="value">Changed Value</param>
        public void OnToggleValueChanged(bool value)
        {
            if (value)
            {
                // Update Background fill
                fillAmount.DOFillAmount(1, 0.25f);
                // Update Image Possition
                swithchButtonRect.DOAnchorPosX(maxX, 0.25f);
            }
            else
            {
                // Update Background fill
                fillAmount.DOFillAmount(0, 0.25f);
                // Update Image Possition
                swithchButtonRect.DOAnchorPosX(-maxX, 0.25f);
            }
        }
    }

}
