using System;

namespace ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts
{
    public class PopupDefinition
    {
        public string PrefabName;

        public Action ConfirmCallback;
        public Action CloseCallback;

        public static PopupDefinition Create(string prefabName)
        {
            return new PopupDefinition(prefabName);
        }
        protected PopupDefinition(string prefabName)
        {
            PrefabName = prefabName;
        }

        public void SetCallbacks(Action confirmCallback, Action closeCallback = null)
        {
            ConfirmCallback = confirmCallback;
            CloseCallback = closeCallback;
        }
    }
}