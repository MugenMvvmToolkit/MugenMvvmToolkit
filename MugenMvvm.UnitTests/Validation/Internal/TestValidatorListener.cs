﻿using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidatorListener : IValidatorListener, IHasPriority
    {
        public Action<IValidator, object?, string, IReadOnlyMetadataContext?>? OnErrorsChanged { get; set; }

        public Action<IValidator, object?, string, Task, IReadOnlyMetadataContext?>? OnAsyncValidation { get; set; }

        public Action<IValidator>? OnDisposed { get; set; }

        public int Priority { get; set; }

        void IValidatorListener.OnErrorsChanged(IValidator validator, object? target, string memberName, IReadOnlyMetadataContext? metadata) =>
            OnErrorsChanged?.Invoke(validator, target, memberName, metadata);

        void IValidatorListener.OnAsyncValidation(IValidator validator, object? target, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata) =>
            OnAsyncValidation?.Invoke(validator, target, memberName, validationTask, metadata);

        void IValidatorListener.OnDisposed(IValidator validator) => OnDisposed?.Invoke(validator);
    }
}