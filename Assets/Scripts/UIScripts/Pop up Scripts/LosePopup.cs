using TMPro;
using System;
using UnityEngine;
using ConnectinnoGames.GameScripts;
using ConnectinnoGames.SoundScripts;

namespace ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts
{
    [Serializable]
    public class LoseDefinition : PopupDefinition
    {
        public LoseDefinition() : base("Lose_Popup")
        {

        }
    }

    public class LosePopup : BasicPopup
    {
        [SerializeField] private TextMeshProUGUI textField;

        private LoseDefinition definition;

        private SoundManager soundManager;

        public override void InitFromDefinition(PopupDefinition givenDefinition)
        {
            base.InitFromDefinition(givenDefinition);

            definition = givenDefinition as LoseDefinition;

            if (definition == null)
                return;

            definition.SetCallbacks(null, Hide);

            soundManager = SoundManager.Instance;

        }

        public void OnReplayButtonClicked()
        {
            soundManager.PlayClick();

            ConnectinnoActions.OnReplayLevel?.Invoke();

            definition.CloseCallback?.Invoke();
        }

        public void OnExtra30SecondsButtonClicked()
        {
            soundManager.PlayClick();

            ConnectinnoActions.OnRemoveCoin?.Invoke(5);

            ConnectinnoActions.OnExtraSeconds?.Invoke();

            definition.CloseCallback?.Invoke();
        }
    }
}