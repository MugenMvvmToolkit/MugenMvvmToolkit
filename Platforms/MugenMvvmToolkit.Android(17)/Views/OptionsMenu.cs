#region Copyright
// ****************************************************************************
// <copyright file="OptionsMenu.cs">
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
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Views
{
    public sealed class OptionsMenu : FrameLayout, IManualBindings
    {
        #region Fields

        private IList<string> _bindings;
        private IMenu _currentMenu;

        #endregion

        #region Constructors

        public OptionsMenu(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            SetMinimumWidth(0);
            SetMinimumHeight(0);
            base.Visibility = ViewStates.Gone;
            base.Id = Resource.Id.OptionsMenu;
        }

        #endregion

        #region Methods

        public void Inflate(Activity activity, IMenu menu)
        {
            _currentMenu = menu;
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
            return EmptyValue<IDataBinding>.ListInstance;
        }

        #endregion

        #region Overrides of View

        public override int Id
        {
            get { return Resource.Id.OptionsMenu; }
            set { }
        }

        public override ViewStates Visibility
        {
            get { return ViewStates.Gone; }
            set { }
        }

        #endregion
    }
}