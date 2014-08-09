using System;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class ObserverMock : DisposableObject, IObserver
    {
        #region Properties

        public Action Update { get; set; }

        public IBindingPathMembers PathMembers { get; set; }

        public Func<bool, bool> Validate { get; set; }

        public Func<bool, object> GetActualSource { get; set; }

        #endregion

        #region Implementation of IObserver

        /// <summary>
        ///     Gets or sets the value changed listener.
        /// </summary>
        public IHandler<ValueChangedEventArgs> Listener { get; set; }

        /// <summary>
        ///     Gets the path.
        /// </summary>
        public IBindingPath Path { get; set; }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        public IBindingPathMembers GetPathMembers(bool throwOnError)
        {
            if (PathMembers == null)
                return UnsetBindingPathMembers.Instance;
            return PathMembers;
        }

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
            return Validate(throwOnError);
        }

        /// <summary>
        ///     Gets the actual source object.
        /// </summary>
        object IObserver.GetActualSource(bool throwOnError)
        {
            return GetActualSource(throwOnError);
        }

        #endregion

        #region Methods

        public void RaiseValueChanged(ValueChangedEventArgs args = null)
        {
            if (args == null)
                args = ValueChangedEventArgs.FalseEventArgs;
            IHandler<ValueChangedEventArgs> handler = Listener;
            if (handler != null) handler.Handle(this, args);
        }

        #endregion
    }
}