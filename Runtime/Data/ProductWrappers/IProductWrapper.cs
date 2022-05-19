using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Data.ProductWrappers
{
    public interface IProductWrapper
    {
        public ProductType Type { get; }
        public string Id { get; }
        public decimal LocalizedPrice { get; }
        public bool CanPurchase { get; }
        public bool IsPurchased { get; }
    }
}