using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Validation;

namespace MugenMvvm.Extensions.Components
{
    public static class ValidationComponentExtensions
    {
        public static IValidator? TryGetValidator(this ItemOrArray<IValidatorProviderComponent> components, IValidationManager validationManager,
            ItemOrIReadOnlyList<object> targets, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validationManager, nameof(validationManager));
            foreach (var c in components)
            {
                var result = c.TryGetValidator(validationManager, targets, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnValidatorCreated(this ItemOrArray<IValidationManagerListener> listeners, IValidationManager validationManager, IValidator validator,
            ItemOrIReadOnlyList<object> targets, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validationManager, nameof(validationManager));
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in listeners)
                c.OnValidatorCreated(validationManager, validator, targets, metadata);
        }

        public static void OnErrorsChanged(this ItemOrArray<IValidatorErrorsChangedListener> listeners, IValidator validator, ItemOrIReadOnlyList<string> members,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in listeners)
                c.OnErrorsChanged(validator, members, metadata);
        }

        public static void OnAsyncValidation(this ItemOrArray<IValidatorAsyncValidationListener> listeners, IValidator validator, string? member, Task validationTask,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(validationTask, nameof(validationTask));
            foreach (var c in listeners)
                c.OnAsyncValidation(validator, member, validationTask, metadata);
        }

        public static Task TryValidateAsync(this ItemOrArray<IValidationHandlerComponent> components, IValidator validator, string? member,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            return components.InvokeAllAsync((validator, member), cancellationToken, metadata,
                (component, s, c, m) => component.TryValidateAsync(s.validator, s.member, c, m));
        }

        public static void GetErrors(this ItemOrArray<IValidatorErrorManagerComponent> components, IValidator validator,
            ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in components)
                c.GetErrors(validator, members, ref errors, source, metadata);
        }

        public static void GetErrors(this ItemOrArray<IValidatorErrorManagerComponent> components, IValidator validator,
            ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in components)
                c.GetErrors(validator, members, ref errors, source, metadata);
        }

        public static void SetErrors(this ItemOrArray<IValidatorErrorManagerComponent> components, IValidator validator,
            object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(source, nameof(source));
            foreach (var c in components)
                c.SetErrors(validator, source, errors, metadata);
        }

        public static void ClearErrors(this ItemOrArray<IValidatorErrorManagerComponent> components, IValidator validator, ItemOrIReadOnlyList<string> members,
            object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in components)
                c.ClearErrors(validator, members, source, metadata);
        }

        public static bool HasErrors(this ItemOrArray<IValidatorErrorManagerComponent> components, IValidator validator, ItemOrIReadOnlyList<string> members,
            object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(validator, nameof(validator));
            foreach (var c in components)
            {
                if (c.HasErrors(validator, members, source, metadata))
                    return true;
            }

            return false;
        }
    }
}