using TMPro;
using System;
using UnityEngine;
using ConnectinnoGames.SoundScripts;

namespace ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts
{
    [Serializable]
    public class ChestDefinition : PopupDefinition
    {
        public Action OpenChest;

        public ChestDefinition(Action openChest) : base("Chest_Popup")
        {
            OpenChest = openChest;
        }
    }

    public class ChestPopup : BasicPopup
    {
        [SerializeField] private TextMeshProUGUI textField;

        private ChestDefinition definition;
        private SoundManager soundManager;
        public override void InitFromDefinition(PopupDefinition givenDefinition)
        {
            base.InitFromDefinition(givenDefinition);

            soundManager = SoundManager.Instance;

            definition = givenDefinition as ChestDefinition;

            if (definition == null)
                return;

            definition.SetCallbacks(null, Hide);
        }

        public void OnOpenChestClicket()
        {
            soundManager.PlayClick();

            definition.CloseCallback?.Invoke();

            definition.OpenChest?.Invoke();
        }

    }
}