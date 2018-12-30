using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public sealed class AsyncRelayCommand : RelayCommandBase<object>
    {
        #region Constructors

        public AsyncRelayCommand(Func<Task> execute, IReadOnlyMetadataContext? context = null)
            : base(execute, null, null, context)
        {
        }

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute, IReadOnlyMetadataContext? context, params object[] notifiers)
            : base(execute, canExecute, notifiers, context)
        {
        }

        public AsyncRelayCommand(Func<object, Task> execute, IReadOnlyMetadataContext? context = null)
            : base(execute, null, null, context)
        {
        }

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute, IReadOnlyMetadataContext? context, params object[] notifiers)
            : base(execute, canExecute, notifiers, context)
        {
        }

        #endregion
    }
}