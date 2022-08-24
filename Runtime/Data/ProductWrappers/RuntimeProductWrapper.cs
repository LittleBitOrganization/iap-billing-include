using System.Globalization;
using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Data.ProductWrappers
{
    public class RuntimeProductWrapper : ProductWrapper
    {
        public RuntimeProductWrapper(Product product)
        {
            TransactionData = new()
            {
                HasReceiptGetter = () => product.hasReceipt,
                ReceiptGetter = () => product.receipt,
                TransactionIdGetter = () => product.transactionID
            };

            Metadata = new()
            {
                CurrencyCode = product.metadata.isoCurrencyCode,
                CurrencySymbol = RegionInfo.CurrentRegion.CurrencySymbol,
                LocalizedDescription = product.metadata.localizedDescription,
                LocalizedPrice = product.metadata.localizedPrice,
                LocalizedPriceString = product.metadata.localizedPriceString,
                LocalizedTitle = product.metadata.localizedTitle,
                CanPurchaseGetter = () =>
                    Definition.Type.Equals(ProductType.Consumable) ||
                    !Metadata.IsPurchasedGetter.Invoke(),
                IsPurchasedGetter = () => TransactionData.HasReceipt ||
                                          !string.IsNullOrEmpty(TransactionData.Receipt) ||
                                          !string.IsNullOrEmpty(TransactionData.TransactionId)
            };

            Definition = new()
            {
                Id = product.definition.id,
                Type = product.definition.type
            };
        }
    }
}