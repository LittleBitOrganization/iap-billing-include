using System;
using System.Collections.Generic;
using LittleBit.Modules.IAppModule.Data.ProductWrappers;
using LittleBit.Modules.IAppModule.Data.Purchases;
using LittleBit.Modules.IAppModule.Services.PurchaseProcessors;
using LittleBit.Modules.IAppModule.Services.TransactionsRestorers;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Services
{
    public partial class IAPService : IService, IStoreListener, IIAPService
    {
        private ConfigurationBuilder _builder;
        private IStoreController _controller;
        private IExtensionProvider _extensionProvider;

        private readonly ITransactionsRestorer _transactionsRestorer;
        private readonly IPurchaseHandler _purchaseHandler;
        private readonly List<OfferConfig> _offerConfigs;
        public event Action<string> OnPurchasingSuccess;
        public event Action<string> OnPurchasingFailed;
        public event Action OnInitializationComplete;

        private ProductCollections _productCollection;

        public IAPService(ITransactionsRestorer transactionsRestorer,
            IPurchaseHandler purchaseHandler, List<OfferConfig> offerConfigs)
        {
            _productCollection = new ProductCollections();

            _purchaseHandler = purchaseHandler;
            _offerConfigs = offerConfigs;
            _transactionsRestorer = transactionsRestorer;

            Init();

            UnityPurchasing.Initialize(this, _builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _extensionProvider = extensions;
            _controller = controller;

            _productCollection.AddUnityIAPProductCollection(controller.products);

            OnInitializationComplete?.Invoke();
        }

        private void Init()
        {
            _builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            _offerConfigs.ForEach(offer =>
            {
                _builder.AddProduct(offer.Id, offer.ProductType);

                _productCollection.AddConfig(offer);
            });

#if IAP_DEBUG || UNITY_EDITOR

            OnInitializationComplete?.Invoke();
#endif
        }

        public void Purchase(string id)
        {
            var product = _controller.products.WithID(id);

            if (product is {availableToPurchase: false}) return;

#if IAP_DEBUG || UNITY_EDITOR
            (GetProductWrapper(id) as EditorProductWrapper)!.Purchase();
            OnPurchasingSuccess?.Invoke(id);
#else
            _controller.InitiatePurchase(product);
#endif
        }

        public IProductWrapper GetProductWrapper(string id)
        {
#if IAP_DEBUG || UNITY_EDITOR
            return GetDebugProductWrapper(id);
#else
            try
            {
                return GetRuntimeProductWrapper(id);
            }
            catch
            {
                Debug.LogError($"Can't create runtime product wrapper with id:{id}");
                return null;
            }
#endif
        }

        private RuntimeProductWrapper GetRuntimeProductWrapper(string id) =>
            _productCollection.GetRuntimeProductWrapper(id);

        private EditorProductWrapper GetDebugProductWrapper(string id) =>
            _productCollection.GetEditorProductWrapper(id);

        public void RestorePurchasedProducts(Action<bool> callback) =>
            _transactionsRestorer.Restore(_extensionProvider, callback);

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var result = _purchaseHandler.ProcessPurchase(purchaseEvent, (success) =>
            {
                var id = purchaseEvent.purchasedProduct.definition.id;

                if (success)
                {
#if IAP_DEBUG || UNITY_EDITOR
                    (GetProductWrapper(id) as EditorProductWrapper)!.Purchase();
#endif
                    OnPurchasingSuccess?.Invoke(id);
                }
                else
                    OnPurchasingFailed?.Invoke(id);
            });

            return result;
        }

        public void OnInitializeFailed(InitializationFailureReason error) => Debug.LogError("Initialization failed!");

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchasingFailed?.Invoke(product.definition.id);
            Debug.LogError("Purchasing failed!");
        }
    }
}