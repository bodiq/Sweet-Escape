using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsSection : UIScreen
    {
        [SerializeField] private Button onCrtButton;
        [SerializeField] private Button offCrtButton;

        [SerializeField] private Button leftArrowButton;
        [SerializeField] private Button rightArrowButton;

        [SerializeField] private List<GameObject> teamPlayersGameObjects = new();
        [SerializeField] private Transform defaultTeamPlayerPos;
        
        private int _currentGameModeBoxIndex;

        private Vector3 _leftOutScreenTeamPlayerInfoPos;
        private Vector3 _rightOutScreenTeamPlayerInfoPos;
        private Vector3 _defaultTeamPlayerInfoPos;

        private const float DurationTeamPlayerInfoBoxChange = 0.5f;

        private void Start()
        {
            var rect = UIManager.Instance.RectTransform.rect;

            var localPosition = defaultTeamPlayerPos.localPosition;
            
            _leftOutScreenTeamPlayerInfoPos = new Vector3(localPosition.x - rect.width, localPosition.y);
            _rightOutScreenTeamPlayerInfoPos = new Vector3(localPosition.x + rect.width, localPosition.y);
            _defaultTeamPlayerInfoPos = localPosition;
        }

        private void OnEnable()
        {
            onCrtButton.onClick.AddListener(TurnOnShader);
            offCrtButton.onClick.AddListener(TurnOffShader);
            
            leftArrowButton.onClick.AddListener(PreviousTeamPlayer);
            rightArrowButton.onClick.AddListener(NextTeamPlayer);

            var isActive = PlayerPrefs.GetInt(GameManager.CrtShaderActiveKey);

            if (isActive == 0)
            {
                onCrtButton.gameObject.SetActive(true);
                offCrtButton.gameObject.SetActive(false);
            }
            else
            {
                onCrtButton.gameObject.SetActive(false);
                offCrtButton.gameObject.SetActive(true);
            }
        }

        private Tween _currentTeamPlayerInfoBoxTween;
        private Tween _nextTeamPlayerInfoBoxTween;
        
        private void PreviousTeamPlayer()
        {
            leftArrowButton.interactable = false;
            rightArrowButton.interactable = false;
            
            var gameModeIndexToShow = _currentGameModeBoxIndex - 1;
            if (gameModeIndexToShow < 0)
            {
                gameModeIndexToShow = teamPlayersGameObjects.Count - 1;
            }

            teamPlayersGameObjects[gameModeIndexToShow].transform.localPosition = _rightOutScreenTeamPlayerInfoPos;

            _currentTeamPlayerInfoBoxTween = teamPlayersGameObjects[_currentGameModeBoxIndex].transform.DOLocalMove(_leftOutScreenTeamPlayerInfoPos, DurationTeamPlayerInfoBoxChange);
            _nextTeamPlayerInfoBoxTween = teamPlayersGameObjects[gameModeIndexToShow].transform.DOLocalMove(_defaultTeamPlayerInfoPos, DurationTeamPlayerInfoBoxChange).OnComplete(() =>
            {
                _currentGameModeBoxIndex = gameModeIndexToShow;
                leftArrowButton.interactable = true;
                rightArrowButton.interactable = true;
            });
        }

        private void NextTeamPlayer()
        {
            leftArrowButton.interactable = false;
            rightArrowButton.interactable = false;
            
            var gameModeIndexToShow = _currentGameModeBoxIndex + 1;
            if (gameModeIndexToShow > teamPlayersGameObjects.Count - 1)
            {
                gameModeIndexToShow = 0;
            }

            teamPlayersGameObjects[gameModeIndexToShow].transform.localPosition = _leftOutScreenTeamPlayerInfoPos;

            _currentTeamPlayerInfoBoxTween = teamPlayersGameObjects[_currentGameModeBoxIndex].transform.DOLocalMove(_rightOutScreenTeamPlayerInfoPos, DurationTeamPlayerInfoBoxChange);
            _nextTeamPlayerInfoBoxTween = teamPlayersGameObjects[gameModeIndexToShow].transform.DOLocalMove(_defaultTeamPlayerInfoPos, DurationTeamPlayerInfoBoxChange).OnComplete(() =>
            {
                _currentGameModeBoxIndex = gameModeIndexToShow;
                leftArrowButton.interactable = true;
                rightArrowButton.interactable = true;
            });
        }

        private void TurnOnShader()
        {
            UIManager.Instance.MainMenuScreen.TurnScanLinesBackground(true);
            GameManager.Instance.GlobalVolume.SetActive(true);
            onCrtButton.gameObject.SetActive(false);
            offCrtButton.gameObject.SetActive(true);
            
            PlayerPrefs.SetInt(GameManager.CrtShaderActiveKey, 1);
        }

        private void TurnOffShader()
        {
            UIManager.Instance.MainMenuScreen.TurnScanLinesBackground(false);
            GameManager.Instance.GlobalVolume.SetActive(false);
            
            onCrtButton.gameObject.SetActive(true);
            offCrtButton.gameObject.SetActive(false);
            
            PlayerPrefs.SetInt(GameManager.CrtShaderActiveKey, 0);
        }

        private void OnDisable()
        {
            leftArrowButton.onClick.RemoveListener(PreviousTeamPlayer);
            rightArrowButton.onClick.RemoveListener(NextTeamPlayer);
            
            _currentTeamPlayerInfoBoxTween?.Kill();
            _nextTeamPlayerInfoBoxTween?.Kill();
        }
    }
}
