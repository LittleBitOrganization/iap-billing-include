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

        private Dictionary<string, EditorProductWrapper> _editorProductWrappers;
        public event Action<string> OnPurchasingSuccess;
        public event Action<string> OnPurchasingFailed;
        public event Action OnInitializationComplete;

        public IAPService(ITransactionsRestorer transactionsRestorer,
            IPurchaseHandler purchaseHandler, List<OfferConfig> offerConfigs)
        {
            _purchaseHandler = purchaseHandler;
            _offerConfigs = offerConfigs;
            _transactionsRestorer = transactionsRestorer;

            var builder = InitBuilder();

            InitAllProducts();

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _extensionProvider = extensions;
            _controller = controller;
            
            OnInitializationComplete?.Invoke();
        }

        private ConfigurationBuilder InitBuilder()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            _offerConfigs.ForEach(offer => builder.AddProduct(offer.Id, offer.ProductType));

            return builder;
        }

        private void InitAllProducts()
        {
            _allProducts = new Dictionary<string, ProductConfig>();

            _editorProductWrappers = new Dictionary<string, EditorProductWrapper>();

            _offerConfigs
                .ForEach(AddProductToAllProducts);

            _offerConfigs
                .SelectMany(o => o.Products)
                .ToList()
                .ForEach(AddProductToAllProducts);


            _allProducts.Select(kvp => kvp.Key)
                .ToList()
                .ForEach(id => _editorProductWrappers.Add(id, CreateEditorProductWrapper(id)));

#if IAP_DEBUG
            OnInitializationComplete?.Invoke();
#endif
        }

        private void AddProductToAllProducts(ProductConfig productConfig)
        {
            var id = productConfig.Id;

            if (string.IsNullOrEmpty(id)) return;

            if (_allProducts.ContainsKey(id)) return;

            _allProducts.Add(id, productConfig);
        }

        public void Purchase(string id)
        {
            var product = _controller.products.WithID(id);

            if (product is {availableToPurchase: false}) return;

            _controller.InitiatePurchase(product);
        }

        public IProductWrapper GetProductWrapper(string id)
        {
#if IAP_DEBUG
            return GetDebugProductWrapper(id);
#else
            try
            {
                return GetRuntimeProductWrapper(id);
            }
            catch
            {
                Debug.LogError($"Can't create runtime product wrapper with id:{id}");
                return GetDebugProductWrapper(id);
            }
#endif
        }

        private RuntimeProductWrapper GetRuntimeProductWrapper(string id) =>
            new RuntimeProductWrapper(_controller.products.WithID(id));

        private EditorProductWrapper GetDebugProductWrapper(string id) =>
            !_editorProductWrappers.ContainsKey(id) ? null : _editorProductWrappers[id];

        private EditorProductWrapper CreateEditorProductWrapper(string id) =>
            new EditorProductWrapper(_allProducts[id]);

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
                {
#if IAP_DEBUG
                    (GetProductWrapper(id) as EditorProductWrapper)!.Purchase();
#endif
                    OnPurchasingSuccess?.Invoke(id);
                }
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