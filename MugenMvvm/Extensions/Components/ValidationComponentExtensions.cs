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

        public static IValidator? TryGetValidator(this IValidatorProviderComponent[] components, IValidationManager validationManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(validationManager, nameof(validationManager));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetValidator(validationManager, request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnValidatorCreated(this IValidatorProviderListener[] listeners, IValidationManager validationManager, IValidator validator, object? request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(validationManager, nameof(validationManager));
            Should.NotBeNull(validator, nameof(validator));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnValidatorCreated(validationManager, validator, request, metadata);
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

        public static ItemOrList<object, IReadOnlyList<object>> TryGetErrors(this IValidatorComponent[] components, IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(validator, nameof(validator));
            if (components.Length == 1)
                return components[0].TryGetErrors(validator, memberName, metadata);

            var result = ItemOrListEditor.Get<object>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetErrors(validator, memberName, metadata));
            return result.ToItemOrList<IReadOnlyList<object>>();
        }

        public static IReadOnlyDictionary<string, object>? TryGetErrors(this IValidatorComponent[] components, IValidator validator, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(validator, nameof(validator));
            if (components.Length == 1)
                return components[0].TryGetErrors(validator, metadata);

            Dictionary<string, object>? errors = null;
            for (var i = 0; i < components.Length; i++)
            {
                var dictionary = components[i].TryGetErrors(validator, metadata);
                if (dictionary == null || dictionary.Count == 0)
                    continue;

                errors ??= new Dictionary<string, object>();
                foreach (var error in dictionary)
                {
                    if (!errors.TryGetValue(error.Key, out var currentError))
                    {
                        errors[error.Key] = error.Value;
                        continue;
                    }

                    if (currentError is ErrorList list)
                        list.AddError(error.Value);
                    else
                    {
                        list = new ErrorList();
                        list.AddError(currentError);
                        list.AddError(error.Value);
                        errors[error.Key] = list;
                    }
                }
            }

            return errors;
        }

        public static Task TryValidateAsync(this IValidatorComponent[] components, IValidator validator, string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(validator, nameof(validator));
            return components.InvokeAllAsync((validator, memberName, metadata), (component, s, c) => component.TryValidateAsync(s.validator, s.memberName, c, s.metadata), cancellationToken);
        }

        public static void ClearErrors(this IValidatorComponent[] components, IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
                components[i].ClearErrors(validator, memberName, metadata);
        }

        public static bool HasErrors(this IValidatorComponent[] components, IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].HasErrors(validator, memberName, metadata))
                    return true;
            }

            return false;
        }

        #endregion

        #region Nested types

        private sealed class ErrorList : List<object>
        {
            #region Constructors

            public ErrorList() : base(2)
            {
            }

            #endregion

            #region Methods

            public void AddError(object error)
            {
                if (error is IEnumerable<object> errors)
                    AddRange(errors);
                else
                    Add(error);
            }

            #endregion
        }

        #endregion
    }
}