#region Copyright

// ****************************************************************************
// <copyright file="SinglePathObserver.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the observer that uses the single path member to observe.
    /// </summary>
    public sealed class SinglePathObserver : ObserverBase, IEventListener, IHasWeakReference
    {
        #region Nested types

        private sealed class SingleBindingPathMembers : IBindingPathMembers
        {
            #region Fields

            private readonly IBindingMemberInfo _lastMember;
            private readonly IBindingPath _path;
            private readonly WeakReference _reference;

            #endregion

            #region Constructors

            public SingleBindingPathMembers(WeakReference source, IBindingPath path, IBindingMemberInfo lastMember)
            {
                _reference = source;
                _lastMember = lastMember;
                _path = path;
            }

            #endregion

            #region Implementation of IBindingPathMembers

            public IBindingPath Path
            {
                get { return _path; }
            }

            public bool AllMembersAvailable
            {
                get { return _reference.Target != null; }
            }

            public IList<IBindingMemberInfo> Members
            {
                //NOTE it's better each time to create a new array than to keep it in memory, because this property is rarely used.
                get { return new[] { _lastMember }; }
            }

            public IBindingMemberInfo LastMember
            {
                get { return _lastMember; }
            }

            public object Source
            {
                get { return _reference.Target; }
            }

            public object PenultimateValue
            {
                get { return _reference.Target; }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool _ignoreAttachedMembers;
        private readonly WeakReference _ref;
        private IDisposable _weakEventListener;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SinglePathObserver" /> class.
        /// </summary>
        public SinglePathObserver([NotNull] object source, [NotNull] IBindingPath path, bool ignoreAttachedMembers)
            : base(source, path)
        {
            Should.BeSupported(path.IsSingle, "The SinglePathObserver supports only single path members.");
            _ignoreAttachedMembers = ignoreAttachedMembers;
            _ref = ServiceProvider.WeakReferenceFactory(this);
        }

        #endregion

        #region Overrides of ObserverBase

        /// <summary>
        ///     Indicates that current observer dependes on <see cref="ObserverBase.ValueChanged" /> subscribers.
        /// </summary>
        protected override bool DependsOnSubscribers
        {
            get { return OriginalSource is WeakReference; }
        }

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        protected override IBindingPathMembers UpdateInternal(bool hasSubscribers)
        {
            object source = GetActualSource();
            if (source == null || source.IsUnsetValue())
                return UnsetBindingPathMembers.Instance;

            IBindingMemberInfo lastMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(source.GetType(), Path.Path, _ignoreAttachedMembers, true);
            var srcRef = OriginalSource as WeakReference;
            if (hasSubscribers || srcRef == null)
                _weakEventListener = TryObserveMember(source, lastMember, this, Path.Path);
            return new SingleBindingPathMembers(srcRef ?? ToolkitExtensions.GetWeakReference(source), Path, lastMember);
        }

        /// <summary>
        ///     Releases the current observers.
        /// </summary>
        protected override void ClearObserversInternal()
        {
            var listener = _weakEventListener;
            _weakEventListener = null;
            if (listener != null)
                listener.Dispose();
        }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.IsWeak
        {
            get { return false; }
        }

        bool IEventListener.TryHandle(object sender, object message)
        {
            RaiseValueChanged(ValueChangedEventArgs.TrueEventArgs);
            return true;
        }

        WeakReference IHasWeakReference.WeakReference
        {
            get { return _ref; }
        }

        #endregion
    }
}