#region Copyright

// ****************************************************************************
// <copyright file="ObserverProvider.cs">
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

using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class ObserverProvider : IObserverProvider
    {
        #region Implementation of IObserverProvider

        public virtual IObserver Observe(object target, IBindingPath path, bool ignoreAttachedMembers, IDataContext context)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(path, nameof(path));
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
