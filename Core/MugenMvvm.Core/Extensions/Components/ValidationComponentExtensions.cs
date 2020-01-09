using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class ValidationComponentExtensions
    {
        #region Methods

        public static IReadOnlyList<IValidator>? TryGetValidators<TRequest>(this IValidatorProviderComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            LazyList<IValidator> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetValidators(request, metadata));
            return result.List;
        }

        public static IAggregatorValidator? TryGetAggregatorValidator<TRequest>(this IAggregatorValidatorProviderComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetAggregatorValidator(request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnValidatorCreated<TRequest>(this IValidatorProviderListener[] listeners, IValidatorProvider validatorProvider, IValidator validator, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(metadata, nameof(metadata));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnValidatorCreated(validatorProvider, validator, request, metadata);
        }

        public static void OnAggregatorValidatorCreated<TRequest>(this IValidatorProviderListener[] listeners, IValidatorProvider validatorProvider, IAggregatorValidator validator, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(metadata, nameof(metadata));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAggregatorValidatorCreated(validatorProvider, validator, request, metadata);
        }

        public static void OnErrorsChanged(this IValidatorListener[] listeners, IValidator validator, string memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(memberName, nameof(memberName));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnErrorsChanged(validator, memberName, metadata);
        }

        public static void OnAsyncValidation(this IValidatorListener[] listeners, IValidator validator, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(memberName, nameof(memberName));
            Should.NotBeNull(validationTask, nameof(validationTask));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAsyncValidation(validator, memberName, validationTask, metadata);
        }

        public static void OnDisposed(this IValidatorListener[] listeners, IValidator validator)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validator, nameof(validator));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnDisposed(validator);
        }

        public static IReadOnlyList<object>? TryGetErrors(this IValidator[] components, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            LazyList<object> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].GetErrors(memberName, metadata));
            return result.List;
        }

        public static IReadOnlyDictionary<string, IReadOnlyList<object>>? TryGetErrors(this IValidator[] components, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Dictionary<string, IReadOnlyList<object>>? errors = null;
            for (var i = 0; i < components.Length; i++)
            {
                var dictionary = components[i].GetErrors(metadata);
                if (dictionary == null || dictionary.Count == 0)
                    continue;

                foreach (var keyValuePair in dictionary)
                {
                    if (keyValuePair.Value.Count == 0)
                        continue;

                    if (errors == null)
                        errors = new Dictionary<string, IReadOnlyList<object>>();

                    if (!errors.TryGetValue(keyValuePair.Key, out var list))
                    {
                        list = new List<object>();
                        errors[keyValuePair.Key] = list;
                    }

                    ((List<object>)list).AddRange(keyValuePair.Value);
                }
            }

            return errors;
        }

        public static Task ValidateAsync(this IValidator[] components, string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 0)
                return Default.CompletedTask;
            if (components.Length == 1)
                return components[0].ValidateAsync(memberName, cancellationToken, metadata);

            var tasks = new Task[components.Length];
            for (var i = 0; i < components.Length; i++)
                tasks[i] = components[i].ValidateAsync(memberName, cancellationToken, metadata);
            return Task.WhenAll(tasks);
        }

        public static void ClearErrors(this IValidator[] components, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
                components[i].ClearErrors(memberName, metadata);
        }

        public static bool HasErrors(this IValidator[] components)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].HasErrors)
                    return true;
            }

            return false;
        }

        #endregion
    }
}