using System;

namespace LittleBit.Modules.IAppModule.Data.ProductWrappers
{
    public class TransactionData
    {
        public Func<bool> HasReceiptGetter { get; set; }
        public Func<string> ReceiptGetter { get; set; }
        public Func<string> TransactionIdGetter { get; set; }

        public bool HasReceipt => HasReceiptGetter.Invoke();

        public string Receipt => ReceiptGetter.Invoke();

        public string TransactionId => TransactionIdGetter.Invoke();
    }
}