using System;

namespace LittleBit.Modules.IAppModule.Commands.Factory
{
    public class PurchaseCommandFactory
    {
        private ICreator _creator;

        public PurchaseCommandFactory(ICreator creator)
        {
            _creator = creator;
            
        }

        public T Create<T>(object[] args = null) where T : IPurchaseCommand
        {
            var @params = args ?? Array.Empty<object>();

            return _creator.Instantiate<T>(@params);
        }
    }
}