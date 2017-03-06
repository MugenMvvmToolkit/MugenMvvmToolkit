#region Copyright

// ****************************************************************************
// <copyright file="ObserverProvider.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using System.Collections.Generic;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class ObserverProvider : IObserverProvider
    {
        #region Nested types

        private sealed class DebuggableBindingPathWrapper : IBindingPath
        {
            #region Fields

            private readonly IBindingPath _path;

            #endregion

            #region Constructors

            public DebuggableBindingPathWrapper(IBindingPath path, string tag)
            {
                _path = path;
                DebugTag = tag;
            }

            #endregion

            #region Properties

            public string Path => _path.Path;

            public IList<string> Parts => _path.Parts;

            public bool IsEmpty => _path.IsEmpty;

            public bool IsSingle => _path.IsSingle;

            public bool IsDebuggable => true;

            public string DebugTag { get; }

            #endregion

            #region Methods

            public override string ToString()
            {
                return Path;
            }

            #endregion
        }

        #endregion

        #region Implementation of interfaces

        public virtual IObserver Observe(object target, IBindingPath path, bool ignoreAttachedMembers, IDataContext context)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(path, nameof(path));

            string tag;
            if (context != null && context.TryGetData(BindingBuilderConstants.DebugTag, out tag))
                path = new DebuggableBindingPathWrapper(path, tag);

            if (path.IsEmpty)
                return new EmptyPathObserver(target, path);

            bool hasStablePath;
            bool observable;
            bool optional;
            if (context == null || !context.TryGetData(BindingBuilderConstants.HasStablePath, out hasStablePath))
                hasStablePath = BindingServiceProvider.HasStablePathDefault;
            if (context == null || !context.TryGetData(BindingBuilderConstants.Observable, out observable))
                observable = BindingServiceProvider.ObservablePathDefault;
            if (context == null || !context.TryGetData(BindingBuilderConstants.Optional, out optional))
                optional = BindingServiceProvider.OptionalBindingDefault;
            if (path.IsSingle)
                return new SinglePathObserver(target, path, ignoreAttachedMembers, hasStablePath, observable, optional);
            return new MultiPathObserver(target, path, ignoreAttachedMembers, hasStablePath, observable, optional);
        }

        #endregion
    }
}