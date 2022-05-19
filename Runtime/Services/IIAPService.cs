using System;
using LittleBit.Modules.IAppModule.Data.ProductWrappers;

namespace LittleBit.Modules.IAppModule.Services
{
    public interface IIAPService
    {
        public event Action<string> OnPurchasingSuccess;
        public event Action<string> OnPurchasingFailed;
        public void Purchase(string id);
        public void RestorePurchasedProducts(Action<bool> callback);
        public IProductWrapper CreateProductWrapper(string id);
    }
}