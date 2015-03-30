#region Copyright

// ****************************************************************************
// <copyright file="UserControl.cs">
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
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace MugenMvvmToolkit.Views
{
    public abstract class UserControl : FrameLayout
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserControl" /> class.
        /// </summary>
        protected UserControl(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserControl" /> class.
        /// </summary>
        protected UserControl(Context context)
            : base(context)
        {
            Initialize(context);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserControl" /> class.
        /// </summary>
        protected UserControl(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize(context);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserControl" /> class.
        /// </summary>
        protected UserControl(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Initialize(context);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the layout id of current control.
        /// </summary>
        protected abstract int LayoutId { get; }

        #endregion

        #region Methods

        private void Initialize(Context context)
        {
            var tuple = context.GetActivity().LayoutInflater.CreateBindableView(LayoutId);
            AddView(tuple.Item1);
        }

        #endregion
    }
}