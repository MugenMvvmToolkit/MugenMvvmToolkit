using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public sealed class AsyncRelayCommand<T> : RelayCommandBase<T>
    {
        #region Constructors

        public AsyncRelayCommand(Func<T, Task> execute, IReadOnlyMetadataContext? metadata = null)
            : base(execute, null, null, metadata)
        {
        }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
            : base(execute, canExecute, notifiers, metadata)
        {
        }

        #endregion
    }
}