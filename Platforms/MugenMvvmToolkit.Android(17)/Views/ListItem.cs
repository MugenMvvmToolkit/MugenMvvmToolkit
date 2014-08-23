#region Copyright
// ****************************************************************************
// <copyright file="ListItemView.cs">
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
using System;
using System.Collections.Generic;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Views
{
    public class ListItem : FrameLayout, ICheckable
    {
        #region Fields

        private bool _checked;
        private readonly int _templateId;
        private readonly IList<IDataBinding> _bindings;
        private object _dataContext;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ListItem" /> class.
        /// </summary>
        public ListItem(int templateId, LayoutInflater inflater)
            : base(inflater.Context)
        {
            _templateId = templateId;
            _bindings = inflater.CreateBindableView(templateId, this, true).Item2;
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
        /// Gets the bindings.
        /// </summary>
        public IList<IDataBinding> Bindings
        {
            get { return _bindings; }
        }

        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                if (Equals(value, _dataContext))
                    return;
                _dataContext = value;
                var eventHandler = DataContextChanged;
                if (eventHandler != null)
                    eventHandler(this, EventArgs.Empty);                
            }
        }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        public event EventHandler DataContextChanged;

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
#if API17
            view.Activated = value;
#else
            if (view == null)
                return;
            var type = view.GetType();
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(type, "Activated", false, false);
            if (member != null && member.CanWrite)
                member.SetValue(view, new object[] { value });
#endif
        }

        #endregion
    }
}