using System;
using LittleBit.Modules.IAppModule.Commands.Factory;
using LittleBit.Modules.IAppModule.Data.ProductWrappers;
using LittleBit.Modules.IAppModule.Data.Purchases;

namespace LittleBit.Modules.IAppModule.Services
{
    public class PurchaseService : IService
    {
        public event Action OnIAPServiceInitialized;

        private readonly IIAPService _iapService;

        private readonly PurchaseCommandFactory _purchaseCommandFactory;

        private OfferConfig _currentOffer;

        private Action<bool> _callback;

        private bool _isPurchasing;


        public PurchaseService(IIAPService iapService, PurchaseCommandFactory purchaseCommandFactory)
        {
            _purchaseCommandFactory = purchaseCommandFactory;
            _iapService = iapService;

            _iapService.OnPurchasingSuccess += OnPurchasingSuccess;
            _iapService.OnPurchasingFailed += OnPurchasingFailed;
            _iapService.OnInitializationComplete += () => OnIAPServiceInitialized?.Invoke();
        }

        public void Purchase(OfferConfig offer, Action<bool> callback)
        {
            if (_isPurchasing) return;

            if (!_iapService.GetProductWrapper(offer.Id).CanPurchase) return;

            _isPurchasing = true;
            _callback = callback;
            _currentOffer = offer;

            _iapService.Purchase(offer.Id);
        }

        public IProductWrapper GetProductWrapper(string id)
        {
            return _iapService.GetProductWrapper(id);
        }

        public IProductWrapper GetProductWrapper(OfferConfig offerConfig)
        {
            return GetProductWrapper(offerConfig.Id);
        }

        private void OnPurchasingFailed(string id)
        {
            _callback?.Invoke(false);
            _callback = null;

            _isPurchasing = false;
        }

        private void OnPurchasingSuccess(string id)
        {
            _currentOffer.HandlePurchase(_purchaseCommandFactory);

            _callback?.Invoke(true);
            _callback = null;

            _isPurchasing = false;
        }
    }
}