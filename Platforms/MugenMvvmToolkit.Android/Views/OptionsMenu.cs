#region Copyright

// ****************************************************************************
// <copyright file="OptionsMenu.cs">
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
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Views
{
    [Register("mugenmvvmtoolkit.views.OptionsMenu")]
    public sealed class OptionsMenu : FrameLayout, IManualBindings
    {
        #region Fields

        private IList<string> _bindings;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionsMenu" /> class.
        /// </summary>
        private OptionsMenu(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionsMenu" /> class.
        /// </summary>
        public OptionsMenu(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            SetMinimumWidth(0);
            SetMinimumHeight(0);
            Visibility = ViewStates.Gone;
            Id = Resource.Id.OptionsMenu;
        }

        #endregion

        #region Methods

        public void Inflate(Activity activity, IMenu menu)
        {
            IBindingMemberInfo bindingMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(typeof(OptionsMenu), AttachedMemberNames.MenuTemplate, false, true);
            var value = (int)bindingMember.GetValue(this, null);
            activity.MenuInflater.Inflate(value, menu, this);
            if (_bindings != null)
            {
                foreach (string binding in _bindings)
                    BindingServiceProvider.BindingProvider.CreateBindingsFromString(menu, binding, null);
            }
        }

        #endregion

        #region Implementation of IManualBindings

        public IList<IDataBinding> SetBindings(IList<string> bindings)
        {
            _bindings = bindings;
            return Empty.Array<IDataBinding>();
        }

        #endregion
    }
}