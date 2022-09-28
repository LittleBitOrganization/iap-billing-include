using LittleBit.Modules.IAppModule.Commands.Factory;

namespace LittleBit.Modules.IAppModule.Data.Products
{
    public interface IProduct
    {
        void HandlePurchase(PurchaseCommandFactory purchaseCommandFactory);
    }
}