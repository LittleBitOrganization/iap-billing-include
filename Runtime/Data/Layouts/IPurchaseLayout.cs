using System;
using LittleBit.Modules.CoreModule.MonoInterfaces;
using LittleBit.Modules.IAppModule.Data.Purchases;

namespace LittleBit.Modules.IAppModule.Layouts
{
    public interface IPurchaseLayout : ILayout
    {
        public event Action OnClickBuy;
        void SetData(OfferConfig offerConfig);

        void SetButtonInteractable(bool interactable);
    }
}