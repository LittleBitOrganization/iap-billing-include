using System;

namespace LittleBit.Modules.IAppModule.Services
{
    public interface IIAPService
    {
        public event Action OnPurchasingSuccess;
        public event Action OnPurchasingFailed;
        public void Purchase(string id);
        public void RestorePurchasedProducts(Action<bool> callback);
        public ProductWrapper CreateProductWrapper(string id);
    }
}