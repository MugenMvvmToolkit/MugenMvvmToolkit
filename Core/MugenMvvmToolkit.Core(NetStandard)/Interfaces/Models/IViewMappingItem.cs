#region Copyright

// ****************************************************************************
// <copyright file="IViewMappingItem.cs">
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

using System;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface IViewMappingItem
    {
        [CanBeNull]
        string Name { get; }

        [NotNull]
        Type ViewType { get; }

        [NotNull]
        Type ViewModelType { get; }

        [NotNull]
        Uri Uri { get; }
    }
}
