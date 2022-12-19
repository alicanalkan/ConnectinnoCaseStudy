using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using ConnectinnoGames.Scripts.Builder_Scripts.Managers;
using ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts.Interfaces;

namespace ConnectinnoGames.Scripts.UI_Scripts.Pop_up_Scripts
{
    public class BasicPopup : MonoBehaviour ,IPopup
    {
        [SerializeField] protected Image panel;
        protected GameObject gameObjectRef;
        protected PopupDefinition popupDefinition;
    
        public virtual void InitFromDefinition(PopupDefinition definition)
        {
            gameObjectRef = this.gameObject;
            popupDefinition = definition;

            Show();
            
            transform.DOScale(Vector3.one, .5f);
        }
        public void Show()
        {
            if(!this.gameObject.activeSelf)
                gameObject.SetActive(true);
        }
        public void Hide()
        {
            PopupManager.ClosePopupBackground();
            transform.DOScale(Vector3.zero, .25f)
                .OnComplete(Destroy);
            //Destroy();
        }

        protected virtual void OnDisable()
        {
            PopupManager.ClosePopupBackground();
            transform.DOScale(Vector3.zero, .25f)
                .OnComplete(Hide);
            //Hide();
        }

        private void Destroy()
        {
            Destroy(this.gameObject);
        
        }
        public bool IsShowing()
        {
            return this.gameObject.activeInHierarchy;
        }

        /*protected override void RefreshSkin()
        {
            //panel.color = Skin.WindowColor;
        }*/
    }
}
