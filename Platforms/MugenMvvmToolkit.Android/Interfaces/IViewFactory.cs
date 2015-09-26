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
using MugenMvvmToolkit.Android.Models;

namespace MugenMvvmToolkit.Android.Interfaces
{
    public interface IViewFactory
    {
        ViewResult Create([NotNull] string name, [NotNull] Context context, [NotNull] IAttributeSet attrs);

        ViewResult Create([NotNull] Type type, [NotNull] Context context, [NotNull] IAttributeSet attrs);
    }
}
