#region Copyright
// ****************************************************************************
// <copyright file="EmptyPathObserver.cs">
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

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the observer that uses the empty path member to observe.
    /// </summary>
    public sealed class EmptyPathObserver : ObserverBase
    {
        #region Nested types

        private sealed class EmptyBindingPathMembers : IBindingPathMembers
        {
            #region Fields

            private readonly WeakReference _reference;

            #endregion

            #region Constructors

            public EmptyBindingPathMembers(EmptyPathObserver observer)
            {
                _reference = ServiceProvider.WeakReferenceFactory(observer, true);
            }

            #endregion

            #region Implementation of IBindingPathMembers

            /// <summary>
            ///     Gets the <see cref="IBindingPath" />.
            /// </summary>
            public IBindingPath Path
            {
                get { return BindingPath.Empty; }
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
                get { return new[] { LastMember }; }
            }

            /// <summary>
            ///     Gets the last value, if all members is available; otherwise returns the empty value.
            /// </summary>
            public IBindingMemberInfo LastMember
            {
                get { return BindingMemberInfo.Empty; }
            }

            /// <summary>
            ///     Gets the source value.
            /// </summary>
            public object Source
            {
                get
                {
                    var observer = (ObserverBase)_reference.Target;
                    if (observer == null)
                        return null;
                    return observer.GetActualSource();
                }
            }

            /// <summary>
            ///     Gets the penultimate value.
            /// </summary>
            public object PenultimateValue
            {
                get { return Source; }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IBindingPathMembers _members;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmptyPathObserver" /> class.
        /// </summary>
        public EmptyPathObserver([NotNull] object source, [NotNull] IBindingPath path)
            : base(source, path)
        {
            Should.BeSupported(path.IsEmpty, "The EmptyPathObserver supports only empty path members.");
            _members = new EmptyBindingPathMembers(this);
        }

        #endregion

        #region Overrides of ObserverBase

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        protected override void UpdateInternal()
        {
        }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        protected override IBindingPathMembers GetPathMembersInternal()
        {
            return _members;
        }

        #endregion
    }
}