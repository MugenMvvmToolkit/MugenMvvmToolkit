using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation
{
    public sealed class Validator : ComponentOwnerBase<IValidator>, IValidator, IHasComponentAddingHandler, IHasDisposeCondition
    {
        private const int DefaultState = 0;
        private const int NoDisposeState = 1;
        private const int DisposedState = 2;

        private IReadOnlyMetadataContext? _metadata;
        private int _state;

        [Preserve(Conditional = true)]
        public Validator(IReadOnlyMetadataContext? metadata = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _metadata = metadata;
        }

        public bool IsDisposed => _state == DisposedState;

        public bool IsDisposable
        {
            get => _state == DefaultState;
            set
            {
                if (value)
                    Interlocked.CompareExchange(ref _state, DefaultState, NoDisposeState);
                else
                    Interlocked.CompareExchange(ref _state, NoDisposeState, DefaultState);
            }
        }

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata as IMetadataContext ?? MugenExtensions.EnsureInitialized(ref _metadata);

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _state, DisposedState, DefaultState) == DefaultState)
            {
                base.GetComponents<IDisposable>().Dispose();
                this.ClearComponents();
                this.ClearMetadata(true);
            }
        }

        public Task ValidateAsync(string? member = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IValidationHandlerComponent>().TryValidateAsync(this, member, cancellationToken, metadata);

        public bool HasErrors(ItemOrIReadOnlyList<string> members = default, object? source = null, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IValidatorErrorManagerComponent>().HasErrors(this, members, source, metadata);

        public void GetErrors(ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source = null,
            IReadOnlyMetadataContext? metadata = null) => GetComponents<IValidatorErrorManagerComponent>().GetErrors(this, members, ref errors, source, metadata);

        public void GetErrors(ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source = null,
            IReadOnlyMetadataContext? metadata = null) => GetComponents<IValidatorErrorManagerComponent>().GetErrors(this, members, ref errors, source, metadata);

        public void SetErrors(object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IValidatorErrorManagerComponent>().SetErrors(this, source, errors, metadata);

        public void ResetErrors(object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IValidatorErrorManagerComponent>().ResetErrors(this, source, errors, metadata);

        public void ClearErrors(ItemOrIReadOnlyList<string> members = default, object? source = null, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IValidatorErrorManagerComponent>().ClearErrors(this, members, source, metadata);

        private new ItemOrArray<TComponent> GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class => IsDisposed ? default : base.GetComponents<TComponent>(metadata);

        bool IHasComponentAddingHandler.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;
    }
}