using LittleBit.Modules.IAppModule.Commands;
using LittleBit.Modules.IAppModule.Commands.Factory;

namespace LittleBit.Modules.IAppModule.Data.Products
{
    public class SampleProductConfig : ProductConfig
    {
        public override void HandlePurchase(PurchaseCommandFactory purchaseCommandFactory)
        {
            purchaseCommandFactory.Create<SampleProductPurchaseCommand>().Execute();
        }
    }
}