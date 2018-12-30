using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public sealed class RelayCommand<T> : RelayCommandBase<T>
    {
        #region Constructors

        public RelayCommand(Action<T> execute, IReadOnlyMetadataContext? context = null)
            : base(execute, null, null, context)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }


        public RelayCommand(Action<T> execute, Func<T, bool> canExecute, IReadOnlyMetadataContext? context, params object[] notifiers)
            : base(execute, canExecute, notifiers, context)
        {
        }

        #endregion
    }
}