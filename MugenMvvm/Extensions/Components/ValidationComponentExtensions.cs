using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ValidationComponentExtensions
    {
        public static IValidator? TryGetValidator(this ItemOrArray<IValidatorProviderComponent> components, IValidationManager validationManager, object? request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validationManager, nameof(validationManager));
            foreach (var c in components)
            {
                var result = c.TryGetValidator(validationManager, request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnValidatorCreated(this ItemOrArray<IValidatorProviderListener> listeners, IValidationManager validationManager, IValidator validator, object? request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validationManager, nameof(validationManager));
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in listeners)
                c.OnValidatorCreated(validationManager, validator, request, metadata);
        }

        public static void OnErrorsChanged(this ItemOrArray<IValidatorListener> listeners, IValidator validator, object? target, string memberName,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(memberName, nameof(memberName));
            foreach (var c in listeners)
                c.OnErrorsChanged(validator, target, memberName, metadata);
        }

        public static void OnAsyncValidation(this ItemOrArray<IValidatorListener> listeners, IValidator validator, object? target, string memberName, Task validationTask,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(memberName, nameof(memberName));
            Should.NotBeNull(validationTask, nameof(validationTask));
            foreach (var c in listeners)
                c.OnAsyncValidation(validator, target, memberName, validationTask, metadata);
        }

        public static void OnDisposed(this ItemOrArray<IValidatorListener> listeners, IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in listeners)
                c.OnDisposed(validator);
        }

        public static ItemOrIReadOnlyList<object> TryGetErrors(this ItemOrArray<IValidatorComponent> components, IValidator validator, string? memberName,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetErrors(validator, memberName, metadata);

            var result = new ItemOrListEditor<object>();
            foreach (var c in components)
                result.AddRange(c.TryGetErrors(validator, memberName, metadata));

            return result.ToItemOrList();
        }

        public static IReadOnlyDictionary<string, object>? TryGetErrors(this ItemOrArray<IValidatorComponent> components, IValidator validator, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            if (components.Count == 0)
                return null;
            if (components.Count == 1)
                return components[0].TryGetErrors(validator, metadata);

            Dictionary<string, object>? errors = null;
            foreach (var c in components)
            {
                var dictionary = c.TryGetErrors(validator, metadata);
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

        public static Task TryValidateAsync(this ItemOrArray<IValidatorComponent> components, IValidator validator, string? memberName, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            return components.InvokeAllAsync((validator, memberName), cancellationToken, metadata,
                (component, s, c, m) => component.TryValidateAsync(s.validator, s.memberName, c, m));
        }

        public static void ClearErrors(this ItemOrArray<IValidatorComponent> components, IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in components)
                c.ClearErrors(validator, memberName, metadata);
        }

        public static bool HasErrors(this ItemOrArray<IValidatorComponent> components, IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in components)
                if (c.HasErrors(validator, memberName, metadata))
                    return true;

            return false;
        }

        private sealed class ErrorList : List<object>
        {
            public ErrorList() : base(2)
            {
            }

            public void AddError(object error)
            {
                if (error is IEnumerable<object> errors)
                    AddRange(errors);
                else
                    Add(error);
            }
        }
    }
}