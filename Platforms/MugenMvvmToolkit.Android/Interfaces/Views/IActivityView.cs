#region Copyright

// ****************************************************************************
// <copyright file="IActivityView.cs">
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
using Android.App;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.Interfaces.Views
{
    public interface IActivityView : IView
    {
        [NotNull]
        IMvvmActivityMediator Mediator { get; }

        object DataContext { get; set; }

        event EventHandler<Activity, EventArgs> DataContextChanged;
    }
}
