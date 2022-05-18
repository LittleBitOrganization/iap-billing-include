using System;
using System.Collections.Generic;
using LittleBit.Modules.IAppModule.Layouts;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace LittleBit.Modules.IAppModule.Data.Purchases
{
    [CreateAssetMenu(order = 0, fileName = "ProductGroup Config", menuName = "Configs/Store Configs/GroupConfig")]
    [Serializable]
    public class OffersGroupConfig : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField, TextArea(1, 3)] private string _description;

        [SerializeField, ShowAssetPreview(64, 64)]
        private Sprite _icon;

        [FormerlySerializedAs("_purchases")] [SerializeField] private List<OfferConfig> offers;
        [SerializeField] private IPurchaseGroupInterfaceContainer _layout;
        
        public IPurchaseGroupInterfaceContainer Layout => _layout;
        public Sprite Icon => _icon;
        public IReadOnlyList<OfferConfig> Offers => offers;
        public string Title => _title;
        public string Description => _description;
    }
}