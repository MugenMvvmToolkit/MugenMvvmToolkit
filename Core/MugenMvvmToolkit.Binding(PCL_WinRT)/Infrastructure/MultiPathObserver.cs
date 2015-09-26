#region Copyright

// ****************************************************************************
// <copyright file="MultiPathObserver.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public sealed class MultiPathObserver : ObserverBase, IEventListener, IHasWeakReference
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

            public bool IsAlive
            {
                get { return Reference.Target != null; }
            }

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                if (ReferenceEquals(Reference, Empty.WeakReference))
                    return false;
                var observer = (MultiPathObserver)Reference.Target;
                if (observer == null)
                {
                    Reference = Empty.WeakReference;
                    var subscriber = Observer;
                    Observer = null;
                    if (subscriber != null)
                        subscriber.Dispose();
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
            private readonly WeakReference _penultimateValueRef;

            #endregion

            #region Constructors

            public MultiBindingPathMembers(WeakReference observerReference, object penultimateValue, IList<IBindingMemberInfo> members)
            {
                _observerRef = observerReference;
                _penultimateValueRef = ToolkitExtensions.GetWeakReference(penultimateValue);
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

            public bool AllMembersAvailable
            {
                get { return _observerRef.Target != null && _penultimateValueRef.Target != null; }
            }

            public IList<IBindingMemberInfo> Members
            {
                get { return _members; }
            }

            public IBindingMemberInfo LastMember
            {
                get { return _lastMember; }
            }

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

            public object PenultimateValue
            {
                get { return _penultimateValueRef.Target; }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool _ignoreAttachedMembers;
        private readonly List<IDisposable> _listeners;
        private readonly LastMemberListener _lastMemberListener;

        #endregion

        #region Constructors

        public MultiPathObserver([NotNull] object source, [NotNull] IBindingPath path, bool ignoreAttachedMembers)
            : base(source, path)
        {
            Should.BeSupported(!path.IsEmpty, "The MultiPathObserver doesn't support the empty path members.");
            _listeners = new List<IDisposable>(path.Parts.Count - 1);
            _ignoreAttachedMembers = ignoreAttachedMembers;
            _lastMemberListener = new LastMemberListener(ServiceProvider.WeakReferenceFactory(this));
        }

        #endregion

        #region Overrides of ObserverBase

        protected override bool DependsOnSubscribers
        {
            get { return false; }
        }

        protected override IBindingPathMembers UpdateInternal(IBindingPathMembers oldPath, bool hasSubscribers)
        {
            object source = GetActualSource();
            if (source == null || source.IsUnsetValue())
            {
                return UnsetBindingPathMembers.Instance;
            }
            bool allMembersAvailable = true;
            IBindingMemberProvider memberProvider = BindingServiceProvider.MemberProvider;
            IList<string> items = Path.Parts;

            //Trying to get member using full path with dot, example BindingErrorProvider.Errors or ErrorProvider.Errors.
            if (items.Count == 2)
            {
                var pathMember = memberProvider.GetBindingMember(source.GetType(), Path.Path, _ignoreAttachedMembers, false);
                if (pathMember != null)
                {
                    var observer = TryObserveMember(source, pathMember, true);
                    if (observer != null)
                        _listeners.Add(observer);
                    return new MultiBindingPathMembers(_lastMemberListener.Reference, source, new[] { pathMember });
                }
            }


            int lastIndex = items.Count - 1;
            var members = new List<IBindingMemberInfo>();
            for (int index = 0; index < items.Count; index++)
            {
                string name = items[index];
                IBindingMemberInfo pathMember = memberProvider
                    .GetBindingMember(source.GetType(), name, _ignoreAttachedMembers, true);
                members.Add(pathMember);
                var observer = TryObserveMember(source, pathMember, index == lastIndex);
                if (observer != null)
                    _listeners.Add(observer);
                if (index == lastIndex)
                    break;
                source = pathMember.GetValue(source, null);
                if (source == null || source.IsUnsetValue())
                {
                    allMembersAvailable = false;
                    break;
                }
            }

            return allMembersAvailable
                ? new MultiBindingPathMembers(_lastMemberListener.Reference, source, members)
                : UnsetBindingPathMembers.Instance;
        }

        protected override IEventListener CreateSourceListener()
        {
            return this;
        }

        protected override void ClearObserversInternal()
        {
            for (int index = 0; index < _listeners.Count; index++)
                _listeners[index].Dispose();
            _listeners.Clear();
        }

        #endregion

        #region Methods

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

        bool IEventListener.IsWeak
        {
            get { return false; }
        }

        bool IEventListener.TryHandle(object sender, object message)
        {
            Update();
            return true;
        }

        WeakReference IHasWeakReference.WeakReference
        {
            get { return _lastMemberListener.Reference; }
        }

        #endregion
    }
}
