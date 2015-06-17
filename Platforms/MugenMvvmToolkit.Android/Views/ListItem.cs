#region Copyright

// ****************************************************************************
// <copyright file="ListItem.cs">
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
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MugenMvvmToolkit.Android.Binding.Infrastructure;

namespace MugenMvvmToolkit.Android.Views
{
    [Register("mugenmvvmtoolkit.android.views.ListItem")]
    public class ListItem : FrameLayout, ICheckable
    {
        #region Fields

        private bool _checked;
        private readonly int _templateId;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ListItem" /> class.
        /// </summary>
        protected ListItem(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ListItem" /> class.
        /// </summary>
        public ListItem(int templateId, BindableLayoutInflater inflater)
            : base(inflater.Context)
        {
            _templateId = templateId;
            inflater.Inflate(templateId, this, true);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the id of template.
        /// </summary>
        public int TemplateId
        {
            get { return _templateId; }
        }

        /// <summary>
        ///     Gets the first child.
        /// </summary>
        protected View FirstChild
        {
            get
            {
                if (ChildCount == 0)
                    return null;
                View firstChild = GetChildAt(0);
                return firstChild;
            }
        }

        #endregion

        #region Implementation of ICheckable

        public virtual void Toggle()
        {
            var contentCheckable = FirstChild as ICheckable;
            if (contentCheckable == null)
                _checked = !_checked;
            else
                contentCheckable.Toggle();
        }

        public virtual bool Checked
        {
            get
            {
                var contentCheckable = FirstChild as ICheckable;
                if (contentCheckable == null)
                    return _checked;

                return contentCheckable.Checked;
            }
            set
            {
                var contentCheckable = FirstChild as ICheckable;
                if (contentCheckable == null)
                {
                    _checked = value;
                    TrySetActivated(FirstChild, value);
                }
                else
                    contentCheckable.Checked = value;
            }
        }

        #endregion

        #region Methods

        private static void TrySetActivated(View view, bool value)
        {
            if (PlatformExtensions.IsApiGreaterThan10 && view.IsAlive())
                view.Activated = value;
        }

        #endregion
    }
}