namespace MugenMvvm.Interfaces.Commands
{
    public interface IConditionRelayCommandMediator : IRelayCommandMediator
    {
        bool HasCanExecute();

        bool CanExecute(object? parameter);
    }
}