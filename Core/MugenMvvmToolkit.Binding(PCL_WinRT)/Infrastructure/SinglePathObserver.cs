#region Copyright

// ****************************************************************************
// <copyright file="SinglePathObserver.cs">
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
    public sealed class SinglePathObserver : ObserverBase, IEventListener, IHasWeakReferenceInternal
    {
        #region Nested types

        private sealed class SingleBindingPathMembers : IBindingPathMembers
        {
            #region Fields

            private readonly IBindingMemberInfo _lastMember;
            private readonly IBindingPath _path;
            public WeakReference Reference;

            #endregion

            #region Constructors

            public SingleBindingPathMembers(WeakReference source, IBindingPath path, IBindingMemberInfo lastMember)
            {
                Reference = source;
                _lastMember = lastMember;
                _path = path;
            }

            #endregion

            #region Implementation of IBindingPathMembers

            public IBindingPath Path => _path;

            public bool AllMembersAvailable => Reference.Target != null;

            //NOTE it's better each time to create a new array than keep it in memory, because this property is rarely used.
            public IList<IBindingMemberInfo> Members => new[] { _lastMember };

            public IBindingMemberInfo LastMember => _lastMember;

            public object Source => Reference.Target;

            public object PenultimateValue => Reference.Target;

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool _ignoreAttachedMembers;
        private readonly bool _hasStablePath;
        private readonly bool _observable;
        private readonly bool _optional;
        private readonly WeakReference _ref;
        private IDisposable _weakEventListener;

        #endregion

        #region Constructors

        public SinglePathObserver([NotNull] object source, [NotNull] IBindingPath path, bool ignoreAttachedMembers, bool hasStablePath, bool observable, bool optional)
            : base(source, path)
        {
            Should.BeSupported(path.IsSingle, "The SinglePathObserver supports only single path members.");
            _ignoreAttachedMembers = ignoreAttachedMembers;
            _ref = ServiceProvider.WeakReferenceFactory(this);
            _hasStablePath = hasStablePath;
            _observable = observable;
            _optional = optional;
        }

        #endregion

        #region Overrides of ObserverBase

        protected override bool DependsOnSubscribers => true;

        protected override IBindingPathMembers UpdateInternal(IBindingPathMembers oldPath, bool hasSubscribers)
        {
            object source = GetActualSource();
            if (source == null || source.IsUnsetValue())
                return UnsetBindingPathMembers.Instance;
            var members = oldPath as SingleBindingPathMembers;
            var srcRef = OriginalSource as WeakReference;
            if (members != null)
            {
                if (srcRef != null || ReferenceEquals(members.Source, source))
                {
                    if (hasSubscribers && _observable && _weakEventListener == null)
                        _weakEventListener = TryObserveMember(source, members.LastMember, this, Path.Path);
                    return members;
                }
                if (_hasStablePath)
                {
                    members.Reference = ToolkitExtensions.GetWeakReference(source);
                    if (hasSubscribers && _observable)
                    {
                        _weakEventListener?.Dispose();
                        _weakEventListener = TryObserveMember(source, members.LastMember, this, Path.Path);
                    }
                    return members;
                }
            }
            IBindingMemberInfo lastMember = GetBindingMember(source.GetType(), Path.Path, _ignoreAttachedMembers, _optional);
            if (lastMember == null)
                return UnsetBindingPathMembers.Instance;

            if (_observable && hasSubscribers)
            {
                _weakEventListener?.Dispose();
                _weakEventListener = TryObserveMember(source, lastMember, this, Path.Path);
            }
            return new SingleBindingPathMembers(srcRef ?? ToolkitExtensions.GetWeakReference(source), Path, lastMember);
        }

        protected override void OnDispose()
        {
            try
            {
                _ref.Target = null;
                _weakEventListener?.Dispose();
                _weakEventListener = null;
            }
            catch
            {
                ;
            }
            base.OnDispose();
        }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.IsWeak => false;

        bool IEventListener.TryHandle(object sender, object message)
        {
            RaiseValueChanged(ValueChangedEventArgs.TrueEventArgs);
            return true;
        }

        WeakReference IHasWeakReference.WeakReference => _ref;

        #endregion
    }
}
