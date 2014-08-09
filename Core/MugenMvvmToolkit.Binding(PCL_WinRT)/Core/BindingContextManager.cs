#region Copyright
// ****************************************************************************
// <copyright file="BindingContextManager.cs">
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
using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Core
{
    /// <summary>
    ///     Represents the binding context manager.
    /// </summary>
    public class BindingContextManager : IBindingContextManager
    {
        #region Nested types

        private sealed class BindingContext : IBindingContext, IEventListener
        {
            #region Fields

            private readonly WeakReference _targetReference;
            private bool _isParentContext;
            private object _dataContext;
            //NOTE to keep observer reference.
// ReSharper disable once NotAccessedField.Local
            private IDisposable _parentListener;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="BindingContextManager" /> class.
            /// </summary>
            public BindingContext(object target)
            {
                _isParentContext = true;
                _targetReference = MvvmExtensions.GetWeakReference(target);
                _parentListener = BindingProvider.Instance.ObserverProvider.TryObserveParent(target, this);
                Handle(null, null);
            }

            #endregion

            #region Implementation of IBindingContext

            /// <summary>
            ///     Gets the source object.
            /// </summary>
            public object Source
            {
                get { return _targetReference.Target; }
            }

            /// <summary>
            ///     Gets the data context.
            /// </summary>
            public object DataContext
            {
                get
                {
                    var dc = _dataContext;
                    var bc = dc as IBindingContext;
                    if (bc == null)
                        return dc;
                    return bc.DataContext;
                }
                set
                {
                    bool isUnsetValue = value.IsUnsetValue();
                    lock (_targetReference)
                    {
                        if (isUnsetValue)
                        {
                            if (_isParentContext)
                                return;
                            _isParentContext = true;
                            UpdateContextInternal();
                        }
                        else
                        {
                            if (ReferenceEquals(_dataContext, value))
                                return;
                            _isParentContext = false;
                            ClearOldContext();
                            _dataContext = value;
                        }
                    }
                    RaiseDataContextChanged(null, EventArgs.Empty);
                }
            }

            /// <summary>
            ///     Occurs when the DataContext property changed.
            /// </summary>
            public event EventHandler<IBindingContext, EventArgs> DataContextChanged;

            #endregion

            #region Methods

            private static IBindingContext GetParentBindingContext(object target)
            {
                object parent = BindingProvider.Instance.VisualTreeManager.FindParent(target);
                if (parent == null)
                    return null;
                return BindingProvider.Instance.ContextManager.GetBindingContext(parent);
            }

            private void UpdateContextInternal()
            {
                if (!_isParentContext)
                    return;
                ClearOldContext();
                object target = _targetReference.Target;
                if (target == null)
                    return;
                var context = GetParentBindingContext(target);
                if (context == null)
                    _dataContext = null;
                else
                {
                    context.DataContextChanged += RaiseDataContextChanged;
                    _dataContext = context;
                }
            }

            private void ClearOldContext()
            {
                var bindingContext = _dataContext as IBindingContext;
                if (bindingContext != null)
                    bindingContext.DataContextChanged -= RaiseDataContextChanged;
            }

            private void RaiseDataContextChanged(object sender, EventArgs eventArgs)
            {
                var handler = DataContextChanged;
                if (handler != null)
                    handler(this, eventArgs);
            }

            #endregion

            #region Implementation of IEventListener

            /// <summary>
            ///     Handles the message.
            /// </summary>
            /// <param name="sender">The object that raised the event.</param>
            /// <param name="message">Information about event.</param>
            public void Handle(object sender, object message)
            {
                lock (_targetReference)
                    UpdateContextInternal();
                RaiseDataContextChanged(null, EventArgs.Empty);
            }

            #endregion
        }

        private sealed class BindingContextSource : IBindingContext, IHandler<ValueChangedEventArgs>
        {
            #region Fields

            private readonly IBindingMemberInfo _member;
            private readonly IObserver _observer;
            private bool _isInvoked;

            #endregion

            #region Constructors

            public BindingContextSource(object source, IBindingMemberInfo member)
            {
                _member = member;
                _observer = BindingProvider.Instance
                                           .ObserverProvider
                                           .Observe(source, BindingPath.Create(member.Path), true);
                _observer.Listener = this;
            }

            #endregion

            #region Implementation of IBindingContext

            /// <summary>
            ///     Gets the source object.
            /// </summary>
            public object Source
            {
                get { return _observer.Source; }
            }

            /// <summary>
            ///     Gets the data context.
            /// </summary>
            public object DataContext
            {
                get
                {
                    object target = _observer.Source;
                    if (target == null)
                        return null;
                    return _member.GetValue(target, null);
                }
                set
                {
                    if (ReferenceEquals(DataContext, value))
                        return;
                    object target = _observer.Source;
                    if (target == null)
                        return;
                    _isInvoked = false;
                    if (value.IsUnsetValue())
                        value = null;
                    _member.SetValue(target, new[] { value });
                    if (!_isInvoked)
                        OnDataContextChanged(null);
                }
            }

            /// <summary>
            ///     Handles the message.
            /// </summary>
            /// <param name="sender">The object that raised the event.</param>
            /// <param name="message">Information about event.</param>
            void IHandler<ValueChangedEventArgs>.Handle(object sender, ValueChangedEventArgs message)
            {
                OnDataContextChanged(message);
            }

            /// <summary>
            ///     Occurs when the DataContext property changed.
            /// </summary>
            public event EventHandler<IBindingContext, EventArgs> DataContextChanged;

            #endregion

            #region Methods

            private void OnDataContextChanged(ValueChangedEventArgs message)
            {
                _isInvoked = true;
                var handler = DataContextChanged;
                if (handler != null)
                    handler(this, message);
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ContextMemberPath =
            "$$BindingContextManager" + AttachedMemberConstants.DataContext;

        private readonly Func<object, object, IBindingContext> _getBindingContext;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberType" /> class.
        /// </summary>
        public BindingContextManager()
        {
            _getBindingContext = (o, o1) => CreateBindingContext(o);
        }

        #endregion

        #region Implementation of IBindingContextManager

        /// <summary>
        ///     Gets the binding context for the specified item.
        /// </summary>
        public IBindingContext GetBindingContext(object item)
        {
            Should.NotBeNull(item, "item");
            var context = item as IBindingContext;
            if (context != null)
                return context;
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(item, ContextMemberPath, _getBindingContext, null);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Creates an instance of <see cref="IBindingContext" /> for the specified item.
        /// </summary>
        /// <returns>An instnace of <see cref="IBindingContext" />.</returns>
        [NotNull]
        protected virtual IBindingContext CreateBindingContext([NotNull] object item)
        {
            IBindingMemberInfo member = GetExplicitDataContext(item);
            if (member == null)
                return new BindingContext(item);
            return new BindingContextSource(item, member);
        }

        /// <summary>
        ///     Tries to get explicit data context member.
        /// </summary>
        protected static IBindingMemberInfo GetExplicitDataContext(object source)
        {
            IBindingMemberInfo member = BindingProvider
                .Instance
                .MemberProvider
                .GetBindingMember(source.GetType(), AttachedMemberConstants.DataContext, true, false);
            if (member != null && member.Type.Equals(typeof(object)) && member.CanRead && member.CanWrite)
                return member;
            return null;
        }

        #endregion
    }
}