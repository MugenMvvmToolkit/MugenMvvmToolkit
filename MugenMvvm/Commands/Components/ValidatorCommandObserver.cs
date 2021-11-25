using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class ValidatorCommandObserver : MultiAttachableComponentBase<ICompositeCommand>, IHasPriority, IDisposableComponent<ICompositeCommand>,
        IValidatorErrorsChangedListener
    {
        private readonly HashSet<IValidator> _observers;

        public ValidatorCommandObserver()
        {
            _observers = new HashSet<IValidator>(InternalEqualityComparer.Reference);
        }

        public int Priority => CommandComponentPriority.ValidatorObserver;

        public bool Add(IValidator notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            lock (_observers)
            {
                if (!_observers.Add(notifier))
                    return false;
                notifier.AddComponent(this);
                return true;
            }
        }

        public bool Contains(IValidator notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            lock (_observers)
            {
                return _observers.Contains(notifier);
            }
        }

        public bool Remove(IValidator notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            lock (_observers)
            {
                if (!_observers.Remove(notifier))
                    return false;
                return notifier.RemoveComponent(this);
            }
        }

        void IDisposableComponent<ICompositeCommand>.OnDisposing(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDisposableComponent<ICompositeCommand>.OnDisposed(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
            lock (_observers)
            {
                foreach (var observer in _observers)
                    observer.RemoveComponent(this);
                _observers.Clear();
            }
        }

        void IValidatorErrorsChangedListener.OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata)
        {
            foreach (var owner in Owners)
                owner.RaiseCanExecuteChanged();
        }
    }
}