using System;
using System.IO;
using System.Linq;
using LittleBit.Modules.IAppModule.Data.Products;
using LittleBit.Modules.IAppModule.Services;
using UnityEngine;
using UnityEngine.Purchasing;
using Random = System.Random;

namespace LittleBit.Modules.IAppModule.Data.ProductWrappers
{
    public partial class EditorProductWrapper : ProductWrapper
    {
        private readonly IIAPService _iapService;
        private static readonly Random _random = new Random();

        private const decimal DefaultPrice = 1;
        private const string DefaultTitle = "Sample Product";
        private const string DefaultDescription = "Sample Product Description";

        public bool IsPurchased => PlayerPrefs.GetInt(GetPlayerPrefsKey(Constants.IsPurchasedKey), Constants.False) ==
                                   Constants.True;

        private Func<string, string> GetPlayerPrefsKey =>
            (key) => Path.Combine(Constants.PlayerPrefsKeyPrefix, Definition.Id, key);

        public EditorProductWrapper(ProductConfig productConfig)
        {
            Definition = new()
            {
                Id = productConfig.Id,
                Type = productConfig.ProductType
            };

            Metadata = new()
            {
                LocalizedDescription = DefaultDescription,
                LocalizedTitle = DefaultTitle,
                LocalizedPrice = DefaultPrice,
                LocalizedPriceString = "USD" + DefaultPrice,
                IsPurchasedGetter = () =>
                    PlayerPrefs.GetInt(GetPlayerPrefsKey(Constants.IsPurchasedKey), Constants.False).Equals(Constants.True),
                CanPurchaseGetter = () => Definition.Type.Equals(ProductType.Consumable) || !IsPurchased
            };

            TransactionData = new()
            {
                TransactionIdGetter = () => IsPurchased ? GetRandomString(8) : String.Empty,
                ReceiptGetter = () => IsPurchased ? "receipt_" + GetRandomString(6) : String.Empty,
                HasReceiptGetter = () => IsPurchased
            };
        }

        private static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public void Purchase()
        {
            PlayerPrefs.SetInt(GetPlayerPrefsKey(Constants.IsPurchasedKey), Constants.True);
        }
    }
}