using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class CycleHandlerValidatorBehavior : ComponentDecoratorBase<IValidator, IValidationHandlerComponent>, IValidationHandlerComponent
    {
        private readonly HashSet<string> _validatingMembers;

        public CycleHandlerValidatorBehavior(int priority = ValidationComponentPriority.CycleHandlerDecorator) : base(priority)
        {
            _validatingMembers = new HashSet<string>(StringComparer.Ordinal);
        }

        public Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            member ??= "";
            try
            {
                lock (_validatingMembers)
                {
                    if (!_validatingMembers.Add(member))
                        return Task.CompletedTask;
                }

                return Components.TryValidateAsync(validator, member, cancellationToken, metadata);
            }
            finally
            {
                lock (_validatingMembers)
                {
                    _validatingMembers.Remove(member);
                }
            }
        }
    }
}