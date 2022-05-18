using System;
using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Services.PurchaseProcessors
{
    public class EditorPurchaseHandler : IPurchaseHandler
    {
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args, Action<bool> callback)
        {
            callback?.Invoke(true);
            return PurchaseProcessingResult.Complete;
        }
    }
}