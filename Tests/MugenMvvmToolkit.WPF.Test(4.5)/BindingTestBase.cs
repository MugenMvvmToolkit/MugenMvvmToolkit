using System;
using System.Linq.Expressions;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test
{
    public abstract class BindingTestBase : TestBase
    {
        #region Properties

        protected IDataContext EmptyContext
        {
            get { return DataContext.Empty; }
        }

        #endregion

        #region Methods

        protected static string GetMemberPath<T>(T target, Expression<Func<T, object>> expression)
        {
            return GetMemberPath(expression);
        }

        protected static string GetMemberPath<T>(Expression<Func<T, object>> expression)
        {
            return BindingExtensions.GetMemberPath(expression);
        }

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            BindingServiceProvider.SetDefaultValues();
            base.OnInit();
            ThreadManager.ImmediateInvokeAsync = true;
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            ThreadManager.ImmediateInvokeOnUiThread = true;
        }

        #endregion
    }
}