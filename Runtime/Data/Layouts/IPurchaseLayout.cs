using System;
using LittleBit.Modules.IAppModule.Data.Purchases;

namespace LittleBit.Modules.IAppModule.Layouts
{
    [Obsolete]
    public interface IPurchaseLayout : ILayout
    {
        public event Action OnClickBuy;
        void SetData(OfferConfig offerConfig);

        void SetButtonInteractable(bool interactable);
    }
}