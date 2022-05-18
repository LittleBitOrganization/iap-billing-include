using LittleBit.Modules.IAppModule.Commands.Factory;
using UnityEngine;

namespace LittleBit.Modules.IAppModule.Data.Products
{
    public abstract class ProductConfig : ScriptableObject
    {
        public abstract void HandlePurchase(PurchaseCommandFactory purchaseCommandFactory);
    }
}