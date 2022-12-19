namespace ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts.Interfaces
{ 
    internal interface IPopup
    {
        void Show();
        void Hide();
        void InitFromDefinition(PopupDefinition definition);
    }
}