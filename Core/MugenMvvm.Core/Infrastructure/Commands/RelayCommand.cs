using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Commands
{
    public sealed class RelayCommand : RelayCommandBase<object>
    {
        #region Constructors

        public RelayCommand(Action execute, IReadOnlyMetadataContext? metadata = null)
            : base(execute, null, null, metadata)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
            : base(execute, canExecute, notifiers, metadata)
        {
        }

        public RelayCommand(Action<object> execute, IReadOnlyMetadataContext? metadata = null)
            : base(execute, null, null, metadata)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers, null)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
            : base(execute, canExecute, notifiers, metadata)
        {
        }

        #endregion
    }
}