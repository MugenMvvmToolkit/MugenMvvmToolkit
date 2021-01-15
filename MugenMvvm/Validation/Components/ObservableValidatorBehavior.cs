using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class ObservableValidatorBehavior : MultiAttachableComponentBase<IValidator>, IValidatorListener, IDisposable, IHasPriority
    {
        private readonly INotifyPropertyChanged _target;

        public ObservableValidatorBehavior(INotifyPropertyChanged target)
        {
            _target = target;
            target.PropertyChanged += OnPropertyChanged;
        }

        public int Priority { get; set; }

        public void Dispose() => _target.PropertyChanged -= OnPropertyChanged;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var owner in Owners)
            {
                owner.ValidateAsync(e.PropertyName).ContinueWith(task => MugenService.Application.OnUnhandledException(task.Exception!, UnhandledExceptionType.Validation),
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        void IValidatorListener.OnErrorsChanged(IValidator validator, object? target, string memberName, IReadOnlyMetadataContext? metadata)
        {
            if (_target is IHasService<IMessenger> hasService)
                hasService.ServiceOptional?.Publish(validator, memberName, metadata);
        }

        void IValidatorListener.OnAsyncValidation(IValidator validator, object? target, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
        }

        void IValidatorListener.OnDisposed(IValidator validator)
        {
        }
    }
}