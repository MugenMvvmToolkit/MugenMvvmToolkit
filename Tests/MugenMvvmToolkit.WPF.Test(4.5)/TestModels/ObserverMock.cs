using System;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class ObserverMock : DisposableObject, IObserver
    {
        #region Properties

        public Action Update { get; set; }

        public IBindingPathMembers PathMembers { get; set; }

        public Func<bool, IBindingPathMembers> GetPathMembers { get; set; }

        public Func<bool, bool> IsValid { get; set; }

        public Func<bool, object> GetActualSource { get; set; }

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

        #region Implementation of IObserver

        /// <summary>
        ///     Gets the path.
        /// </summary>
        public IBindingPath Path { get; set; }

        public bool IsTrackChangesEnabled { get; set; }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        IBindingPathMembers IObserver.GetPathMembers(bool throwOnError)
        {
            if (GetPathMembers != null)
                return GetPathMembers(throwOnError);
            if (PathMembers == null)
                return UnsetBindingPathMembers.Instance;
            return PathMembers;
        }

        public event EventHandler<IObserver, ValueChangedEventArgs> ValueChanged;

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        void IObserver.Update()
        {
            Update();
        }

        /// <summary>
        ///     Determines whether the current source is valid.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid; false to return false.
        /// </param>
        /// <returns>
        ///     If <c>true</c> current source is valid, otherwise <c>false</c>.
        /// </returns>
        bool IObserver.Validate(bool throwOnError)
        {
            return IsValid(throwOnError);
        }

        /// <summary>
        ///     Gets the actual source object.
        /// </summary>
        object IObserver.GetActualSource(bool throwOnError)
        {
            return GetActualSource(throwOnError);
        }

        #endregion
    }
}