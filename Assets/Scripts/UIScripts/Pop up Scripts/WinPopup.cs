using TMPro;
using System;
using UnityEngine;
using ConnectinnoGames.SoundScripts;

namespace ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts
{
    [Serializable]
    public class WinDefinition : PopupDefinition
    {
        public Action OnNextLevel;

        public WinDefinition(Action onNextLevel) : base("Win_Popup")
        {
            OnNextLevel = onNextLevel;
        }
    }

    public class WinPopup : BasicPopup
    {
        [SerializeField] private TextMeshProUGUI textField;
        
        private WinDefinition definition;
        
        public override void InitFromDefinition(PopupDefinition givenDefinition)
        {
            base.InitFromDefinition(givenDefinition);
            
            definition = givenDefinition as WinDefinition;

            if (definition == null)
                return;

            definition.SetCallbacks(null, Hide);
        }

        public void OnNextLevelClicked()
        {
            SoundManager.Instance.PlayClick();

            definition.OnNextLevel?.Invoke();

            definition.CloseCallback?.Invoke();
        }

        public async void OnMainMenuClicked()
        {
            SoundManager.Instance.PlayClick();

            await SceneLoadManager.LoadScene("MainMenu");

            definition.CloseCallback?.Invoke();
        }
    }
}