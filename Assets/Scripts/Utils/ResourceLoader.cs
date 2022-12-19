using UnityEngine;

namespace Bastardhaus.Scripts.Utils
{
    public static class ResourceLoader
    {
        /// <summary>
        /// Loads pop-up GameObject according to given prefab name.
        /// </summary>
        /// <param name="prefabName">Given GameObject prefab name</param>
        /// <returns></returns>
        public static GameObject LoadPopup(string prefabName)
        {
            var targetPopup = Resources.Load("Popups/" + prefabName) as GameObject;
            return targetPopup;
        }
        
        /// <summary>
        /// Loads Pop-up Canvas.
        /// </summary>
        /// <returns></returns>
        public static GameObject LoadPopupCanvas()
        {
            var popupCanvas = Resources.Load("PopupCanvas") as GameObject;
            return popupCanvas;
        }


        /// <summary>
        /// Loads Raycast Blocker For Popups
        /// </summary>
        /// <returns></returns>
        public static GameObject LoadRaycastBlocker()
        {
            var raycastBlocker = Resources.Load<GameObject>("UI/RaycastBlocker");
            return raycastBlocker;
        }
    }
}