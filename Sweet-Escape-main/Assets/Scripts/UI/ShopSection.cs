using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using DG.Tweening;
using Enums;
using Extensions;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;
using Product = UnityEngine.Purchasing.Product;

namespace UI
{
    public class ShopSection : UIScreen, IDetailedStoreListener
    {
        [SerializeField] private Button coinsButton;
        [SerializeField] private List<ShopItem> shopItems;
        [SerializeField] private List<ShopItem> shopMoneyItems;
        [SerializeField] private GameObject shopListGameObject;
        [SerializeField] private GameObject jumpToSectionGameObject;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentPanel;

        [SerializeField] private Button specialOfferButton;
        [SerializeField] private Button powerUpsButton;
        [SerializeField] private Button playerSkinsButton;
        [SerializeField] private Button coinBundlesButton;
        
        [SerializeField] private List<RectTransform> rectTransformsToMove;

        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;

        private Action<bool> _onPurchaseCompleted;

        private Vector3 _initialShopListPos;
        private Vector3 _initialTittlePos;

        private Vector3 _initialJumpToSectionPos;

        private bool _hasAnimationDone;
        
        private const float DurationShopListAnimation = 0.5f;
        private const float DurationShopTittleAnimation = 0.5f;
        private const float TenMilliseconds = 0.03f;

        private Coroutine _startShopItemsUIShowCoroutine;

        protected override async void Awake()
        {
            base.Awake();
            
            _initialShopListPos = shopListGameObject.transform.localPosition;
            _initialJumpToSectionPos = jumpToSectionGameObject.transform.localPosition;

            var options = new InitializationOptions()
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                .SetEnvironmentName("Test");
#else
                .SetEnvironmentName("Production");
#endif
            await UnityServices.InitializeAsync(options);
            ResourceRequest operation = Resources.LoadAsync<TextAsset>("IAPProductCatalog");
            operation.completed += HandleIAPCatalogLoaded;
        }

