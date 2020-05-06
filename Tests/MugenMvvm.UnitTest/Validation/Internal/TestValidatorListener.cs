using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestValidatorListener : IValidatorListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IValidator, string, IReadOnlyMetadataContext?>? OnErrorsChanged { get; set; }

        public Action<IValidator, string, Task, IReadOnlyMetadataContext?>? OnAsyncValidation { get; set; }

        public Action<IValidator>? OnDisposed { get; set; }

        #endregion

        #region Implementation of interfaces

        void IValidatorListener.OnErrorsChanged(IValidator validator, string memberName, IReadOnlyMetadataContext? metadata)
        {
            OnErrorsChanged?.Invoke(validator, memberName, metadata);
        }

        void IValidatorListener.OnAsyncValidation(IValidator validator, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            OnAsyncValidation?.Invoke(validator, memberName, validationTask, metadata);
        }

        void IValidatorListener.OnDisposed(IValidator validator)
        {
            OnDisposed?.Invoke(validator);
        }

        #endregion
    }
}