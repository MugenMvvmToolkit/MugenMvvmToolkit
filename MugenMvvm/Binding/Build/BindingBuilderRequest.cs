using System;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Extensions;

namespace MugenMvvm.Binding.Build
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingBuilderRequest
    {
        #region Fields

        public readonly Func<Delegate, BindingExpressionRequest> GetRequestHandler;
        public readonly Delegate OriginalDelegate;

        #endregion

        #region Constructors

        private BindingBuilderRequest(Delegate originalDelegate, Func<Delegate, BindingExpressionRequest> getRequestHandler)
        {
            OriginalDelegate = originalDelegate;
            GetRequestHandler = getRequestHandler;
        }

        #endregion

        #region Properties

        public bool IsEmpty => GetRequestHandler == null;

        #endregion

        #region Methods

        public static BindingBuilderRequest Get<TTarget, TSource>(BindingBuilderDelegate<TTarget, TSource> getBuilder)
            where TTarget : class
            where TSource : class
        {
            Should.NotBeNull(getBuilder, nameof(getBuilder));
            return new BindingBuilderRequest(getBuilder, d => ((BindingBuilderDelegate<TTarget, TSource>)d).Invoke(default));
        }

        public BindingExpressionRequest ToBindingExpressionRequest()
        {
            if (GetRequestHandler == null)
                return default;
            if (OriginalDelegate.HasClosure())
                BindingExceptionManager.ThrowCannotUseExpressionClosure(OriginalDelegate);
            return GetRequestHandler(OriginalDelegate);
        }

        #endregion
    }
}