﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation.Components
{
    public sealed class InlineValidatorComponent : ValidatorComponentBase<object>
    {
        #region Constructors

        public InlineValidatorComponent(object target) : base(target)
        {
        }

        #endregion

        #region Methods

        protected override CancellationToken GetCancellationToken(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => cancellationToken;

        public void SetErrors(string memberName, ItemOrIReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            UpdateErrors(memberName, errors.GetRawValue(), false, metadata);
        }

        protected override ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => default;

        #endregion
    }
}