using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Audio
{
    public class ButtonAudioPlayer : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Inspector

        [SerializeField] [Unity.Collections.ReadOnly] private Button button;
        [SerializeField] private AudioType clickAudioType = AudioType.UIClick;
        [SerializeField] private AudioType clickErrorAudioType = AudioType.UIError;

        [SerializeField] private bool pointerDown = true;
        [SerializeField] private bool pointerUp;
        [SerializeField] private bool pointerClick;
        
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            SetupComponents();
        }

        #endregion
        
        private void Awake()
        {
            SetupComponents();
        }

        private void SetupComponents()
        {
            if (button == null && !TryGetComponent(out button))
            {
                Debug.LogError($"{name} {nameof(button)} is missing.");
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerDown)
            {
                AudioManager.Instance.PlaySFX(button.interactable ? clickAudioType : clickErrorAudioType);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerUp)
            {
                throw new System.NotImplementedException();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (pointerClick)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}