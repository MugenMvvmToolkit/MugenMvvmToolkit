#region Copyright

// ****************************************************************************
// <copyright file="IViewMappingProvider.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IViewMappingProvider
    {
        [NotNull]
        IEnumerable<IViewMappingItem> ViewMappings { get; }

        [NotNull]
        IList<IViewMappingItem> FindMappingsForView([NotNull] Type viewType, bool throwOnError);

        IViewMappingItem FindMappingForViewModel([NotNull] Type viewModelType, [CanBeNull] string viewName,
            bool throwOnError);
    }
}
