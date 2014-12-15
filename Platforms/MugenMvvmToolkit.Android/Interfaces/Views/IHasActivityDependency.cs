#region Copyright
// ****************************************************************************
// <copyright file="IHasActivityDependency.cs">
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
using Android.App;

namespace MugenMvvmToolkit.Interfaces.Views
{
    public interface IHasActivityDependency
    {
        void OnAttached(Activity activity);
    }
}