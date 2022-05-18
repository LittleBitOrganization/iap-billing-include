using System;
using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Services.TransactionsRestorers
{
    public class GooglePlayTransactionsRestorer : ITransactionsRestorer
    {
        public void Restore(IExtensionProvider extensionProvider, Action<bool> callback)
        {
            extensionProvider.GetExtension<IGooglePlayStoreExtensions>().RestoreTransactions(success =>
            {
                callback?.Invoke(success);
            });
        }
    }
}