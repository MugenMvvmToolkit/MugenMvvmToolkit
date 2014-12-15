#region Copyright
// ****************************************************************************
// <copyright file="IBindableMenuInflater.cs">
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

using Android.Views;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IBindableMenuInflater
    {
        /// <summary>
        ///     Gets or sets underlying menu inflater, if any.
        /// </summary>
        [CanBeNull]
        MenuInflater MenuInflater { get; set; }

        /// <summary>
        ///     Inflate a menu hierarchy from the specified XML resource.
        /// </summary>
        void Inflate(int menuRes, IMenu menu, object parent);
    }
}