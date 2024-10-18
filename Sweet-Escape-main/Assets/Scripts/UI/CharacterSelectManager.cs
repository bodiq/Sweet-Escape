using System;
using System.Collections.Generic;
using Enums;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace UI
{
    public class CharacterSelectManager : SerializedMonoBehaviour
    {
        [OdinSerialize] private IDictionary<Characters, CharacterUIInfo> _characters;
        [SerializeField] private CharacterChoice firstCharacterChoice;
        public static CharacterSelectManager Instance { get; private set; }
    
        public Action<CharacterChoice> CharacterChanged;

        private CharacterChoice _lastSelectedCharacter;
        
        private CharacterUIInfo _lastOpenedCharacterUI;
        private MainMenuScreen _mainMenuScreen;

        public Characters _lastChoosenCharacter = Characters.Noob;

        public CharacterUIInfo LastOpenedCharacterUI => _lastOpenedCharacterUI;
    
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            CharactersUIInfoInitialization();
            _lastSelectedCharacter = firstCharacterChoice;
            _lastOpenedCharacterUI = _characters[_lastSelectedCharacter.Character];
        }

        private void OnEnable()
        {
            _mainMenuScreen = UIManager.Instance.GetUIScreen<MainMenuScreen>();
            
            CharacterChanged += RefreshSelectedCharacter;

            if (_lastSelectedCharacter)
            {
                _lastSelectedCharacter.TurnOnSelectedState();
            }
            else
            {
                _lastSelectedCharacter = firstCharacterChoice;
                _lastSelectedCharacter.TurnOnSelectedState();
            }
            
            UIManager.Instance.CharactersSection.SkinSelection.Initialize(_lastSelectedCharacter.Character);
        }

        public void CharactersUIInfoInitialization()
        {
            foreach (var character in _characters)
            {
                character.Value.InitializeCharacterUIInfo(character.Key);
            }
        }

        private void RefreshSelectedCharacter(CharacterChoice characterChoice)
        {
            if (_lastSelectedCharacter && _lastSelectedCharacter != characterChoice)
            {
                _lastSelectedCharacter.TurnOffSelectedState();
            }
            
            if (_characters.TryGetValue(characterChoice.Character, out var characterInfo))
            {
                if (_lastOpenedCharacterUI)
                {
                    _lastOpenedCharacterUI.gameObject.SetActive(false);
                }
                characterInfo.InitializeCharacterUIInfo(characterChoice.Character);
                characterInfo.gameObject.SetActive(true);
                _lastOpenedCharacterUI = characterInfo;
            }
            
            _lastSelectedCharacter = characterChoice;
            _lastChoosenCharacter = characterChoice.Character;
            _mainMenuScreen.ChangeSelectedCharacter(characterChoice.Character);
            UIManager.Instance.CharactersSection.ChangeBackgroundColor(_lastChoosenCharacter);

            switch (characterChoice.Character)
            {
                case Characters.Noob:
                    GameManager.Instance.skinEnum = SkinEnum.MintyFresh;
                    break;
                case Characters.Meltie:
                    GameManager.Instance.skinEnum = SkinEnum.MeltieHotCream;
                    break;
                case Characters.Kermit:
                    GameManager.Instance.skinEnum = SkinEnum.KermitGreen;
                    break;
                case Characters.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            UIManager.Instance.CharactersSection.SkinSelection.Initialize(_lastSelectedCharacter.Character);
        }

        private void OnDisable()
        {
            CharacterChanged -= RefreshSelectedCharacter;
        }
    }
}
