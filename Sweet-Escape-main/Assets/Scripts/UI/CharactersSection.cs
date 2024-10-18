using Configs;
using DG.Tweening;
using Enums;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace UI
{
    public class CharactersSection : UIScreen
    {
        [SerializeField] private Transform characters;
        [SerializeField] private Transform characterList;
        [SerializeField] private SkinSelection skinSelection;
        [SerializeField] private UpgradeScene upgradeScene;
        [SerializeField] private Image backgroundColor;

        private Vector3 _initialCharactersPos;
        private Vector3 _initialCharacterListPos;

        private bool _hasAnimationDone;
        
        private Tweener _characterTween;
        private Tweener _characterListTween;
        
        private const float CharacterAnimationDuration = 0.5f;
        private const float CharacterListAnimationDuration = 0.5f;

        public SkinSelection SkinSelection => skinSelection;
        public UpgradeScene UpgradeScene => upgradeScene;

        protected override void Awake()
        {
            base.Awake();
            _initialCharactersPos = characters.localPosition;
            _initialCharacterListPos = characterList.localPosition;
        }

        private void OnEnable()
        {
            GameManager.Instance.ResetAnimationInfo += ResetAnimationInfo;
            
            if (!_hasAnimationDone)
            {
                RestartPosForAnimation();
                StartUIShow();
            }
        }
        
        private void StartUIShow()
        {
            _characterTween = characters.DOLocalMove(_initialCharactersPos, CharacterAnimationDuration);
            _characterListTween = characterList.DOLocalMove(_initialCharacterListPos, CharacterListAnimationDuration);

            _hasAnimationDone = true;
        }

        private void RestartPosForAnimation()
        {
            var characterPosition = characters.localPosition;
            var characterListPosition = characterList.localPosition;
            
            characterPosition = new Vector3(characterPosition.x - Screen.width, characterPosition.y, characterPosition.z);
            characterListPosition = new Vector3(characterListPosition.x, characterListPosition.y - Screen.height / 2, characterListPosition.z);
            
            characters.localPosition = characterPosition;
            characterList.localPosition = characterListPosition;
        }

        public void ChangeBackgroundColor(Characters character)
        {
            if (CharacterConfig.Instance.CharacterData.TryGetValue(character, out var data))
            {
                backgroundColor.color = data.backgroundCharacterSelectionColor;
            }
        }

        private void ResetAnimationInfo()
        {
            _hasAnimationDone = false;
        }

        private void OnDisable()
        {
            _characterTween?.Kill(true);
            _characterListTween?.Kill(true);
        }
    }
}
