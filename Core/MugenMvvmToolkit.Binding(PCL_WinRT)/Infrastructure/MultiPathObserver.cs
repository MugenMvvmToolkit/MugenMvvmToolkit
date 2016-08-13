#region Copyright

// ****************************************************************************
// <copyright file="MultiPathObserver.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public sealed class MultiPathObserver : ObserverBase, IEventListener, IHasWeakReferenceInternal
    {
        #region Nested types

        private sealed class LastMemberListener : IEventListener
        {
            #region Fields

            public WeakReference Reference;
            public IDisposable Observer;

            #endregion

            #region Constructors

            public LastMemberListener(WeakReference reference)
            {
                Reference = reference;
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsAlive => Reference.Target != null;

            public bool IsWeak => true;

            public bool TryHandle(object sender, object message)
            {
                if (ReferenceEquals(Reference, Empty.WeakReference))
                    return false;
                var observer = (MultiPathObserver)Reference.Target;
                if (observer == null)
                {
                    Reference = Empty.WeakReference;
                    Observer?.Dispose();
                    Observer = null;
                    return false;
                }
                observer.RaiseValueChanged(ValueChangedEventArgs.TrueEventArgs);
                return true;
            }

            #endregion
        }

        private sealed class MultiBindingPathMembers : IBindingPathMembers
        {
            #region Fields

            private readonly IBindingMemberInfo _lastMember;
            private readonly IList<IBindingMemberInfo> _members;
            private readonly WeakReference _observerRef;
            public WeakReference PenultimateValueRef;

            #endregion

            #region Constructors

            public MultiBindingPathMembers(WeakReference observerReference, object penultimateValue, IList<IBindingMemberInfo> members)
            {
                PenultimateValueRef = ToolkitExtensions.GetWeakReference(penultimateValue);
                _observerRef = observerReference;
                _members = members;
                _lastMember = _members[_members.Count - 1];
            }

            #endregion

            #region Implementation of IBindingPathMembers

            public IBindingPath Path
            {
                get
                {
                    var observer = (ObserverBase)_observerRef.Target;
                    if (observer == null)
                        return BindingPath.None;
                    return observer.Path;
                }
            }

            public bool AllMembersAvailable => _observerRef.Target != null && PenultimateValueRef.Target != null;

            public IList<IBindingMemberInfo> Members => _members;

            public IBindingMemberInfo LastMember => _lastMember;

            public object Source
            {
                get
                {
                    var observer = (ObserverBase)_observerRef.Target;
                    if (observer == null)
                        return null;
                    return observer.GetActualSource();
                }
            }

            public object PenultimateValue => PenultimateValueRef.Target;

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool _ignoreAttachedMembers;
        private readonly bool _hasStablePath;
        private readonly bool _observable;
        private readonly bool _optional;
        private readonly List<IDisposable> _listeners;
        private readonly LastMemberListener _lastMemberListener;

        #endregion

        #region Constructors

        public MultiPathObserver([NotNull] object source, [NotNull] IBindingPath path, bool ignoreAttachedMembers, bool hasStablePath, bool observable, bool optional)
            : base(source, path)
        {
            Should.BeSupported(!path.IsEmpty, "The MultiPathObserver doesn't support the empty path members.");
            _listeners = new List<IDisposable>(path.Parts.Count - 1);
            _ignoreAttachedMembers = ignoreAttachedMembers;
            _lastMemberListener = new LastMemberListener(ServiceProvider.WeakReferenceFactory(this));
            _hasStablePath = hasStablePath;
            _observable = observable;
            _optional = optional;
        }

        #endregion

        #region Overrides of ObserverBase

        protected override IBindingPathMembers UpdateInternal(IBindingPathMembers oldPath, bool hasSubscribers)
        {
            object source = GetActualSource();
            if (source == null || source.IsUnsetValue())
                return UnsetBindingPathMembers.Instance;
            ClearListeners();
            int lastIndex;
            if (_hasStablePath)
            {
                var pathMembers = oldPath as MultiBindingPathMembers;
                if (pathMembers != null)
                {
                    var list = pathMembers.Members;
                    lastIndex = list.Count - 1;
                    for (int index = 0; index < list.Count; index++)
                    {
                        var pathMember = list[index];
                        if (_observable)
                        {
                            var observer = TryObserveMember(source, pathMember, index == lastIndex);
                            if (observer != null)
                                _listeners.Add(observer);
                        }
                        if (index == lastIndex)
                            break;
                        source = pathMember.GetValue(source, null);
                        if (source == null || source.IsUnsetValue())
                        {
                            if (Path.IsDebuggable)
                                DebugInfo($"Value is not available for '{pathMember.Path}'", new[] { GetActualSource(false) });
                            return UnsetBindingPathMembers.Instance;
                        }
                    }
                    pathMembers.PenultimateValueRef = ToolkitExtensions.GetWeakReference(source);
                    return pathMembers;
                }
            }

            IList<string> items = Path.Parts;
            lastIndex = items.Count - 1;
            var members = new List<IBindingMemberInfo>();
            for (int index = 0; index < items.Count; index++)
            {
                IBindingMemberInfo pathMember = GetBindingMember(source.GetType(), items[index], _ignoreAttachedMembers, _optional);
                if (pathMember == null)
                    return UnsetBindingPathMembers.Instance;
                members.Add(pathMember);
                if (_observable)
                {
                    var observer = TryObserveMember(source, pathMember, index == lastIndex);
                    if (observer != null)
                        _listeners.Add(observer);
                }
                if (index == lastIndex)
                    break;
                source = pathMember.GetValue(source, null);
                if (source == null || source.IsUnsetValue())
                {
                    if (Path.IsDebuggable)
                        DebugInfo($"Value is not available for '{pathMember.Path}'", new[] { GetActualSource(false) });
                    return UnsetBindingPathMembers.Instance;
                }
            }

            return new MultiBindingPathMembers(_lastMemberListener.Reference, source, members);
        }

        protected override IEventListener CreateSourceListener()
        {
            return this;
        }

        protected override void OnDispose()
        {
            try
            {
                _lastMemberListener.Reference.Target = null;
                ClearListeners();
            }
            catch
            {
                ;
            }
            base.OnDispose();
        }

        #endregion

        #region Methods

        private void ClearListeners()
        {
            if (_listeners.Count == 0)
                return;
            for (int index = 0; index < _listeners.Count; index++)
                _listeners[index].Dispose();
            _listeners.Clear();
        }

        private IDisposable TryObserveMember(object source, IBindingMemberInfo pathMember, bool isLastInChain)
        {
            if (source == null)
                return null;
            if (isLastInChain)
            {
                var observer = TryObserveMember(source, pathMember, _lastMemberListener, pathMember.Path);
                _lastMemberListener.Observer = observer;
                return observer;
            }
            return TryObserveMember(source, pathMember, this, pathMember.Path);
        }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.IsWeak => false;

        bool IEventListener.TryHandle(object sender, object message)
        {
            Update();
            return true;
        }

        WeakReference IHasWeakReference.WeakReference => _lastMemberListener.Reference;

        #endregion
    }
}
