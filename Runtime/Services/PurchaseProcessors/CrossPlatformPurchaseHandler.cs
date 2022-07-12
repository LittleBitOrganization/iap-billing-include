using System;
using com.adjust.sdk.purchase;
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
#if UNITY_IOS
                AdjustPurchase.VerifyPurchaseiOS(args.purchasedProduct.receipt,
                    args.purchasedProduct.transactionID,
                    args.purchasedProduct.definition.storeSpecificId,
                    delegate(ADJPVerificationInfo info) { VerificationInfoDelegate(info, callback); });

#endif

#if UNITY_ANDROID
                AdjustPurchase.VerifyPurchaseAndroid(
                    args.purchasedProduct.definition.id,
                    args.purchasedProduct.transactionID,
                    "hello, world",
                    delegate(ADJPVerificationInfo info) { VerificationInfoDelegate(info, callback); });
#endif

                return PurchaseProcessingResult.Complete;
            }
            catch (Exception e)
            {
                Debug.LogError("Invalid receipt!");

                callback?.Invoke(false);

                return PurchaseProcessingResult.Complete;
            }
        }

        private void VerificationInfoDelegate(ADJPVerificationInfo verificationInfo, Action<bool> purchaseCallback)
        {
            switch (verificationInfo.VerificationState)
            {
                case null:
                    purchaseCallback?.Invoke(false);
                    return;
                case ADJPVerificationState.ADJPVerificationStatePassed:
                    purchaseCallback?.Invoke(true);
                    return;
                default:
                    purchaseCallback?.Invoke(false);
                    break;
            }
        }
    }
}