using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Commands.Components
{
    public sealed class ValidatorCommandObserver : CommandObserverBase<IValidator>, IHasPriority, IValidatorErrorsChangedListener
    {
        public int Priority => CommandComponentPriority.ValidatorObserver;

        protected override void OnAdded(IValidator notifier) => notifier.AddComponent(this);

        protected override void OnRemoved(IValidator notifier) => notifier.Components.Remove(this);

        void IValidatorErrorsChangedListener.OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata) =>
            RaiseCanExecuteChanged();
    }
}