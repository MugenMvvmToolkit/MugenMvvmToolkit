using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public sealed class AsyncRelayCommand<T> : RelayCommandBase<T>
    {
        #region Constructors

        public AsyncRelayCommand(Func<T, Task> execute, IReadOnlyMetadataContext? context = null)
            : base(execute, null, null, context)
        {
        }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute, IReadOnlyMetadataContext? context, params object[] notifiers)
            : base(execute, canExecute, notifiers, context)
        {
        }

        #endregion
    }
}