using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class MappingValidatorDecorator : ComponentDecoratorBase<IValidator, IValidatorErrorManagerComponent>, IValidatorErrorManagerComponent,
        IValidatorAsyncValidationListener, IValidatorErrorsChangedListener, IComponentCollectionDecorator<IValidatorAsyncValidationListener>,
        IComponentCollectionDecorator<IValidatorErrorsChangedListener>
    {
        private readonly Dictionary<string, string> _mappingFromTo;
        private readonly Dictionary<string, string> _mappingToFrom;
        private ItemOrArray<IValidatorAsyncValidationListener> _asyncListeners;
        private ItemOrArray<IValidatorErrorsChangedListener> _listeners;

        public MappingValidatorDecorator() : this(ValidationComponentPriority.MappingValidatorDecorator)
        {
        }

        public MappingValidatorDecorator(int priority) : base(priority)
        {
            _mappingFromTo = new Dictionary<string, string>(3, StringComparer.Ordinal);
            _mappingToFrom = new Dictionary<string, string>(3, StringComparer.Ordinal);
        }

        public void Add(string from, string to)
        {
            Should.NotBeNull(from, nameof(from));
            lock (_mappingFromTo)
            {
                _mappingFromTo[from] = to;
                _mappingToFrom[to] = from;
            }
        }

        public bool Remove(string mapping)
        {
            Should.NotBeNull(mapping, nameof(mapping));
            lock (_mappingFromTo)
            {
                if (!_mappingFromTo.Remove(mapping, out mapping!))
                    return false;
                _mappingToFrom.Remove(mapping);
                return true;
            }
        }

        public void OnAsyncValidation(IValidator validator, string? member, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            _asyncListeners.OnAsyncValidation(validator, member, validationTask, metadata);
            if (string.IsNullOrEmpty(member))
                return;
            lock (_mappingFromTo)
            {
                _mappingFromTo.TryGetValue(member!, out member);
            }

            if (member != null)
                _asyncListeners.OnAsyncValidation(validator, member, validationTask, metadata);
        }

        public bool HasErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata)
        {
            var result = Components.HasErrors(validator, members, source, metadata);
            if (result || members.IsEmpty)
                return result;

            members = GetMappings(members, false);
            return !members.IsEmpty && Components.HasErrors(validator, members, source, metadata);
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source,
            IReadOnlyMetadataContext? metadata)
        {
            Components.GetErrors(validator, members, ref errors, source, metadata);
            if (members.IsEmpty)
                return;
            members = GetMappings(members, false);
            if (!members.IsEmpty)
                Components.GetErrors(validator, members, ref errors, source, metadata);
            if (errors.IsEmpty)
                return;

            lock (_mappingFromTo)
            {
                foreach (var error in errors)
                {
                    if (!error.IsEmpty && _mappingFromTo.TryGetValue(error.Member, out var mapping))
                        errors.Add(new ValidationErrorInfo(error.Target, mapping, error.Error));
                }
            }
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source, IReadOnlyMetadataContext? metadata)
        {
            Components.GetErrors(validator, members, ref errors, source, metadata);
            if (members.IsEmpty)
                return;
            members = GetMappings(members, false);
            if (!members.IsEmpty)
                Components.GetErrors(validator, members, ref errors, source, metadata);
        }

        public void SetErrors(IValidator validator, object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata) =>
            Components.SetErrors(validator, source, errors, metadata);

        public void ClearErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata) =>
            Components.ClearErrors(validator, members, source, metadata);

        public void OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata)
        {
            _listeners.OnErrorsChanged(validator, members, metadata);
            if (members.IsEmpty)
                return;
            members = GetMappings(members, true);
            if (!members.IsEmpty)
                _listeners.OnErrorsChanged(validator, members, metadata);
        }

        private ItemOrIReadOnlyList<string> GetMappings(ItemOrIReadOnlyList<string> members, bool fromToMapping)
        {
            var mappings = new ItemOrListEditor<string>(2);
            lock (_mappingFromTo)
            {
                var dict = fromToMapping ? _mappingFromTo : _mappingToFrom;
                foreach (var member in members)
                {
                    if (dict.TryGetValue(member, out var mapping))
                        mappings.Add(mapping);
                }
            }

            return mappings;
        }

        void IComponentCollectionDecorator<IValidatorAsyncValidationListener>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IValidatorAsyncValidationListener> components, IReadOnlyMetadataContext? metadata) =>
            _asyncListeners = this.Decorate(ref components);

        void IComponentCollectionDecorator<IValidatorErrorsChangedListener>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IValidatorErrorsChangedListener> components, IReadOnlyMetadataContext? metadata) =>
            _listeners = this.Decorate(ref components);
    }
}