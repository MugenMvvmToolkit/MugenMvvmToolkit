#region Copyright

// ****************************************************************************
// <copyright file="BindingContextManager.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class BindingContextManager : IBindingContextManager
    {
        #region Nested types

        private sealed class BindingContext : IBindingContext, IEventListener
        {
            #region Fields

            private readonly WeakReference _srcRef;
            private bool _isParentContext;
            private object _dataContext;

            #endregion

            #region Constructors

            public BindingContext(object target)
            {
                _isParentContext = true;
                _srcRef = ServiceProvider.WeakReferenceFactory(target);
                var parentMember = BindingServiceProvider.VisualTreeManager.GetParentMember(target.GetType());
                if (parentMember != null)
                    parentMember.TryObserve(target, this);
                TryHandle(null, null);
            }

            #endregion

            #region Implementation of interfaces

            public object Source => _srcRef.Target;

            public object Value
            {
                get
                {
                    var dc = _dataContext;
                    var bc = dc as IBindingContext;
                    if (bc == null)
                        return dc;
                    return bc.Value;
                }
                set
                {
                    bool isUnsetValue = value.IsUnsetValue();
                    lock (_srcRef)
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
                            _isParentContext = false;
                            ClearOldContext();
                            if (Equals(_dataContext, value))
                                return;
                            _dataContext = value;
                        }
                    }
                    RaiseValueChanged();
                }
            }

            public bool IsAlive => true;

            public bool IsWeak => true;

            public bool TryHandle(object sender, object message)
            {
                if (!(sender is IBindingContext))
                {
                    lock (_srcRef)
                        UpdateContextInternal();
                }
                RaiseValueChanged();
                return true;
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion

            #region Methods

            private static IBindingContext GetParentBindingContext(object target)
            {
                object parent = BindingServiceProvider.VisualTreeManager.FindParent(target);
                if (parent == null)
                    return null;
                return BindingServiceProvider.ContextManager.GetBindingContext(parent);
            }

            private void UpdateContextInternal()
            {
                if (!_isParentContext)
                    return;
                ClearOldContext();
                object target = _srcRef.Target;
                if (target == null)
                    return;
                var context = GetParentBindingContext(target);
                if (context == null)
                    _dataContext = null;
                else
                {
                    WeakEventManager.AddBindingContextListener(context, this, false);
                    _dataContext = context;
                }
            }

            private void ClearOldContext()
            {
                var bindingContext = _dataContext as IBindingContext;
                if (bindingContext != null)
                    WeakEventManager.RemoveBindingContextListener(bindingContext, this);
            }

            private void RaiseValueChanged()
            {
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            #endregion
        }

        private sealed class BindingContextSource : IBindingContext
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
                _observer = BindingServiceProvider
                    .ObserverProvider
                    .Observe(source, BindingServiceProvider.BindingPathFactory(member.Path), true);
                _observer.ValueChanged += ObserverOnValueChanged;
            }

            #endregion

            #region Implementation of interfaces

            public object Source => _observer.Source;

            public bool IsAlive => true;

            public object Value
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
                    if (ReferenceEquals(Value, value))
                        return;
                    object target = _observer.Source;
                    if (target == null)
                        return;
                    _isInvoked = false;
                    if (value.IsUnsetValue())
                        value = null;
                    _member.SetSingleValue(target, value);
                    if (!_isInvoked)
                        OnDataContextChanged(null);
                }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion

            #region Methods

            private void ObserverOnValueChanged(IObserver sender, ValueChangedEventArgs args)
            {
                OnDataContextChanged(args);
            }

            private void OnDataContextChanged(ValueChangedEventArgs message)
            {
                _isInvoked = true;
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, message);
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ContextMemberPath = "$$ContextManager." + AttachedMemberConstants.DataContext;
        private static readonly Func<object, object, IBindingContext> CreateBindingContextDelegate;

        #endregion

        #region Constructors

        static BindingContextManager()
        {
            CreateBindingContextDelegate = CreateBindingContext;
        }

        #endregion

        #region Properties

        [NotNull]
        protected virtual IBindingContext CreateBindingContext([NotNull] object item)
        {
            IBindingMemberInfo member = GetExplicitDataContextMember(item);
            if (member == null)
                return new BindingContext(item);
            return new BindingContextSource(item, member);
        }

        protected virtual IBindingMemberInfo GetExplicitDataContextMember(object source)
        {
            IBindingMemberInfo member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(source.GetType(), AttachedMemberConstants.DataContext, true, false);
            if (member != null && member.Type.Equals(typeof(object)) && member.CanRead && member.CanWrite)
                return member;
            return null;
        }

        #endregion

        #region Methods

        private static IBindingContext CreateBindingContext(object item, object state)
        {
            return ((BindingContextManager)state).CreateBindingContext(item);
        }

        #endregion

        #region Implementation of IBindingContextManager

        public bool HasBindingContext(object item)
        {
            Should.NotBeNull(item, nameof(item));
            if (item is IBindingContext || item is IBindingContextHolder)
                return true;
            return ServiceProvider.AttachedValueProvider.Contains(item, ContextMemberPath);
        }

        public IBindingContext GetBindingContext(object item)
        {
            Should.NotBeNull(item, nameof(item));
            var holder = item as IBindingContextHolder;
            if (holder != null)
                return holder.BindingContext;
            var context = item as IBindingContext;
            if (context != null)
                return context;
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(item, ContextMemberPath, CreateBindingContextDelegate, this);
        }

        #endregion
    }
}
