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

        public static IValidator? TryGetValidator<TRequest>(this IValidatorProviderComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetValidator(request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnValidatorCreated<TRequest>(this IValidatorProviderListener[] listeners, IValidationManager validatorProvider, IValidator validator, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(metadata, nameof(metadata));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnValidatorCreated(validatorProvider, validator, request, metadata);
        }

        public static void OnErrorsChanged(this IValidatorListener[] listeners, IValidator validator, object? target, string memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(memberName, nameof(memberName));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnErrorsChanged(validator, target, memberName, metadata);
        }

        public static void OnAsyncValidation(this IValidatorListener[] listeners, IValidator validator, object? target, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(memberName, nameof(memberName));
            Should.NotBeNull(validationTask, nameof(validationTask));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAsyncValidation(validator, target, memberName, validationTask, metadata);
        }

        public static void OnDisposed(this IValidatorListener[] listeners, IValidator validator)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validator, nameof(validator));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnDisposed(validator);
        }

        public static ItemOrList<object, IReadOnlyList<object>> TryGetErrors(this IValidatorComponent[] components, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetErrors(memberName, metadata);

            ItemOrList<object, List<object>> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetErrors(memberName, metadata));
            return result.Cast<IReadOnlyList<object>>();
        }

        public static IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>? TryGetErrors(this IValidatorComponent[] components, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetErrors(metadata);

            Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>? errors = null;
            for (var i = 0; i < components.Length; i++)
            {
                var dictionary = components[i].TryGetErrors(metadata);
                if (dictionary == null || dictionary.Count == 0)
                    continue;

                foreach (var keyValuePair in dictionary)
                {
                    if (keyValuePair.Value.IsNullOrEmpty())
                        continue;

                    if (errors == null)
                        errors = new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>();

                    errors.TryGetValue(keyValuePair.Key, out var list);
                    var editableList = list.Cast<List<object>>();
                    editableList.AddRange(keyValuePair.Value);
                    errors[keyValuePair.Key] = editableList.Cast<IReadOnlyList<object>>();
                }
            }

            return errors;
        }

        public static Task? TryValidateAsync(this IValidatorComponent[] components, string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 0)
                return null;
            if (components.Length == 1)
                return components[0].TryValidateAsync(memberName, cancellationToken, metadata);
            ItemOrList<Task, List<Task>> tasks = default;
            for (var i = 0; i < components.Length; i++)
                tasks.Add(components[i].TryValidateAsync(memberName, cancellationToken, metadata));
            return tasks.WhenAll();
        }

        public static void ClearErrors(this IValidatorComponent[] components, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
                components[i].ClearErrors(memberName, metadata);
        }

        public static bool HasErrors(this IValidatorComponent[] components, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].HasErrors(memberName, metadata))
                    return true;
            }

            return false;
        }

        #endregion
    }
}