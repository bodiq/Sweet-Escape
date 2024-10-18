using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abilities;
using Audio;
using Configs;
using DG.Tweening;
using Enums;
using Extensions;
using Structs;
using UnityEngine;
using AudioType = Audio.AudioType;

public class  Player : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerMagnet playerMagnet;
    [SerializeField] private PlayerCollision playerCollision;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private BoxCollider2D playerBoxCollider;
    [SerializeField] private BoxCollider2D magnetBoxCollider;

    [SerializeField] private TongueGrab tongueGrab;

    [Header("Movement")]
    public DirectionEnum direction;

    public bool ableToMove = true;

    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerCollision PlayerCollision => playerCollision;
    public PlayerMagnet PlayerMagnet => playerMagnet;

    public TongueGrab TongueGrab => tongueGrab;

    public readonly Dictionary<Enums.PowerUps, bool> PowerUpsActivated = new();
    
    public float multiplierCoins;
    public float multiplierSprinkle;

    public int coinsPerGame;

    private bool _isDead;
    private int _shields = 0;
    private int _inGameShields = 0;

    private int _level;
    private CharacterPreset _characterPreset;

    private readonly List<ICharacterAbility> _characterAbilities = new();

    private Characters _selectedCharacter;

    private bool _hasImmunity;

    private WaitForSeconds _immunityDuration;

    private Coroutine _immunityCoroutine;

    private const string AnimationStateDeath = "Death";

    private readonly Vector3 _offsetForHazardOnRevive = new(0f, -30f, 0f);

    public bool HasImmunity => _hasImmunity;

    public bool IsDead => _isDead;

    private readonly Color _initialNormalColorValue = new (1, 1, 1, 1);
    private readonly Color _startNormalColorValue = new (1, 1, 1, 1);
    private readonly Color _endNormalColorValue = new (1, 1, 1, 0.1f);

    private readonly Color _endHitColorValue = new(1f, 0.56f, 0.56f, 1f);
    
    private Color _endTempHitColorValue;

    private const float DurationAlphaChange = 0.3f;
    private const float DurationHitColorChange = 0.35f;

    private Coroutine _deathCoroutine;

    public bool magnetSprinkles;
    private bool _isInfinitiveFireImmune;

    private Tweener _colorChangeTweener;
    
    private RuntimeAnimatorController _defaultController;
    private RuntimeAnimatorController _shieldedController;
    
    private int _fireImmune = 0;

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerRespawn += OnRespawn;
        multiplierCoins = 1;
        multiplierSprinkle = 1;
        
        var allPowerUps = Enum.GetValues(typeof(Enums.PowerUps)).Cast<Enums.PowerUps>().ToArray();
        PowerUpsActivated.SetupKeys(allPowerUps);

        _immunityDuration = new WaitForSeconds(CharacterConfig.Instance.ImmunityDuration);
    }

    public void OnPlayerDamage(bool isLava = false)
    {
        if ( _shields == 0 || isLava && !_isDead)
        {
            _isDead = true;
            ableToMove = false;
            SetParent(null);
            playerBoxCollider.enabled = false;
            magnetBoxCollider.enabled = false;
            _deathCoroutine ??= StartCoroutine(DeathCoroutine());
            return;
        }

        if (_shields <= 0) return;
        
        AudioManager.Instance.PlaySFX(AudioType.Hit);

        if (_colorChangeTweener != null)
        {
            _colorChangeTweener.Kill();
            _colorChangeTweener = null;
            spriteRenderer.color = _initialNormalColorValue;
        }
        
        foreach (var enemy in GameManager.Instance.Enemies.Where(enemy => enemy.GetEnemyType() is not (EnemyType.BarrelDropper or EnemyType.SlamBlock or EnemyType.PersistantHazard or EnemyType.StaticSpike or EnemyType.HiddenSpike)))
        {
            enemy.TurnCollider(false);
        }

        _hasImmunity = true;
            
        _colorChangeTweener = spriteRenderer.DOColor(_endHitColorValue, DurationHitColorChange).SetLoops(2, LoopType.Yoyo).OnComplete(
        () =>
        {
            _hasImmunity = false;
            foreach (var enemy in GameManager.Instance.Enemies.Where(enemy => enemy.GetEnemyType() is not (EnemyType.BarrelDropper or EnemyType.SlamBlock or EnemyType.PersistantHazard or EnemyType.StaticSpike or EnemyType.HiddenSpike)))
            {
                enemy.TurnCollider(true);
            }
        });
            
        playerMovement.SetPlayerLastPos();
        _shields--;

        var additionalShields = PlayerPrefs.GetInt(Enums.PowerUps.WaffleShield.ToString());

        if (_inGameShields > 0)
        {
            _inGameShields--;
        }
        else if (additionalShields > 0)
        {
            additionalShields--;
        }
            
        PlayerPrefs.SetInt(Enums.PowerUps.WaffleShield.ToString(), additionalShields);

        UIManager.Instance.HUDScreen.UIPowerUpsManager.DecreaseOneShield();
        if (_shields == 0)
        {
            PlayerMovement.ChangeControllerToDefault(_defaultController);
        }
    }
    
    public void Initialize(int level, CharacterPreset characterPreset, Characters character, int additionalShields, RuntimeAnimatorController defaultController, RuntimeAnimatorController shieldedController)
    {
        _defaultController = defaultController;
        _shieldedController = shieldedController;

        SetShields(additionalShields);
        
        _selectedCharacter = character;
        _level = level;
        _characterPreset = characterPreset;
        InitializeAbilities();

        PlayerMovement.SetStartController(_shields > 0 ? _shieldedController : _defaultController);
    }

    private IEnumerator DeathCoroutine()
    {
        AudioManager.Instance.PlaySFX(AudioType.DeadHit);
        PlayerMovement.StopMovement();
        PlayerMovement.ChangeAnimationState(AnimationStateDeath);
        UIManager.Instance.LostScreenUI.FadeInRedBackground();
        yield return new WaitForSeconds(0.8f);
        GameManager.Instance.OnPlayerDeath?.Invoke();
        _deathCoroutine = null;
    }

    private void InitializeAbilities()
    {
        for (var i = 0; i < _level; i++)
        {
            if (i < _characterPreset.abilityList.Count)
            {
                var ability = _characterPreset.abilityList[i];
                if (ability.AbilityType == CharacterAbilities.Nothing || ability.Ability == null)
                {
                    continue;
                }
                _characterAbilities.Add(ability.Ability);
                ability.Ability.InitializeAbility(this, i + 1);
            }
            else
            {
                break;
            }
        }
    }

    public void SetFireImmune(int count, bool isInfinitive = false)
    {
        _fireImmune = count;
        _isInfinitiveFireImmune = isInfinitive;
        
        if (count > 0 || isInfinitive)
        {
            foreach (var enemy in GameManager.Instance.Enemies.Where( enemy => enemy.GetEnemyType() is EnemyType.FireCannon))
            {
                enemy.TurnCollider(false); 
            }
        }
    }

    public void CheckForFireImmune()
    {
        if (_isInfinitiveFireImmune)
        {
            return;
        }

        if (_fireImmune > 0)
        {
            _fireImmune--;
        }
        else
        {
            OnPlayerDamage();
        }
    }

    public void OnPlayerGetCoin(int count)
    {
        var finalCoinReward = count;
        if (GameManager.Instance.CharactersData.TryGetValue(_selectedCharacter, out var characterData))
        {
            var amountCoinReward = count * characterData.AdditionalPercentCoinReward;
            var toRound = count + amountCoinReward;
            finalCoinReward = Mathf.RoundToInt(toRound);
            coinsPerGame += finalCoinReward;
            coinsPerOneRun += finalCoinReward;
        }

        UIManager.Instance.HUDScreen.RefreshCoinsCount(finalCoinReward);
    }

    private void TurnOnImmunity()
    {
        _immunityCoroutine = StartCoroutine(StartImmunityCoroutine());
    }

    private IEnumerator StartImmunityCoroutine()
    {
        foreach (var enemy in GameManager.Instance.Enemies.Where(enemy => enemy.GetEnemyType() is not (EnemyType.BarrelDropper or EnemyType.SlamBlock or EnemyType.PersistantHazard or EnemyType.StaticSpike or EnemyType.HiddenSpike)))
        {
            enemy.TurnCollider(false);
        }
        _hasImmunity = true;
        FlashPlayerTween(DurationAlphaChange);
        yield return _immunityDuration;
        
        foreach (var enemy in GameManager.Instance.Enemies.Where(enemy => enemy.GetEnemyType() is not (EnemyType.BarrelDropper or EnemyType.SlamBlock or EnemyType.PersistantHazard or EnemyType.StaticSpike or EnemyType.HiddenSpike)))
        {
            enemy.TurnCollider(true);
        }
        _flashingPlayerTween.Kill();
        spriteRenderer.color = _initialNormalColorValue;
        _hasImmunity = false;
        _immunityCoroutine = null;
    }

    public void SetShields(int count, bool isSet = false, bool inGameShields = false)
    {
        if (!isSet)
        {
            if (inGameShields)
            {
                _inGameShields += count;
            }

            _shields += count;
        }
        else
        {
            _shields = count;
        }
        
        UIManager.Instance.HUDScreen.UIPowerUpsManager.InitializePowerUp(Enums.PowerUps.WaffleShield, null, _shields);

        if (_shields > 0)
        {
            PlayerMovement.ChangeControllerToShielded(_shieldedController);
        }
        else
        {
            PlayerMovement.SetStartController(_defaultController);
        }
    }

    public void OnPlayerGetPoint(float count)
    {
        UIManager.Instance.HUDScreen.IncreasePointsCount(count);
    }
    
    private void OnRespawn()
    {
        _inGameShields = 0;

        _shields = PlayerPrefs.GetInt(Enums.PowerUps.WaffleShield.ToString());
        
        SetShields(_shields, true);

        coinsPerOneRun = 0;

        Respawn();
        PlayerMovement.SetRespawnPlayerMovement();
        _characterAbilities.Clear();
        
        if (_selectedCharacter != Characters.Kermit)
        {
            InitializeAbilities();
        }

        if (_immunityCoroutine != null)
        {
            StopCoroutine(_immunityCoroutine);
            _immunityCoroutine = null;
        }
        
        _flashingPlayerTween.Kill();
        spriteRenderer.color = _initialNormalColorValue;

        multiplierCoins = 1;
        multiplierSprinkle = 1;
    }

    private void Respawn()
    {
        transform.position = TilemapManager.Instance.PlayerSpawnPoint.position;
        ableToMove = true;
        _isDead = false;
        
        playerBoxCollider.enabled = true;
        magnetBoxCollider.enabled = true;
        
        foreach (var enemy in GameManager.Instance.Enemies)
        {
            if (enemy.GetEnemyType() is EnemyType.BarrelDropper or EnemyType.SlamBlock or EnemyType.PersistantHazard or EnemyType.StaticSpike or EnemyType.HiddenSpike)
            {
                continue;
            }

            enemy.TurnCollider(true);
        }
        
        TurnAllPowerUps(false);
        _deathCoroutine = null;
    }

    public void ReviveAtThePosition()
    {
        playerBoxCollider.enabled = true;
        magnetBoxCollider.enabled = true;

        PlayerCollision._lastCollision = null;
        
        PlayerMovement.SetPlayerLastPos();
        ableToMove = true;
        _isDead = false;

        if (Mathf.Abs(TilemapManager.Instance.PersistantHazard.transform.position.y - transform.position.y) < 35)
        {
            TilemapManager.Instance.PersistantHazard.transform.position += _offsetForHazardOnRevive;
        }
        
        TurnOnImmunity();
    }
    
    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRespawn -= OnRespawn;
    }

    private void TurnAllPowerUps(bool isActivated)
    {
        PowerUpsActivated[Enums.PowerUps.WaffleShield] = isActivated;
        PowerUpsActivated[Enums.PowerUps.Magnet] = isActivated;
        PlayerMagnet.MagnetTurn(false);
        PowerUpsActivated[Enums.PowerUps.ChillBlast] = isActivated;
        PowerUpsActivated[Enums.PowerUps.GoldSpoon] = isActivated;
        PowerUpsActivated[Enums.PowerUps.HundAThousands] = isActivated;
    }

    #region Tween

    private Tweener _flashingPlayerTween;
    private Tweener _hitPlayerTween;
    public int coinsPerOneRun;

    private Tweener FlashPlayerTween(float duration, Ease ease = Ease.Linear)
    {
        if (_flashingPlayerTween.IsActive())
        {
            _flashingPlayerTween.ChangeValues(0f, 1f, duration)
                .SetEase(ease)
                .Restart();
        }
        else
        {
            _flashingPlayerTween = DOTween.To(FlashSetter, 0f, 1f, duration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(ease)
                .SetLink(gameObject)
                .SetAutoKill(false);
        }

        return _flashingPlayerTween;
    }

    private void FlashSetter(float t)
    {
        spriteRenderer.color = Color.Lerp(_startNormalColorValue, _endNormalColorValue, t);
    }

    private Tweener SetPlayerHitColor(float duration, Color endValue, Ease ease = Ease.Linear)
    {
        _hitPlayerTween = DOTween.To(HitSetter, 0, 1, duration).SetLoops(0, LoopType.Yoyo);

        return _hitPlayerTween;
    }

    private void HitSetter(float t)
    {
        spriteRenderer.color = Color.Lerp(_initialNormalColorValue, _endHitColorValue, t);
    }

    #endregion
}

