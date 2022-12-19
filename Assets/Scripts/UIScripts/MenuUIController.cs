using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ConnectinnoGames.Managers;
using ConnectinnoGames.GameScripts;
using ConnectinnoGames.Scripts.Builder_Scripts.Managers;
using ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts;
using ConnectinnoGames.SoundScripts;

namespace ConnectinnoGames.UIScripts
{
    public class MenuUIController : MonoBehaviour
    {
        [SerializeField] Toggle musicToggle;
        [SerializeField] Toggle soundToggle;
        [SerializeField] Toggle hapticToggle;
        [SerializeField] Image chestImage;
        [SerializeField] TextMeshProUGUI levelText;

        private GameManager gameManager;
        private ConnectinnoGameData gameData;
        private GameObject particle;
        private void Start()
        {
            gameManager = GameManager.Instance;
            gameData = gameManager.GetGameData();

            /// <summary>
            /// Handle Saved Settings
            /// </summary>
            musicToggle.isOn = SettingsManager.GetMusicSettings();
            soundToggle.isOn = SettingsManager.GetSoundSettings();
            hapticToggle.isOn = SettingsManager.GetHapticSettings();
            /// <summary>
            /// Handle settings interface Actions
            /// </summary>
            musicToggle.onValueChanged.AddListener(Managers.SettingsManager.SetMusicSettings);
            soundToggle.onValueChanged.AddListener(Managers.SettingsManager.SetSoundSettings);
            hapticToggle.onValueChanged.AddListener(Managers.SettingsManager.SetHapticSettings);

            // Update Level UI
            levelText.text = $"Level {gameData.level}";

            // CheckChestState
            CheckChestState();
        }

        private void OnDisable()
        { 
            ///Removes Toggles listeners
            musicToggle.onValueChanged.RemoveListener(Managers.SettingsManager.SetMusicSettings);
            soundToggle.onValueChanged.RemoveListener(Managers.SettingsManager.SetSoundSettings);
            hapticToggle.onValueChanged.RemoveListener(Managers.SettingsManager.SetHapticSettings);
        }

        private void CheckChestState()
        {
            if (gameData.openableChests.Count > 0)
            {
                chestImage.sprite = Resources.Load<Sprite>("UI/ChestOpen");
                if(particle == null)
                {
                    particle = Instantiate(Resources.Load<GameObject>("Particle/ChestParticle"));
                    particle.transform.position = new Vector2(chestImage.transform.position.x, chestImage.transform.position.y);
                }

            }
            else
            {
                if(particle != null)
                {
                    chestImage.sprite = Resources.Load<Sprite>("UI/ChestClosed");
                    Destroy(particle);
                }
            }
        }

        public void OnChestButtonClicked()
        {
            SoundManager.Instance.PlayClick();
            if (gameData.openableChests.Count > 0)
            {
                PopupManager.ShowPopup(new ChestDefinition(OpenChest));
            }
        }
        
        public void OpenChest()
        {
            gameData.coinAmount = gameData.coinAmount + 15;
            gameData.openableChests.RemoveAt(gameData.openableChests.Count -1);
            gameManager.SaveData();
            CheckChestState();
        }

        public void OnPlayButtonClicked()
        {
            SoundManager.Instance.PlayClick();
            GameManager.Instance.StartGame(); 
        }
    }
}

