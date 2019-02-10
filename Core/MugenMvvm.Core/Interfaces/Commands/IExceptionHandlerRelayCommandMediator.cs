using System;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IExceptionHandlerRelayCommandMediator : IRelayCommandMediator//todo fix
    {
        bool OnExecuteFailed(Exception exception, object? parameter);

        bool OnCanExecuteFailed(Exception exception, object? parameter);
    }
}