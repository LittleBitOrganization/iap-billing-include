using System;

namespace LittleBit.Modules.IAppModule.Data.ProductWrappers
{
    public class TransactionData
    {
        internal Func<bool> HasReceiptGetter { get; set; }
        internal Func<string> ReceiptGetter { get; set; }
        internal Func<string> TransactionIdGetter { get; set; }

        public bool HasReceipt => HasReceiptGetter.Invoke();

        public string Receipt => ReceiptGetter.Invoke();

        public string TransactionId => TransactionIdGetter.Invoke();
    }
}