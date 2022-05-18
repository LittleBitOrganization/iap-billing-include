using System.Collections.Generic;
using LittleBit.Modules.IAppModule.Data.Purchases;
using UnityEngine;

namespace LittleBit.Modules.IAppModule.Data
{
    [CreateAssetMenu(order = 0, fileName = "Store Config", menuName = "Configs/Store Configs/StoreConfig")]
    public class StoreConfig : ScriptableObject
    {
        [SerializeField]
        private List<OffersGroupConfig> _groups;

        public IReadOnlyList<OffersGroupConfig> Groups => _groups;
    }
}