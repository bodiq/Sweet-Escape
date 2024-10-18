using Audio;
using Cinemachine;
using DG.Tweening;
using Enemy;
using Enums;
using MoreMountains.FeedbacksForThirdParty;
using UnityEngine;

public class PersistantHazard : MonoBehaviour, IEnemy
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Animator animator;
    [SerializeField] private CinemachineVirtualCamera camera;

    [SerializeField] private AudioSource persistantAudioSource;

    private bool _isFrozen;

    private float _lastSavedDistance;

    private float _endVolume;

    private const float DefaultOrthoSize = 18f;
    private const float DefaultAmplitudeGain = 0.2f;
    private const float DefaultVolumePersistantHazard = 0.1f;

    private MMF_CinemachineImpulse _cameraShake;

    private Player _player;

    private float _volumeAudioSource;

    private const float DecreasingVolume = 0.03f;
    private const float IncreasingVolume = 0.01f;

    private float _decreasingVolumeNumber;
    private float _increasingVolumeNumber;
    
    public float MovementSpeed
    {
        get => movementSpeed;
        set => movementSpeed = value;
    }
    
    private void Awake()
    {
        GameManager.Instance.Enemies.Add(this);
    }

    private void Start()
    {
        _cameraShake = TilemapManager.Instance.HazardCloseShake.FeedbacksList[0] as MMF_CinemachineImpulse;
        _player = GameManager.Instance.player;
    }

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerRespawn += ResetPosition;
        GameManager.Instance.OnStopGame += TurnOffSound;
        
        AudioManager.Instance.AddEnemyAudioSourceToPool(persistantAudioSource);
        SetSoundData();
    }
    
    private void SetSoundData()
    {
        _volumeAudioSource = PlayerPrefs.GetFloat(GameManager.UserSoundFXVolumeKey);
        persistantAudioSource.volume = _volumeAudioSource;

        _decreasingVolumeNumber = DecreasingVolume * _volumeAudioSource;
        _increasingVolumeNumber = IncreasingVolume * _volumeAudioSource;
    }

    private void FixedUpdate()
    {
        if (!_isFrozen)
        {
            var position = transform.position;
            position += new Vector3(0, 1f, 0f) * (Time.deltaTime * MovementSpeed);

            transform.position = position;
        }

        if (_player)
        {
            var distance = Mathf.Abs(transform.position.y + 10f - _player.transform.position.y);

            if (distance <= 25f)
            {
                if (persistantAudioSource && !persistantAudioSource.isPlaying)
                {
                    persistantAudioSource.Play();   
                    SmoothSetToDefault(0.3f);
                }
                
                if(!TilemapManager.Instance.HazardCloseShake.IsPlaying)
                {
                    TilemapManager.Instance.HazardCloseShake.PlayFeedbacks();
                }

                if (distance > _lastSavedDistance && Time.timeScale != 0f && camera.m_Lens.OrthographicSize < DefaultOrthoSize && _cameraShake.m_ImpulseDefinition.m_AmplitudeGain > DefaultAmplitudeGain)
                {
                    _cameraShake.m_ImpulseDefinition.m_AmplitudeGain -= 0.01f;
                    camera.m_Lens.OrthographicSize += 0.2f;
                    if (_volumeAudioSource > 0)
                    {
                        persistantAudioSource.volume -= _decreasingVolumeNumber;   
                    }
                }
                else if(distance < _lastSavedDistance && Time.timeScale != 0f)
                {
                    _cameraShake.m_ImpulseDefinition.m_AmplitudeGain += 0.002f;
                    camera.m_Lens.OrthographicSize -= 0.007f;
                    if (_volumeAudioSource > 0)
                    {
                        persistantAudioSource.volume += _increasingVolumeNumber;
                    }
                }
                
                _lastSavedDistance = distance;
            }
            else
            {
                if (TilemapManager.Instance.HazardCloseShake.IsPlaying)
                {
                    TilemapManager.Instance.HazardCloseShake.StopFeedbacks();
                    SmoothSetToDefault(0.6f, 0f);
                }
            }
        }
        else
        {
            _player = GameManager.Instance.player;
        }
    }

    private void TurnOffSound()
    {
        persistantAudioSource.Stop();
    }

    private void SmoothSetToDefault(float duration, float otherVolume = DefaultVolumePersistantHazard)
    {
        if (_volumeAudioSource > 0)
        {
            _endVolume = otherVolume;
        }
        //TODO:Make reusable Tween
        DOTween.To(Setter, 0, 1, duration).SetEase(Ease.Linear);
    }

    private void Setter(float t)
    {
        camera.m_Lens.OrthographicSize = Mathf.Lerp(camera.m_Lens.OrthographicSize, DefaultOrthoSize, t);
        _cameraShake.m_ImpulseDefinition.m_AmplitudeGain = Mathf.Lerp(_cameraShake.m_ImpulseDefinition.m_AmplitudeGain, DefaultAmplitudeGain, t);
        persistantAudioSource.volume = Mathf.Lerp(persistantAudioSource.volume, _endVolume, t);
    }
    
    private void ResetPosition()
    {
        transform.position = startPosition.position;
        TilemapManager.Instance.HazardCloseShake.StopFeedbacks();
        _cameraShake.m_ImpulseDefinition.m_AmplitudeGain = DefaultAmplitudeGain;
        camera.m_Lens.OrthographicSize = DefaultOrthoSize;
        persistantAudioSource.volume = 0f;
    }
    
    public void OnEnter(Player player)
    {
        player.OnPlayerDamage(true);
    }

    public void NormalizeHazardPosition()
    {
        var transform1 = transform;
        var normalizedHazardPos = new Vector3(GameManager.Instance.player.transform.position.x, transform1.position.y);

        transform1.position = normalizedHazardPos;
    }

    public void OnExit()
    {
        
    }

    public GameObject GameObject => gameObject;
    public void ChangeMovement()
    {
        
    }

    public void Freeze()
    {
        animator.enabled = false;
        _isFrozen = true;
    }

    public void UnFreeze()
    {
        animator.enabled = true;
        _isFrozen = false;
    }

    public DirectionEnum GetEnemyDirection()
    {
        return DirectionEnum.None;
    }

    public void TurnCollider(bool isActive)
    {
        return;
    }

    public EnemyType GetEnemyType()
    {
        return EnemyType.PersistantHazard;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRespawn -= ResetPosition;
        GameManager.Instance.OnStopGame -= TurnOffSound;

        AudioManager.Instance.enemyAudioSources.Remove(persistantAudioSource);
    }
}
