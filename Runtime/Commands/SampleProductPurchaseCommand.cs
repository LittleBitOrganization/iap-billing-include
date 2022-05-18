using UnityEngine;

namespace LittleBit.Modules.IAppModule.Commands
{
    public class SampleProductPurchaseCommand : IPurchaseCommand
    {
        public void Execute()
        {
            PurchaseSampleProduct(true);
        }

        public void Undo()
        {
            PurchaseSampleProduct(false);
        }

        private void PurchaseSampleProduct(bool purchased)
        {
            PlayerPrefs.SetInt("SampleProductPurchased", purchased ? 1 : 0);
        }
    }
}