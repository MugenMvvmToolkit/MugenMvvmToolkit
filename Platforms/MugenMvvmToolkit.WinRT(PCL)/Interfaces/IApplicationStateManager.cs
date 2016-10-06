#region Copyright

// ****************************************************************************
// <copyright file="IApplicationStateManager.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using Windows.UI.Xaml;

namespace MugenMvvmToolkit.WinRT.Interfaces
{
    public interface IApplicationStateManager
    {
        IList<Type> KnownTypes { get; }

        void OnSaveState([NotNull] FrameworkElement element, [NotNull] IDictionary<string, object> state, object args,
            IDataContext context = null);

        void OnLoadState([NotNull] FrameworkElement element, [NotNull] IDictionary<string, object> state, object args,
            IDataContext context = null);
    }
}
