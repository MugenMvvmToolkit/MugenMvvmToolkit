using System;
using System.Linq.Expressions;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Silverlight.Binding.Modules;
using MugenMvvmToolkit.UWP.Binding.Modules;
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
            base.OnInit();
            if (ValueConverterEx == null)
            {
                //to invoke static constructor.
                new PlatformDataBindingModule();
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
