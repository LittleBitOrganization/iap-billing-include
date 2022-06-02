using System;
using LittleBit.Modules.IAppModule.Commands.Factory;
using LittleBit.Modules.IAppModule.Data.Purchases;

namespace LittleBit.Modules.IAppModule.Services
{
    internal class PurchaseHandler
    {
        private readonly IIAPService _iapService;
        private readonly PurchaseCommandFactory _purchaseCommandFactory;
        private readonly PurchaseService _purchaseService;

        private OfferConfig _currentOffer;
        private Action<bool> _callback;
        private bool _isPurchasing;

        public PurchaseHandler(PurchaseService purchaseService, IIAPService iapService,
            PurchaseCommandFactory purchaseCommandFactory)
        {
            _purchaseService = purchaseService;
            _iapService = iapService;
            _purchaseCommandFactory = purchaseCommandFactory;

            Subscribe();
        }

        private void Subscribe()
        {
            _iapService.OnPurchasingSuccess += OnPurchasingSuccess;
            _iapService.OnPurchasingFailed += OnPurchasingFailed;
        }

        public void Purchase(OfferConfig offer, Action<bool> callback)
        {
            if (!_purchaseService.IsInitialized) return;
            
            if (_isPurchasing) return;

            if (!_iapService.GetProductWrapper(offer.Id).CanPurchase) return;

            _isPurchasing = true;
            _callback = callback;
            _currentOffer = offer;

            _iapService.Purchase(offer.Id);
        }

        private void OnPurchasingSuccess(string id)
        {
            if (!_purchaseService.IsInitialized) return;
            
            if (ProductIsNull(id) || OfferIsNull())
            {
                CompletePurchase(null);
                return;
            }
            
            _currentOffer.HandlePurchase(_purchaseCommandFactory);
            
            CompletePurchase(() => { _callback?.Invoke(true); });
        }

        private void OnPurchasingFailed(string id) => CompletePurchase(() => { _callback?.Invoke(false); });

        private void CompletePurchase(Action callback)
        {
            callback?.Invoke();

            _currentOffer = null;
            _callback = null;
            _isPurchasing = false;
        }

        private bool ProductIsNull(string id) => _purchaseService.GetProductWrapper(id).Equals(null);

        private bool OfferIsNull() => _currentOffer.Equals(null);
    }
}