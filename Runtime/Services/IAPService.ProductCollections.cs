using System.Collections.Generic;
using System.Linq;
using LittleBit.Modules.IAppModule.Data.Products;
using LittleBit.Modules.IAppModule.Data.ProductWrappers;
using LittleBit.Modules.IAppModule.Data.Purchases;
using UnityEngine.Purchasing;

namespace LittleBit.Modules.IAppModule.Services
{
    public partial class IAPService
    {
        public class ProductCollections
        {
            public Dictionary<string, ProductConfig> AllProducts { get; }
            private readonly Dictionary<string, EditorProductWrapper> _editorProductWrappers;
            private ProductCollection _productsCollection;

            public ProductCollections()
            {
                AllProducts = new Dictionary<string, ProductConfig>();
                _editorProductWrappers = new Dictionary<string, EditorProductWrapper>();
            }

            public void AddConfig(OfferConfig productConfig)
            {
                AddToAllProducts(productConfig);
                productConfig.Products.ToList().ForEach(AddToAllProducts);
            }

            private void AddToAllProducts(ProductConfig productConfig)
            {
                if (productConfig == null) return;

                if (string.IsNullOrEmpty(productConfig.Id)) return;

                if (AllProducts.ContainsKey(productConfig.Id)) return;
                
                AllProducts.Add(productConfig.Id, productConfig);
                
                CreateEditorProductWrapper(productConfig.Id);
            }

            public void AddUnityIAPProductCollection(ProductCollection productsCollection)
            {
                _productsCollection = productsCollection;
            }

            private void CreateEditorProductWrapper(string id)
            {
                if (!AllProducts.ContainsKey(id)) return;

                _editorProductWrappers.Add(id, new EditorProductWrapper(GetProductConfig(id)));
            }

            public EditorProductWrapper GetEditorProductWrapper(string id)
                => !_editorProductWrappers.ContainsKey(id) ? null : _editorProductWrappers[id];

            public ProductConfig GetProductConfig(string id)
                => !AllProducts.ContainsKey(id) ? null : AllProducts[id];

            public RuntimeProductWrapper GetRuntimeProductWrapper(string id)
                => _productsCollection == null ? null : new RuntimeProductWrapper(_productsCollection.WithID(id));
        }
    }
}