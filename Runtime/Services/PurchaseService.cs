using System;
using LittleBit.Modules.IAppModule.Commands.Factory;
using LittleBit.Modules.IAppModule.Data.ProductWrappers;
using LittleBit.Modules.IAppModule.Data.Purchases;

namespace LittleBit.Modules.IAppModule.Services
{
    public class PurchaseService : IService
    {
        private readonly IIAPService _iapService;
        private OfferConfig _currentOffer;

        private bool _isPurchasing;
        private Action<bool> _callback;
        private PurchaseCommandFactory _purchaseCommandFactory;

        public PurchaseService(IIAPService iapService, PurchaseCommandFactory purchaseCommandFactory)
        {
            _purchaseCommandFactory = purchaseCommandFactory;
            _iapService = iapService;

            _iapService.OnPurchasingSuccess += OnPurchasingSuccess;
            _iapService.OnPurchasingFailed += OnPurchasingFailed;
        }

        public void Purchase(OfferConfig offer, Action<bool> callback)
        {
            if (_isPurchasing) return;

            if (!_iapService.CreateProductWrapper(offer.Id).CanPurchase) return;
            
            _isPurchasing = true;
            _callback = callback;
            _currentOffer = offer;

            _iapService.Purchase(offer.Id);
        }

        public IProductWrapper CreateProductWrapper(string id)
        {
            return _iapService.CreateProductWrapper(id);
        }
        
        public IProductWrapper CreateProductWrapper(OfferConfig offerConfig)
        {
            return CreateProductWrapper(offerConfig.Id);
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