using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Configs;
using DG.Tweening;
using Enums;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AudioType = Audio.AudioType;
using Product = UnityEngine.Purchasing.Product;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private ShopCoinBundlesItemType coinBundlesItemType;
    [SerializeField] private ShopCurrencyItems shopCurrencyItems;
    [SerializeField] private Characters character;
    [SerializeField] private Enums.PowerUps powerUp;
    [SerializeField] private GameObject shopItemBox;

    [SerializeField] private TextMeshProUGUI price;
    [SerializeField] private TextMeshProUGUI boxName;
    [SerializeField] private TextMeshProUGUI boxDescription;
    [SerializeField] private TextMeshProUGUI unlockCharacterFirstText;
    [SerializeField] private GameObject textButtonToBuy;
    [SerializeField] private GameObject textButtonMaxLimit;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundIcon;
    [SerializeField] private Image locked;
    [SerializeField] private List<Image> levelProgress;
    [SerializeField] private Sprite levelGain;
    [SerializeField] private Sprite levelEmpty;

    [SerializeField] private GameObject shopItemRotate;
    [SerializeField] private GameObject feedbackPopup;

    [SerializeField] private Button purchaseButton;

    private readonly Vector3 _firstPhaseRotation = new Vector3(0f, 0f, 12f);
    private readonly Vector3 _secondPhaseRotation = new Vector3(0f, 0f, -8f);
    private readonly Vector3 _thirdPhaseRotation = new Vector3(0f, 0f, 4f);
    private readonly Vector3 _fourthPhaseRotation = new Vector3(0f, 0f, 0f);

    private const int SkinPrice = 400;
    private const int SecondsPerLevelHundAThous = 2;
    private const int StartSecondsHundAThous = 10;
    
    private const float DurationRotationChange = 0.2f;
    private const float DurationSlideBoxMove = 0.75f;

    public ShopCurrencyItems ShopCurrencyItems => shopCurrencyItems;

    private SkinData _skinData;

    private Product _model;
    public delegate void PurchaseEvent(Product model, Action<bool> onComplete);
    public event PurchaseEvent OnPurchase;

    private Vector3 _startBoxPos;
    private Vector3 _endBoxPos;
    
    private Tween _moveTween;
    private Tween _rotateTween;
    
    public void Setup(Product product)
    {
        _model = product;
    }

    private void Start()
    {
        RefreshData();
    }

    private void OnEnable()
    {
        if (coinBundlesItemType != ShopCoinBundlesItemType.None)
        {
            purchaseButton.onClick.AddListener(Purchase);
        }
        else switch (shopCurrencyItems)
        {
            case ShopCurrencyItems.PlayerSkins:
                purchaseButton.onClick.AddListener(BuySkin);
                break;
            case ShopCurrencyItems.PowerUps:
                purchaseButton.onClick.AddListener(UpgradePowerUp);
                break;
            case ShopCurrencyItems.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void BuySkin()
    {
        if (GameManager.Instance.UserData.Coins < SkinPrice) return;
        
        var coinsLeft = GameManager.Instance.UserData.Coins - SkinPrice;

        GameManager.Instance.UserData.Coins = coinsLeft;
        PlayerPrefs.SetInt(GameManager.CoinsNameKey, coinsLeft);
        PlayerPrefs.SetInt(_skinData.skinEnum.ToString(), 1);
        UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(coinsLeft);

        var pooledFeedbackPopup = ObjectUIPool.Instance.GetPooledFeedbackPopup();
        if (pooledFeedbackPopup)
        {
            pooledFeedbackPopup.transform.position = purchaseButton.transform.position;
            pooledFeedbackPopup.gameObject.SetActive(true);
            pooledFeedbackPopup.StartSelfDestroyCoroutine();
        }

        AudioManager.Instance.PlaySFX(AudioType.CollectCoinUI);
        
        gameObject.SetActive(false);
    }

    private void UpgradePowerUp()
    {
        var key = powerUp.ToString();
        var level = PlayerPrefs.GetInt(key);

        var price = 200;
        
        if (powerUp == Enums.PowerUps.WaffleShield)
        {
            price = PowerUpConfig.Instance.costPerLevel;
        }
        else
        {
            price = level * PowerUpConfig.Instance.costPerLevel;
        }
        
        if (!(GameManager.Instance.UserData.Coins >= price)) return;
        
        var coinsLeft = GameManager.Instance.UserData.Coins - price;
        GameManager.Instance.UserData.Coins = coinsLeft;
        PlayerPrefs.SetInt(GameManager.CoinsNameKey, coinsLeft);
        UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(coinsLeft);
        
        if (level == 3 && powerUp == Enums.PowerUps.WaffleShield)
        {
            
        }
        else
        {
            level++;
        }

        PlayerPrefs.SetInt(key, level);
        
        var pooledFeedbackPopup = ObjectUIPool.Instance.GetPooledFeedbackPopup();
        if (pooledFeedbackPopup)
        {
            pooledFeedbackPopup.transform.position = purchaseButton.transform.position;
            pooledFeedbackPopup.gameObject.SetActive(true);
            pooledFeedbackPopup.StartSelfDestroyCoroutine();
        }
        
        AudioManager.Instance.PlaySFX(AudioType.CollectCoinUI);

        if (PowerUpConfig.Instance.PowerUpPresets.TryGetValue(powerUp, out var value))
        {
            GameManager.Instance.PowerUpData[powerUp].Level = level;
            GameManager.Instance.PowerUpData[powerUp].DurationTime = value.startDuration + value.perLevelAdditionalDuration * (level - 1);
            GameManager.Instance.PowerUpData[powerUp].Coefficient = value.startCoefficientMultiplier + value.perLevelAdditionalMultiplier * (level - 1);
        }
        
        RefreshPowerUp();
    }

    private void RefreshPowerUp()
    {
        if (!PowerUpConfig.Instance.PowerUpPresets.TryGetValue(powerUp, out var powerUpPreset)) return;
        
        var powerUpLevel = PlayerPrefs.GetInt(powerUp.ToString());

        var priceNextLevel = 200;
        
        if (powerUp == Enums.PowerUps.WaffleShield)
        {
            priceNextLevel = PowerUpConfig.Instance.costPerLevel;
        }
        else
        {
            priceNextLevel = PowerUpConfig.Instance.costPerLevel * powerUpLevel;
        }
        
        if (powerUp == Enums.PowerUps.HundAThousands)
        {
            boxDescription.text = powerUpPreset.startDescriptionText + " " + GameManager.Instance.PowerUpData[powerUp].Coefficient + "x" + powerUpPreset.endDescriptionText + " " + GameManager.Instance.PowerUpData[powerUp].DurationTime + " seconds";

            for (var i = 0; i < powerUpLevel; i++)
            {
                levelProgress[i].sprite = levelGain;
            }
            price.text = priceNextLevel.ToString();
        }
        else
        {
            if (powerUp == Enums.PowerUps.Magnet)
            {
                boxDescription.text = powerUpPreset.startDescriptionText + GameManager.Instance.PowerUpData[powerUp].Coefficient + powerUpPreset.endDescriptionText;
            }
            else
            {
                boxDescription.text = powerUpPreset.startDescriptionText + GameManager.Instance.PowerUpData[powerUp].DurationTime + powerUpPreset.endDescriptionText;
            }

            for (var i = 0; i < powerUpLevel; i++)
            {
                levelProgress[i].sprite = levelGain;
            }

            if (powerUp == Enums.PowerUps.WaffleShield)
            {
                if (powerUpLevel == 3)
                {
                    purchaseButton.interactable = false;
                    textButtonToBuy.SetActive(false);
                    textButtonMaxLimit.SetActive(true);
                }
                else
                {
                    textButtonMaxLimit.SetActive(false);
                    textButtonToBuy.SetActive(true);
                }
            }

            price.text = priceNextLevel.ToString();
        }

        if (powerUpLevel == 10)
        {
            purchaseButton.interactable = false;
            textButtonToBuy.SetActive(false);
            textButtonMaxLimit.SetActive(true);
        }
    }

    public void RefreshData()
    {
        if (coinBundlesItemType == ShopCoinBundlesItemType.RemoveAdv)
        {
            if (PlayerPrefs.HasKey(ShopCoinBundlesItemType.RemoveAdv.ToString()))
            {
                gameObject.SetActive(false);
            }
        }
        
        if (shopCurrencyItems == ShopCurrencyItems.None) return;
        
        switch (shopCurrencyItems)
        {
            case ShopCurrencyItems.None:
                break;
            case ShopCurrencyItems.PlayerSkins:
                if (PlayerPrefs.HasKey(character.ToString()))
                {
                    if (CharacterConfig.Instance.CharacterData.TryGetValue(character, out var value))
                    {
                        _skinData = value.skinData[1];
                        if (PlayerPrefs.HasKey(_skinData.skinEnum.ToString()))
                        {
                            gameObject.SetActive(false);
                        }
                        else
                        {
                            locked.gameObject.SetActive(false);
                            unlockCharacterFirstText.gameObject.SetActive(false);
                            purchaseButton.gameObject.SetActive(true);
                            boxName.text = character.ToString();
                            boxDescription.text = _skinData.skinName;
                            backgroundIcon.gameObject.SetActive(true);
                            itemIcon.gameObject.SetActive(true);
                            itemIcon.sprite = _skinData.characterSkinSprite;
                        }
                    }
                }
                else
                {
                    boxName.text = character.ToString();
                    boxDescription.text = "???";
                    purchaseButton.gameObject.SetActive(false);
                    backgroundIcon.gameObject.SetActive(false);
                    itemIcon.gameObject.SetActive(false);
                    locked.gameObject.SetActive(true);
                    unlockCharacterFirstText.gameObject.SetActive(true);
                }
                break;
            case ShopCurrencyItems.PowerUps:
                RefreshPowerUp();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void StartUIShow()
    {
        shopItemRotate.transform.Rotate(_firstPhaseRotation);
        
        AudioManager.Instance.PlaySFX(AudioType.ShopItemMove);
        
        _moveTween = shopItemBox.transform.DOLocalMove(_startBoxPos, DurationSlideBoxMove).SetEase(Ease.Linear).OnComplete(() =>
        {
            _rotateTween = shopItemRotate.transform.DOLocalRotate(_secondPhaseRotation, DurationRotationChange).OnComplete(() =>
            {
                _rotateTween = shopItemRotate.transform.DOLocalRotate(_thirdPhaseRotation, DurationRotationChange).OnComplete(() =>
                {
                    _rotateTween = shopItemRotate.transform.DOLocalRotate(_fourthPhaseRotation, DurationRotationChange);
                });
            });
        });
    }

    public void SetupPreAnimationPos()
    {
        var objectLocalPos = shopItemBox.transform.localPosition;
        _startBoxPos = objectLocalPos;

        var rect = UIManager.Instance.RectTransform.rect;

        objectLocalPos = new Vector3(_startBoxPos.x + rect.width, _startBoxPos.y, _startBoxPos.z);
        shopItemBox.transform.localPosition = objectLocalPos;
    }

    public void Purchase()
    {
        OnPurchase?.Invoke(_model, HandlePurchaseComplete);
    }

    private void HandlePurchaseComplete(bool isSuccess)
    {
        if (!isSuccess) return;
        
        switch (coinBundlesItemType)
        {
            case ShopCoinBundlesItemType.CoinsForAdv:
                break;
            case ShopCoinBundlesItemType.CoinsForMoney:
                
                var reward = _model.definition.id switch
                {
                    "single_scoop" => 1000f,
                    "triple_scoop" => 3000f,
                    "piggy_sundea" => 7000f,
                    "truck_load" => 12000f,
                    "coin_factory" => 25000f,
                    _ => 0f
                };

                GameManager.Instance.UserData.Coins += (int)reward;
                UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(GameManager.Instance.UserData.Coins);
                PlayerPrefs.SetInt(GameManager.CoinsNameKey, GameManager.Instance.UserData.Coins);
                AudioManager.Instance.PlaySFX(AudioType.Purchase);
                feedbackPopup.SetActive(true);
                break;
            case ShopCoinBundlesItemType.RemoveAdv:
                feedbackPopup.SetActive(true);
                AudioManager.Instance.PlaySFX(AudioType.Purchase);
                PlayerPrefs.SetInt(coinBundlesItemType.ToString(), 1);
                GameManager.Instance.isRemovedAdv = true;
                gameObject.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void OnDisable()
    {
        if (coinBundlesItemType != ShopCoinBundlesItemType.None)
        {
            purchaseButton.onClick.RemoveListener(Purchase);
        }
        else if (shopCurrencyItems == ShopCurrencyItems.PlayerSkins)
        {
            purchaseButton.onClick.RemoveListener(BuySkin);
        }
        else if (shopCurrencyItems == ShopCurrencyItems.PowerUps)
        {
            purchaseButton.onClick.RemoveListener(UpgradePowerUp);
        }
    }

    public void SetDefaultPos()
    {
        _moveTween?.Kill();
        _rotateTween?.Kill();
        
        shopItemBox.transform.localPosition = _startBoxPos;
        shopItemRotate.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
