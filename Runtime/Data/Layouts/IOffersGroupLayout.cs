using LittleBit.Modules.CoreModule.MonoInterfaces;
using LittleBit.Modules.IAppModule.Data.Purchases;

namespace LittleBit.Modules.IAppModule.Layouts
{
    public interface IOffersGroupLayout : ILayout
    {
        public ILayout GetContentLayout();
        public void SetData(OffersGroupConfig offersGroupConfig);
    }
}