        private void HandleIAPCatalogLoaded(AsyncOperation operation)
        {
            var request = operation as ResourceRequest;
            
            Debug.Log($"Loaded Asset: {request?.asset}");
            ProductCatalog catalog = JsonUtility.FromJson<ProductCatalog>((request?.asset as TextAsset)?.text);
            Debug.Log($"Loaded catalog with {catalog.allProducts.Count} items");

            /*StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
            StandardPurchasingModule.Instance().useFakeStoreAlways = true;*/
            
#if UNITY_ANDROID
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.GooglePlay));
#elif UNITY_IOS 
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.AppleAppStore));
#else
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.NotSpecified));
#endif

            foreach (var item in catalog.allProducts)
            {
                builder.AddProduct(item.id, item.type);
            }
            
            UnityPurchasing.Initialize(this, builder);
        }

        public override void TurnOff()
        {
            base.TurnOff();

            UIManager.Instance.MainMenuScreen.FactoryShopTittleBackground.SetActive(false);
            
            _jumpToSectionTween?.Kill();
            
            jumpToSectionGameObject.transform.localPosition = _initialJumpToSectionPos;
            
            if (_startShopItemsUIShowCoroutine != null)
            {
                StopCoroutine(_startShopItemsUIShowCoroutine);
                _startShopItemsUIShowCoroutine = null;
            }
            
            foreach (var shopItem in shopItems)
            {
                shopItem.SetDefaultPos();
            }
        }

        private void OnEnable()
        {
            GameManager.Instance.ResetAnimationInfo += ResetAnimationInfo;
            
            AdsManager.Instance.LoadRewardedAd();

            coinsButton.onClick.AddListener(OnCoinsButtonClicked);
            
            specialOfferButton.onClick.AddListener(JumpToSpecialOfferSection);
            powerUpsButton.onClick.AddListener(JumpToPowerUpsSection);
            playerSkinsButton.onClick.AddListener(JumpToPlayerSkinsSection);
            coinBundlesButton.onClick.AddListener(JumpToCoinBundlesSection);

            if (_hasAnimationDone) return;

            _startShopItemsUIShowCoroutine = StartCoroutine(StartShopItemsUIShow());

            _hasAnimationDone = true;
            //ResetPosForAnimation();
            //StartUIShow();
        }

        private IEnumerator StartShopItemsUIShow()
        {
            foreach (var shopItem in shopItems)
            {
                shopItem.SetupPreAnimationPos();
            }
            
            for (var i = 0; i < shopItems.Count; i++)
            {
                shopItems[i].StartUIShow();
                yield return new WaitForSeconds(TenMilliseconds * (i + 1));
            }
        }

        public void RefreshPlayerSkinsData()
        {
            foreach (var shopItem in shopItems.Where(shopItem => shopItem.ShopCurrencyItems == ShopCurrencyItems.PlayerSkins))
            {
                shopItem.RefreshData();
            }
        }

        private void JumpToSpecialOfferSection()
        {
            scrollRect.FocusOnItem(rectTransformsToMove[0]);
        }
        
        private void JumpToPowerUpsSection()
        {
            scrollRect.FocusOnItem(rectTransformsToMove[1]);
        }
        
        private void JumpToPlayerSkinsSection()
        {
            scrollRect.FocusOnItem(rectTransformsToMove[2]);
        }
        
        private void JumpToCoinBundlesSection()
        {
            scrollRect.FocusOnItem(rectTransformsToMove[3]);
        }

        private void Start()
        {
            var rect = UIManager.Instance.RectTransform.rect;
            jumpToSectionGameObject.transform.localPosition = new Vector3(_initialJumpToSectionPos.x, _initialJumpToSectionPos.y + rect.height / 4, _initialJumpToSectionPos.z);
        }

        //private Tweener _shopListTween;
        private Tweener _jumpToSectionTween;

        public void StartUIShow()
        {
            _jumpToSectionTween = jumpToSectionGameObject.transform.DOLocalMove(_initialJumpToSectionPos, 1f).SetEase(Ease.OutBack);
        }
        
        private void ResetAnimationInfo()
        {
            _hasAnimationDone = false;
        }

        private void OnDisable()
        {
            coinsButton.onClick.RemoveListener(OnCoinsButtonClicked);

           //_shopListTween?.Kill(true);
            _jumpToSectionTween?.Kill();
            
            specialOfferButton.onClick.RemoveListener(JumpToSpecialOfferSection);
            powerUpsButton.onClick.RemoveListener(JumpToPowerUpsSection);
            playerSkinsButton.onClick.RemoveListener(JumpToPlayerSkinsSection);
            coinBundlesButton.onClick.RemoveListener(JumpToCoinBundlesSection);

            if (_startShopItemsUIShowCoroutine != null)
            {
                StopCoroutine(_startShopItemsUIShowCoroutine);
                _startShopItemsUIShowCoroutine = null;
            }

            /*
            Debug.LogError("Disable");
            
            foreach (var shopItem in shopItems)
            {
                shopItem.SetDefaultPos();
            }*/
        }

        private void OnCoinsButtonClicked()
        {
            AudioManager.Instance.mainBackgroundAudioSource.Stop();
            AdsManager.Instance.ShowRewardedAd(Constants.AdPlacementShopCoins);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"Error initializing IAP because of {error}." + $"\r\nShow a message to the player depending on the error");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"Error initializing IAP because of {error}." + $"\r\nShow a message to the player depending on the error");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            Debug.Log($"Successfully purchased {purchaseEvent.purchasedProduct.definition.id}");
            _onPurchaseCompleted?.Invoke(true);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log($"Failed to purchase {product.definition.id} because {failureReason}");
            _onPurchaseCompleted?.Invoke(false);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;
            InitializeShopItems();   
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.Log($"Failed to purchase {product.definition.id} because {failureDescription}");
            _onPurchaseCompleted?.Invoke(false);
        }

        private void InitializeShopItems()
        {
            var products = _storeController.products.all;

            for (var i = 0; i < products.Length; i++)
            {
                var shopItem = shopMoneyItems[i];
                shopItem.OnPurchase += HandlePurchase;
                shopItem.Setup(products[i]);
            }
        }

        private void HandlePurchase(Product product, Action<bool> onPurchaseCompleted)
        {
            _onPurchaseCompleted = onPurchaseCompleted;
            _storeController.InitiatePurchase(product);
        }
    }
}