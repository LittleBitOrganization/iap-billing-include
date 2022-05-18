using System;
using LittleBit.Modules.IAppModule.Data;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace LittleBit.Modules.IAppModule.Services.PurchaseProcessors
{
    public class CrossPlatformPurchaseHandler : IPurchaseHandler
    {
        private readonly CrossPlatformTangles _crossPlatformTangles;


        public CrossPlatformPurchaseHandler(CrossPlatformTangles crossPlatformTangles)
        {
            _crossPlatformTangles = crossPlatformTangles;
        }
        
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args,
            Action<bool> callback)
        {
            try
            {
                var validator =
                    new CrossPlatformValidator(_crossPlatformTangles.GetGoogleData(), _crossPlatformTangles.GetAppleData(), Application.identifier);

                validator.Validate(args.purchasedProduct.receipt);

                callback?.Invoke(true);
                
                return PurchaseProcessingResult.Complete;
            }
            catch (IAPSecurityException ex)
            {
                Debug.LogError("Invalid receipt: " + ex);
                
                callback?.Invoke(false);

                return PurchaseProcessingResult.Complete;
            }
        }
    }
}