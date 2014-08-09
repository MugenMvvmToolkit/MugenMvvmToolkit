using System;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class BindingSourceMock : DisposableObject, IBindingSource
    {
        #region Properties

        public Func<bool, bool> IsValid { get; set; }

        public Func<bool, object> GetSource { get; set; }

        public Func<bool, IBindingPathMembers> GetPathMembers { get; set; }

        #endregion

        #region Implementation of IBindingSource

        /// <summary>
        ///     Gets the path.
        /// </summary>
        public IBindingPath Path { get; set; }

        /// <summary>
        ///     Determines whether the current source is valid.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid; false to return false.
        /// </param>
        /// <returns>
        ///     If <c>true</c> current source is valid, otherwise <c>false</c>.
        /// </returns>
        public bool Validate(bool throwOnError)
        {
            return IsValid(throwOnError);
        }

        /// <summary>
        ///     Gets the source object.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid.
        /// </param>
        object IBindingSource.GetSource(bool throwOnError)
        {
            return GetSource(throwOnError);
        }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid.
        /// </param>
        IBindingPathMembers IBindingSource.GetPathMembers(bool throwOnError)
        {
            if (GetPathMembers == null)
                return UnsetBindingPathMembers.Instance;
            return GetPathMembers(throwOnError);
        }

        public event EventHandler<IBindingSource, ValueChangedEventArgs> ValueChanged;

        #endregion

        #region Methods

        public void RaiseValueChanged(ValueChangedEventArgs args = null)
        {
            if (args == null)
                args = ValueChangedEventArgs.FalseEventArgs;
            var handler = ValueChanged;
            if (handler != null) handler(this, args);
        }

        #endregion
    }
}