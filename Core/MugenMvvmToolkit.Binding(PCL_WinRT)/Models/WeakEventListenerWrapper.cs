using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the weak event listener container.
    /// </summary>
    public struct WeakEventListenerWrapper
    {
        #region Fields

        /// <summary>
        ///     Gets the empty instance of <see cref="WeakEventListenerWrapper" />.
        /// </summary>
        public static readonly WeakEventListenerWrapper Empty;

        private readonly object _item;

        #endregion

        #region Constructors

        static WeakEventListenerWrapper()
        {
            Empty = default(WeakEventListenerWrapper);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WeakEventListenerWrapper" /> class, referencing the specified
        ///     listener.
        /// </summary>
        public WeakEventListenerWrapper(IEventListener listener)
        {
            if (listener.IsWeak)
                _item = listener;
            else
                _item = ToolkitExtensions.GetWeakReference(listener);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the value that indicates that struct is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _item == null; }
        }

        /// <summary>
        ///     Gets the undelying object.
        /// </summary>
        public object Source
        {
            get { return _item; }
        }

        /// <summary>
        ///     Gets the current event listener, if any.
        /// </summary>
        [NotNull]
        public IEventListener EventListener
        {
            get
            {
                var listener = _item as IEventListener;
                if (listener != null)
                    return listener;
                if (_item == null)
                    return BindingExtensions.EmptyListener;
                return ((WeakReference)_item).Target as IEventListener ?? BindingExtensions.EmptyListener;
            }
        }

        #endregion
    }
}