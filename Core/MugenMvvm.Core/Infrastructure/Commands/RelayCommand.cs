using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public class RelayCommand : RelayCommandBase<object>
    {
        #region Constructors

        public RelayCommand(Action execute, IReadOnlyMetadataContext? context = null)
            : base(execute, null, null, context)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute, IReadOnlyMetadataContext? context, params object[] notifiers)
            : base(execute, canExecute, notifiers, context)
        {
        }

        public RelayCommand(Action<object> execute, IReadOnlyMetadataContext? context = null)
            : base(execute, null, null, context)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute, IReadOnlyMetadataContext? context, params object[] notifiers)
            : base(execute, canExecute, notifiers, context)
        {
        }

        #endregion
    }
}