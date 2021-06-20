using System.ComponentModel;
using MugenMvvm.Collections;
using MugenMvvm.Components;
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
    public sealed class ObservableValidatorBehavior : MultiAttachableComponentBase<IValidator>, IValidatorErrorsChangedListener, IDisposableComponent<IValidator>, IHasPriority
    {
        private readonly INotifyPropertyChanged _target;

        public ObservableValidatorBehavior(INotifyPropertyChanged target)
        {
            _target = target;
            target.PropertyChanged += OnPropertyChanged;
        }

        public int Priority { get; set; }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            foreach (var owner in Owners)
                owner.ValidateAsync(e.PropertyName).LogException(UnhandledExceptionType.Validation);
        }

        void IDisposableComponent<IValidator>.Dispose(IValidator owner, IReadOnlyMetadataContext? metadata) => _target.PropertyChanged -= OnPropertyChanged;

        void IValidatorErrorsChangedListener.OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata)
        {
            if (_target is IHasService<IMessenger> hasService)
                hasService.GetService(true)?.Publish(validator, members.GetRawValue() ?? "", metadata);
        }
    }
}