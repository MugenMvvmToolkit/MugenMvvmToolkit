using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Tests.Validation
{
    public class TestValidatorErrorsChangedListener : IValidatorErrorsChangedListener, IHasPriority
    {
        public Action<IValidator, ItemOrIReadOnlyList<string>, IReadOnlyMetadataContext?>? OnErrorsChanged { get; set; }

        public int Priority { get; set; }

        void IValidatorErrorsChangedListener.OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata) =>
            OnErrorsChanged?.Invoke(validator, members, metadata);
    }
}