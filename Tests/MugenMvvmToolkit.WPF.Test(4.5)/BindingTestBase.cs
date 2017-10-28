#region Copyright

// ****************************************************************************
// <copyright file="BindingTestBase.cs">
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

using System;
using System.Linq.Expressions;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Silverlight.Binding.Modules;
using MugenMvvmToolkit.UWP.Binding.Modules;
using MugenMvvmToolkit.WPF.Binding;
using MugenMvvmToolkit.UWP.Binding;
using MugenMvvmToolkit.WPF.Binding.Modules;

namespace MugenMvvmToolkit.Test
{
    public abstract class BindingTestBase : TestBase
    {
        #region Fields

        protected static Func<IBindingMemberInfo, Type, object, object> ValueConverterEx;

        #endregion

        #region Properties

        protected IDataContext EmptyContext => DataContext.Empty;

        #endregion

        #region Methods

        protected static string GetMemberPath<T>(T target, Expression<Func<T, object>> expression)
        {
            return GetMemberPath(expression);
        }

        protected static string GetMemberPath<T>(Expression<Func<T, object>> expression)
        {
            return BindingExtensions.GetMemberPath(() => expression);
        }

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            BindingServiceProvider.SetDefaultValues();
            BindingServiceProvider.MemberProvider.Register(AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.AsErrorsSource,
                (info, o) => BindingConstants.ErrorsSourceValue, null));
            BindingServiceProvider.ValueConverter = BindingConverterExtensions.Convert;
            base.OnInit();
            if (ValueConverterEx == null)
            {
                //to invoke static constructor.
#if NETFX_CORE
                new UwpDataBindingModule();
#else
                new WpfDataBindingModule();
#endif
                ValueConverterEx = BindingServiceProvider.ValueConverter;
            }
            else
                BindingServiceProvider.ValueConverter = ValueConverterEx;
            ThreadManager.ImmediateInvokeAsync = true;
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            ThreadManager.ImmediateInvokeOnUiThread = true;
        }

        #endregion
    }
}
