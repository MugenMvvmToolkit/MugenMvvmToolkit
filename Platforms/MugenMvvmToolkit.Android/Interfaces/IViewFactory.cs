#region Copyright

// ****************************************************************************
// <copyright file="IViewFactory.cs">
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
using Android.Content;
using Android.Util;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IViewFactory
    {
        /// <summary>
        ///     Creates an instance of <see cref="ViewResult" /> using the view name.
        /// </summary>
        [NotNull]
        ViewResult Create([NotNull] string name, [NotNull] Context context, [NotNull] IAttributeSet attrs);

        /// <summary>
        ///     Creates an instance of <see cref="ViewResult" /> using the view type.
        /// </summary>
        [NotNull]
        ViewResult Create([NotNull] Type type, [NotNull] Context context, [NotNull] IAttributeSet attrs);
    }
}