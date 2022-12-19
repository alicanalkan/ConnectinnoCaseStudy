using System.Collections.Generic;
using Bastardhaus.Scripts.Utils;
using ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts;
using ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ConnectinnoGames.Scripts.Builder_Scripts.Managers
{
    public class PopupManager : MonoBehaviour
    {
        private static readonly List<GameObject> popupList = new List<GameObject>();

        private static Transform popupTargetCanvas;

        private static GameObject raycastBlocker;

        private static Image popupCanvasBackground;
        
        private void Awake()
        {
            var popupCanvasObject = ResourceLoader.LoadPopupCanvas();
            popupTargetCanvas = Instantiate(popupCanvasObject).transform;
            popupCanvasBackground = popupTargetCanvas.GetComponent<Image>();

            var raycastBlockerObject = ResourceLoader.LoadRaycastBlocker();
            raycastBlocker = Instantiate(raycastBlockerObject);

            raycastBlocker.SetActive(false);

            DontDestroyOnLoad(popupTargetCanvas);
            DontDestroyOnLoad(raycastBlocker);
            DontDestroyOnLoad(gameObject);
        }
    
        private static void CloseAll()
        {
            for (var i = 0; i < popupList.Count; i++)
            {
                var popup = popupList[i];
                popupList.RemoveAt(i);
                Destroy(popup);
            }
        }

        public static void ShowPopup(PopupDefinition popupDefinition)
        {
            CloseAll();

            raycastBlocker.SetActive(true);

            popupCanvasBackground.enabled = true;
            popupCanvasBackground.raycastTarget = true;
        
            var loadedPopup = ResourceLoader.LoadPopup(popupDefinition.PrefabName);
            var popupGameObject = Instantiate(loadedPopup, popupTargetCanvas);
            
            var iPopup = popupGameObject.GetComponent<IPopup>();
        
            iPopup.InitFromDefinition(popupDefinition);

            popupList.Add(popupGameObject);
        }

        public static void ClosePopupBackground()
        {
            raycastBlocker.SetActive(false);
            popupCanvasBackground.raycastTarget = false;
            popupCanvasBackground.enabled = false;
        }
    }
}