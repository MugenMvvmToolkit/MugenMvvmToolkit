#region Copyright
// ****************************************************************************
// <copyright file="IModalView.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
namespace MugenMvvmToolkit.Interfaces.Views
{
    public interface IModalView : IView
    {
    }

    public interface IModalNavSupportView : IModalView
    {
    }

    public interface ITabView : IView
    {
    }
}