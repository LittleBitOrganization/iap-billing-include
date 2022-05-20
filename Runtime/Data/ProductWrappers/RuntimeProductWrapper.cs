using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Data.ProductWrappers
{
    public class RuntimeProductWrapper : IProductWrapper
    {
        private readonly Product _product;
        public RuntimeProductWrapper(Product product) => _product = product;
        public ProductType Type => _product.definition.type;
        public string Id => _product.definition.id;
        public decimal LocalizedPrice => _product.metadata.localizedPrice;
        public string LocalizedTitle => _product.metadata.localizedTitle;
        public string LocalizedDescription => _product.metadata.localizedDescription;
        public string LocalizedPriceString => _product.metadata.localizedPriceString;
        public bool CanPurchase => _product.definition.type == ProductType.Consumable || !IsPurchased;

        public bool IsPurchased => _product.hasReceipt ||
                                   !string.IsNullOrEmpty(_product.receipt) ||
                                   !string.IsNullOrEmpty(_product.transactionID);
    }
}