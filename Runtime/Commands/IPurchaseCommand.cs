namespace LittleBit.Modules.IAppModule.Commands
{
    public interface IPurchaseCommand
    {
        public void Execute();
        public void Undo();
    }
}