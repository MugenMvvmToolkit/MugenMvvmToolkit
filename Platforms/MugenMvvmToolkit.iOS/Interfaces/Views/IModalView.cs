#region Copyright

// ****************************************************************************
// <copyright file="IModalView.cs">
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

using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.iOS.Interfaces.Views
{
    public interface IModalView : IView
    {
    }

    public interface IModalNavSupportView : IModalView
    {
    }

    public interface ISupportActivationModalView : IModalView
    {
        bool Activate();
    }

    public interface ITabView : IView
    {
    }
}
