using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public sealed class AsyncRelayCommand : RelayCommandBase<object>
    {
        #region Constructors

        public AsyncRelayCommand(Func<Task> execute, IReadOnlyMetadataContext? metadata = null)
            : base(execute, null, null, metadata)
        {
        }

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
            : base(execute, canExecute, notifiers, metadata)
        {
        }

        public AsyncRelayCommand(Func<object, Task> execute, IReadOnlyMetadataContext? metadata = null)
            : base(execute, null, null, metadata)
        {
        }

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
            : base(execute, canExecute, notifiers, metadata)
        {
        }

        #endregion
    }
}