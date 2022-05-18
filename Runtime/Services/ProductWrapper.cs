using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Services
{
    public class ProductWrapper
    {
        private readonly Product _product;

        public ProductWrapper(Product product)
        {
            _product = product;
        }
        
        public string Id => _product.definition.id;
        public decimal LocalizedPrice => _product.metadata.localizedPrice;

        public bool IsBought() => _product.hasReceipt ||
                                  !string.IsNullOrEmpty(_product.receipt) ||
                                  !string.IsNullOrEmpty(_product.transactionID);

        public bool CanBuy() => _product.definition.type == ProductType.Consumable || !IsBought();
    }
}