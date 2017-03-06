#region Copyright

// ****************************************************************************
// <copyright file="ContentViewManagerBase.cs">
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

#if WINFORMS
using MugenMvvmToolkit.WinForms.Binding.Interfaces;

namespace MugenMvvmToolkit.WinForms.Binding.Infrastructure
#elif ANDROID
using MugenMvvmToolkit.Android.Binding.Interfaces;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
#elif TOUCH
using MugenMvvmToolkit.iOS.Binding.Interfaces;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
#endif

{
    public abstract class ContentViewManagerBase<TView, TContent> : IContentViewManager
    {
        #region Implementation of IContentViewManager

#if ANDROID
        bool IContentViewManager.SetContent(object view, object content)
        {
            return SetContent((TView)view, (TContent)content);
        }
#else
        void IContentViewManager.SetContent(object view, object content)
        {
            SetContent((TView)view, (TContent)content);
        }
#endif

        #endregion

        #region Methods

#if ANDROID
        protected abstract bool SetContent(TView view, TContent content);
#else
        protected abstract void SetContent(TView view, TContent content);
#endif

        #endregion
    }
}
