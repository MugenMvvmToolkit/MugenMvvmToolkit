using System.ComponentModel;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class PropertyChangedValidatorObserver : MultiAttachableComponentBase<IValidator>, IValidatorErrorsChangedListener, IDisposableComponent<IValidator>, IHasPriority
    {
        private readonly INotifyPropertyChanged _target;

        public PropertyChangedValidatorObserver(INotifyPropertyChanged target, int priority = ValidationComponentPriority.PropertyChangedObserver)
        {
            Should.NotBeNull(target, nameof(target));
            _target = target;
            Priority = priority;
            target.PropertyChanged += OnPropertyChanged;
        }

        public int Priority { get; init; }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            foreach (var owner in Owners)
                owner.Validate(e.PropertyName);
        }

        void IDisposableComponent<IValidator>.OnDisposing(IValidator owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDisposableComponent<IValidator>.OnDisposed(IValidator owner, IReadOnlyMetadataContext? metadata) => _target.PropertyChanged -= OnPropertyChanged;

        void IValidatorErrorsChangedListener.OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata)
        {
            if (_target is IHasService<IMessenger> hasService)
                hasService.GetService(true)?.Publish(validator, members.GetRawValue() ?? "", metadata);
        }
    }
}