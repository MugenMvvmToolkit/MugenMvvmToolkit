#region Copyright

// ****************************************************************************
// <copyright file="IViewControllerView.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.iOS.Interfaces.Mediators;

namespace MugenMvvmToolkit.iOS.Interfaces.Views
{
    public interface IViewControllerView : IView
    {
        [NotNull]
        IMvvmViewControllerMediator Mediator { get; }
    }
}
