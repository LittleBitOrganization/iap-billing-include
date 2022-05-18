using System;
using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Services.PurchaseProcessors
{
    public interface IPurchaseHandler
    {
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args, Action<bool> callback);
    }
}