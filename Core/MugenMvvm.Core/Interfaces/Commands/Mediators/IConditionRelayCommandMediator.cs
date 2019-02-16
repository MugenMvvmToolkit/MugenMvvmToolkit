namespace MugenMvvm.Interfaces.Commands.Mediators
{
    public interface IConditionRelayCommandMediator : IRelayCommandMediator
    {
        bool HasCanExecute();

        bool CanExecute(object? parameter);
    }
}