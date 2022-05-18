using System;
using System.Collections.Generic;
using LittleBit.Modules.IAppModule.Data.Purchases;
using LittleBit.Modules.IAppModule.Services.PurchaseProcessors;
using LittleBit.Modules.IAppModule.Services.TransactionsRestorers;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Services
{
    public class IAPService : IService, IStoreListener, IIAPService
    {
        private IStoreController _controller;
        private IExtensionProvider _extensionProvider;
        
        private readonly ITransactionsRestorer _transactionsRestorer;
        private readonly IPurchaseHandler _purchaseHandler;
        private readonly List<OfferConfig> _offerConfigs;

        public event Action OnPurchasingSuccess;
        public event Action OnPurchasingFailed;

        public IAPService(ITransactionsRestorer transactionsRestorer,
            IPurchaseHandler purchaseHandler, List<OfferConfig> offerConfigs)
        {
            _purchaseHandler = purchaseHandler;
            _offerConfigs = offerConfigs;
            _transactionsRestorer = transactionsRestorer;

            var builder = InitBuilder();

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _extensionProvider = extensions;
            _controller = controller;
        }

        private ConfigurationBuilder InitBuilder()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            _offerConfigs.ForEach(offer => builder.AddProduct(offer.Id, offer.ProductType));
            return builder;
        }

        public void Purchase(string id)
        {
            var product = _controller.products.WithID(id);

            if (product is {availableToPurchase: false}) return;

            _controller.InitiatePurchase(product);
        }

        public ProductWrapper CreateProductWrapper(string id)
        {
            return new ProductWrapper(_controller.products.WithID(id));
        }

        public void RestorePurchasedProducts(Action<bool> callback)
        {
            _transactionsRestorer.Restore(_extensionProvider, callback);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var result = _purchaseHandler.ProcessPurchase(purchaseEvent, (success) =>
            {
                if (success) OnPurchasingSuccess?.Invoke();
                else OnPurchasingFailed?.Invoke();
                
                Debug.LogError("Processing result: " + success);
            });

            return result;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError("INITIALIZED FAILED!");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchasingFailed?.Invoke();
            Debug.LogError("PURCHASE FAILED!");
        }
    }
}