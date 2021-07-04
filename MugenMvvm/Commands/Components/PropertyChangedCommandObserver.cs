using System;
using System.ComponentModel;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class PropertyChangedCommandObserver : MultiAttachableComponentBase<ICompositeCommand>, IHasDisposeCondition, IHasPriority,
        IDisposableComponent<ICompositeCommand>
    {
        private PropertyChangedEventHandler? _handler;

        public PropertyChangedCommandObserver()
        {
            IsDisposable = true;
        }

        public Func<object?, PropertyChangedEventArgs, bool>? CanNotify { get; set; }

        public bool IsDisposable { get; set; }

        public int Priority => CommandComponentPriority.PropertyChangedObserver;

        private PropertyChangedEventHandler PropertyChangedEventHandler => _handler ??= this.ToWeakReference().CommandNotifierOnPropertyChangedHandler;

        public ActionToken Add(INotifyPropertyChanged notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            notifier.PropertyChanged += PropertyChangedEventHandler;
            return ActionToken.FromDelegate((n, h) => ((INotifyPropertyChanged)n!).PropertyChanged -= (PropertyChangedEventHandler)h!, notifier, PropertyChangedEventHandler);
        }

        public void Dispose()
        {
            if (IsDisposable)
            {
                (_handler?.Target as IWeakReference)?.Release();
                _handler = null;
            }
        }

        internal void Handle(object? sender, PropertyChangedEventArgs message)
        {
            if (CanNotify == null || CanNotify(sender, message))
            {
                foreach (var owner in Owners)
                    owner.RaiseCanExecuteChanged();
            }
        }

        void IDisposableComponent<ICompositeCommand>.Dispose(ICompositeCommand owner, IReadOnlyMetadataContext? metadata) => Dispose();
    }
}