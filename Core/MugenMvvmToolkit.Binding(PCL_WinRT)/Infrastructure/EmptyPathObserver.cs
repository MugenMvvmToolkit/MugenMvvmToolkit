#region Copyright

// ****************************************************************************
// <copyright file="EmptyPathObserver.cs">
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

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public sealed class EmptyPathObserver : ObserverBase
    {
        #region Nested types

        private sealed class EmptyBindingPathMembers : DefaultListener, IBindingPathMembers
        {
            #region Constructors

            public EmptyBindingPathMembers(WeakReference @ref)
                : base(@ref)
            {
            }

            #endregion

            #region Implementation of IBindingPathMembers

            public IBindingPath Path
            {
                get { return BindingPath.Empty; }
            }

            public bool AllMembersAvailable
            {
                get
                {
                    var observer = (ObserverBase)Ref.Target;
                    if (observer == null)
                        return false;
                    return !observer.GetActualSource().IsUnsetValue();
                }
            }

            public IList<IBindingMemberInfo> Members
            {
                get { return new[] { LastMember }; }
            }

            public IBindingMemberInfo LastMember
            {
                get { return BindingMemberInfo.Empty; }
            }

            public object Source
            {
                get
                {
                    var observer = (ObserverBase)Ref.Target;
                    if (observer == null)
                        return null;
                    return observer.GetActualSource();
                }
            }

            public object PenultimateValue
            {
                get { return Source; }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly EmptyBindingPathMembers _members;

        #endregion

        #region Constructors

        public EmptyPathObserver([NotNull] object source, [NotNull] IBindingPath path)
            : base(source, path)
        {
            Should.BeSupported(path.IsEmpty, "The EmptyPathObserver supports only empty path members.");
            _members = new EmptyBindingPathMembers(ServiceProvider.WeakReferenceFactory(this));
        }

        #endregion

        #region Overrides of ObserverBase

        protected override bool DependsOnSubscribers
        {
            get { return true; }
        }

        protected override IBindingPathMembers UpdateInternal(IBindingPathMembers oldPath, bool hasSubscribers)
        {
            return _members;
        }

        protected override void ClearObserversInternal()
        {
        }

        protected override IEventListener CreateSourceListener()
        {
            return _members;
        }

        #endregion
    }
}
