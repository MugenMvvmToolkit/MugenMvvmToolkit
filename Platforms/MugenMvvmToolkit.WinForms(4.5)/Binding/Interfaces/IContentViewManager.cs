﻿#region Copyright

// ****************************************************************************
// <copyright file="IContentViewManager.cs">
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

#if WINFORMS
namespace MugenMvvmToolkit.WinForms.Binding.Interfaces
#elif ANDROID
namespace MugenMvvmToolkit.Android.Binding.Interfaces
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Binding.Interfaces
#endif

{
    /// <summary>
    ///     Represents the interface that allows to set content in the specified view.
    /// </summary>
    public interface IContentViewManager
    {
#if ANDROID
        /// <summary>
        ///     Sets the specified content.
        /// </summary>
        bool SetContent([NotNull] object view, [CanBeNull] object content);
#else
        /// <summary>
        ///     Sets the specified content.
        /// </summary>
        void SetContent([NotNull] object view, [CanBeNull] object content);
#endif
    }
}