#region Copyright
// ****************************************************************************
// <copyright file="BindingSource.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Sources
{
    /// <summary>
    ///     Represents the observable binding source.
    /// </summary>
    public class BindingSource : IBindingSource, IHandler<ValueChangedEventArgs>
    {
        #region Fields

        private readonly IObserver _observer;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingSource" /> class.
        /// </summary>
        public BindingSource([NotNull] IObserver observer)
        {
            Should.NotBeNull(observer, "observer");
            _observer = observer;
            _observer.Listener = this;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current observer.
        /// </summary>
        [NotNull]
        protected IObserver Observer
        {
            get { return _observer; }
        }

        #endregion

        #region Implementation of IBindingSource

        /// <summary>
        ///     Gets the path.
        /// </summary>
        public IBindingPath Path
        {
            get { return _observer.Path; }
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
        public bool Validate(bool throwOnError)
        {
            return _observer.Validate(throwOnError);
        }

        /// <summary>
        ///     Gets the source object.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid; false to return false.
        /// </param>
        public object GetSource(bool throwOnError)
        {
            return _observer.GetActualSource(throwOnError);
        }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid.
        /// </param>
        public IBindingPathMembers GetPathMembers(bool throwOnError)
        {
            return _observer.GetPathMembers(throwOnError);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _observer.Dispose();
            _observer.Listener = null;
            ValueChanged = null;
        }

        /// <summary>
        ///     Occurs when value changed.
        /// </summary>
        public event EventHandler<IBindingSource, ValueChangedEventArgs> ValueChanged;

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        void IHandler<ValueChangedEventArgs>.Handle(object sender, ValueChangedEventArgs message)
        {
            RaiseValueChanged(message);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Raises the <see cref="ValueChanged" /> event.
        /// </summary>
        protected virtual void RaiseValueChanged(ValueChangedEventArgs args)
        {
            var handler = ValueChanged;
            if (handler != null)
                handler(this, args);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}, Member: {1}, IsValid: {2}", Path, GetPathMembers(false).LastMember, Validate(false).ToString());
        }

        #endregion
    }
}