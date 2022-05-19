using System;
using System.Collections.Generic;
using System.Linq;
using LittleBit.Modules.IAppModule.Data.Products;
using LittleBit.Modules.IAppModule.Data.ProductWrappers;
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
        private Dictionary<string, ProductConfig> _allProducts;

        public event Action<string> OnPurchasingSuccess;
        public event Action<string> OnPurchasingFailed;

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

            InitAllProducts();

            return builder;
        }

        private void InitAllProducts()
        {
            _allProducts = new Dictionary<string, ProductConfig>();

            _offerConfigs.ForEach(o => _allProducts.Add(o.Id, o));
            _offerConfigs.SelectMany(o => o.Products).ToList().ForEach(p => _allProducts.Add(p.Id, p));
        }

        public void Purchase(string id)
        {
            var product = _controller.products.WithID(id);

            if (product is {availableToPurchase: false}) return;

            _controller.InitiatePurchase(product);
        }

        public IProductWrapper CreateProductWrapper(string id)
        {
#if UNITY_EDITOR
            return new EditorProductWrapper(_allProducts[id], this);
#else
            return new RuntimeProductWrapper(_controller.products.WithID(id));
#endif
        }

        public void RestorePurchasedProducts(Action<bool> callback)
        {
            _transactionsRestorer.Restore(_extensionProvider, callback);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var result = _purchaseHandler.ProcessPurchase(purchaseEvent, (success) =>
            {
                var id = purchaseEvent.purchasedProduct.definition.id;

                if (success)
                    OnPurchasingSuccess?.Invoke(id);
                else
                    OnPurchasingFailed?.Invoke(id);

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
            OnPurchasingFailed?.Invoke(product.definition.id);
            Debug.LogError("PURCHASE FAILED!");
        }
    }
}