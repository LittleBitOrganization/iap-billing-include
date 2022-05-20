using System;
using System.IO;
using LittleBit.Modules.IAppModule.Data.Products;
using LittleBit.Modules.IAppModule.Services;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Data.ProductWrappers
{
    public partial class EditorProductWrapper : IProductWrapper
    {
        private readonly IIAPService _iapService;

        private const decimal DefaultPrice = 1;
        private const string DefaultName = "Sample Product";
        private const string DefaultDescription = "Sample Product Description";

        public ProductType Type { get; }
        public string Id { get; }
        public decimal LocalizedPrice => DefaultPrice;
        public string LocalizedTitle => DefaultName;
        public string LocalizedDescription => DefaultDescription;
        public string LocalizedPriceString => $@"{DefaultPrice}$";
        public bool CanPurchase => Type == ProductType.Consumable || !IsPurchased;

        public bool IsPurchased => PlayerPrefs.GetInt(GetPlayerPrefsKey(Constants.IsPurchasedKey), Constants.False) ==
                                   Constants.True;

        private Func<string, string> GetPlayerPrefsKey =>
            (key) => Path.Combine(Constants.PlayerPrefsKeyPrefix, Id, key);

        public EditorProductWrapper(ProductConfig productConfig)
        {
            Type = productConfig.ProductType;
            Id = productConfig.Id;
        }
        
        public void Purchase()
        {
            PlayerPrefs.SetInt(GetPlayerPrefsKey(Constants.IsPurchasedKey), Constants.True);
        }
        
    }
}