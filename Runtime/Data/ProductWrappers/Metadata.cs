using System;

namespace LittleBit.Modules.IAppModule.Data.ProductWrappers
{
    public class Metadata
    {
        public decimal LocalizedPrice { get; internal set; }

        public string LocalizedTitle { get; internal set; }

        public string LocalizedDescription { get; internal set; }

        public string LocalizedPriceString { get; internal set; }
        
        public string CurrencyCode { get; internal set; }
        
        public string CurrencySymbol { get; internal set; }

        public Func<bool> CanPurchaseGetter { get; internal set; }
        public Func<bool> IsPurchasedGetter { get; internal set; }

        public bool CanPurchase => CanPurchaseGetter.Invoke();
        public bool IsPurchased => IsPurchasedGetter.Invoke();
    }
}