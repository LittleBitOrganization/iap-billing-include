using System;
using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Services.TransactionsRestorers
{
    public class AppleTransactionsRestorer : ITransactionsRestorer
    {
        public void Restore(IExtensionProvider extensionProvider, Action<bool> callback)
        {
            extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(success =>
            {
                callback?.Invoke(success);
            });
        }
    }
}