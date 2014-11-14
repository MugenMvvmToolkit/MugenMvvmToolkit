#region Copyright
// ****************************************************************************
// <copyright file="SinglePathObserver.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the observer that uses the single path member to observe.
    /// </summary>
    public sealed class SinglePathObserver : ObserverBase, IEventListener
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
                get { return _reference.Target != null; }
            }

            /// <summary>
            ///     Gets the available members.
            /// </summary>
            public IList<IBindingMemberInfo> Members
            {
                //NOTE Much more better each time to create a new array than to keep it in memory.
                get { return new[] { _lastMember }; }
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
                get { return _reference.Target; }
            }

            /// <summary>
            ///     Gets the penultimate value.
            /// </summary>
            public object PenultimateValue
            {
                get { return _reference.Target; }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool _ignoreAttachedMembers;
        private IDisposable _weakEventListener;
        private IBindingPathMembers _pathMembers;

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
            _pathMembers = UnsetBindingPathMembers.Instance;
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
                if (_weakEventListener != null)
                    _weakEventListener.Dispose();
                object source = GetActualSource();
                if (source == null || source.IsUnsetValue())
                    _pathMembers = UnsetBindingPathMembers.Instance;
                else
                {
                    IBindingMemberInfo lastMember = BindingServiceProvider
                        .MemberProvider
                        .GetBindingMember(source.GetType(), Path.Path, _ignoreAttachedMembers, true);
                    _pathMembers = new SingleBindingPathMembers(OriginalSource as WeakReference ?? ToolkitExtensions.GetWeakReference(source), Path, lastMember);
                    _weakEventListener = TryObserveMember(source, lastMember, this, Path.Path);
                }
            }
            catch (Exception)
            {
                _pathMembers = UnsetBindingPathMembers.Instance;
                throw;
            }
        }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        protected override IBindingPathMembers GetPathMembersInternal()
        {
            return _pathMembers;
        }

        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected override void OnDispose()
        {
            var listener = _weakEventListener;
            if (listener != null)
                listener.Dispose();
            base.OnDispose();
        }

        #endregion

        #region Implementation of IEventListener

        bool IEventListener.IsAlive
        {
            get { return IsAlive; }
        }

        bool IEventListener.IsWeak
        {
            get { return false; }
        }

        bool IEventListener.TryHandle(object sender, object message)
        {
            RaiseValueChanged(ValueChangedEventArgs.TrueEventArgs);
            return true;
        }

        #endregion
    }
}