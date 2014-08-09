#region Copyright
// ****************************************************************************
// <copyright file="MultiPathObserver.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the observer that uses the several path members to observe.
    /// </summary>
    public class MultiPathObserver : ObserverBase, IEventListener, IHasSelfWeakReference
    {
        #region Nested types

        private sealed class LastMemberListener : IEventListener
        {
            #region Fields

            private readonly MultiPathObserver _multiPathObserver;

            #endregion

            #region Constructors

            public LastMemberListener(MultiPathObserver multiPathObserver)
            {
                _multiPathObserver = multiPathObserver;
            }

            #endregion

            #region Implementation of IEventListener

            public void Handle(object sender, object message)
            {
                _multiPathObserver.RaiseValueChanged(ValueChangedEventArgs.TrueEventArgs);
            }

            #endregion
        }

        private sealed class MultiBindingPathMembers : IBindingPathMembers
        {
            #region Fields

            private readonly IBindingMemberInfo _lastMember;
            private readonly IList<IBindingMemberInfo> _members;
            private readonly WeakReference _observerRef;
            private readonly IBindingPath _path;
            private readonly WeakReference _penultimateValueRef;

            #endregion

            #region Constructors

            public MultiBindingPathMembers(WeakReference observerReference, object penultimateValue, IBindingPath path,
                IList<IBindingMemberInfo> members)
            {
                _observerRef = observerReference;
                _penultimateValueRef = MvvmExtensions.GetWeakReference(penultimateValue);
                _path = path;
                _members = members;
                _lastMember = _members[_members.Count - 1];
            }

            #endregion

            #region Implementation of IBindingPathMembers

            /// <summary>
            ///     Gets the <see cref="IBindingPath" />.
            /// </summary>
            public IBindingPath Path
            {
                get { return _path; }
            }

            /// <summary>
            ///     Gets the value that indicates that all members are available, if <c>true</c>.
            /// </summary>
            public bool AllMembersAvailable
            {
                get { return _observerRef.Target != null && _penultimateValueRef.Target != null; }
            }

            /// <summary>
            ///     Gets the available members.
            /// </summary>
            public IList<IBindingMemberInfo> Members
            {
                get { return _members; }
            }

            /// <summary>
            ///     Gets the last value, if all members is available; otherwise returns the empty value.
            /// </summary>
            public IBindingMemberInfo LastMember
            {
                get { return _lastMember; }
            }

            /// <summary>
            ///     Gets the source value.
            /// </summary>
            public object Source
            {
                get
                {
                    var observer = (ObserverBase)_observerRef.Target;
                    if (observer == null)
                        return null;
                    object result;
                    observer.GetActualSource(out result);
                    return result;
                }
            }

            /// <summary>
            ///     Gets the penultimate value.
            /// </summary>
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

        private IBindingPathMembers _members;
        private readonly WeakReference _selfReference;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultiPathObserver" /> class.
        /// </summary>
        public MultiPathObserver([NotNull] object source, [NotNull] IBindingPath path, bool ignoreAttachedMembers)
            : base(source, path)
        {
            Should.BeSupported(!path.IsEmpty, "The MultiPathObserver doesn't support the empty path members.");
            _listeners = new List<IDisposable>();
            _lastMemberListener = new LastMemberListener(this);
            _ignoreAttachedMembers = ignoreAttachedMembers;
            _members = UnsetBindingPathMembers.Instance;
            _selfReference = ServiceProvider.WeakReferenceFactory(this, true);
            Update();
        }

        #endregion

        #region Overrides of ObserverBase

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        protected override void UpdateInternal()
        {
            try
            {
                ClearListeners();
                object source;
                if (!GetActualSource(out source) || source == null)
                {
                    _members = UnsetBindingPathMembers.Instance;
                    return;
                }
                bool allMembersAvailable = true;
                IBindingMemberProvider memberProvider = BindingProvider.Instance.MemberProvider;
                IList<string> items = Path.Parts;
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

                _members = allMembersAvailable
                    ? new MultiBindingPathMembers(_selfReference, source, Path, members)
                    : UnsetBindingPathMembers.Instance;
            }
            catch (Exception)
            {
                _members = UnsetBindingPathMembers.Instance;
                throw;
            }
        }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        protected override IBindingPathMembers GetPathMembersInternal()
        {
            return _members;
        }

        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected override void OnDispose()
        {
            ClearListeners();
            base.OnDispose();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Tries to observer the specified binding member.
        /// </summary>
        protected virtual IDisposable TryObserveMember(object source, IBindingMemberInfo pathMember, bool isLastInChain)
        {
            if (source == null)
                return null;
            if (isLastInChain)
                return TryAddEventHandler(source, pathMember, _lastMemberListener, pathMember.Path);
            return TryAddEventHandler(source, pathMember, this, pathMember.Path);
        }

        /// <summary>
        ///     Clears all listeners.
        /// </summary>
        protected void ClearListeners()
        {
            for (int index = 0; index < _listeners.Count; index++)
                _listeners[index].Dispose();
            _listeners.Clear();
        }

        #endregion

        #region Implementation of interfaces

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        void IEventListener.Handle(object sender, object message)
        {
            Update();
        }

        /// <summary>
        ///     Gets the self weak reference.
        /// </summary>
        WeakReference IHasSelfWeakReference.SelfReference
        {
            get { return _selfReference; }
        }

        #endregion
    }
}