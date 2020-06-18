using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Build;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Build
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindingBuilderFrom<TTarget, TSource>
        where TTarget : class
        where TSource : class
    {
        #region Fields

        internal readonly object PathOrExpression;

        #endregion

        #region Constructors

        public BindingBuilderFrom(object pathOrExpression)
        {
            Should.NotBeNull(pathOrExpression, nameof(pathOrExpression));
            PathOrExpression = pathOrExpression;
        }

        #endregion

        #region Methods

        public BindingBuilderTo<TTarget, TSource> To(string path)
        {
            return To<TSource>(path);
        }

        public BindingBuilderTo<TTarget, T> To<T>(string path) where T : class
        {
            return new BindingBuilderTo<TTarget, T>(new BindingBuilderFrom<TTarget, T>(PathOrExpression), path, default);
        }

        public BindingBuilderTo<TTarget, TSource> To(IExpressionNode expression)
        {
            return To<TSource>(expression);
        }

        public BindingBuilderTo<TTarget, T> To<T>(IExpressionNode expression) where T : class
        {
            return new BindingBuilderTo<TTarget, T>(new BindingBuilderFrom<TTarget, T>(PathOrExpression), expression, default);
        }

        public BindingBuilderTo<TTarget, TSource> To(Expression<Func<IBindingBuilderContext<TTarget, TSource>, object>> expression)
        {
            return To<TSource>(expression);
        }

        public BindingBuilderTo<TTarget, T> To<T>(Expression<Func<IBindingBuilderContext<TTarget, T>, object>> expression) where T : class
        {
            return new BindingBuilderTo<TTarget, T>(new BindingBuilderFrom<TTarget, T>(PathOrExpression), expression, default);
        }

        public BindingBuilderTo<TTarget, TSource> To(Expression<Action<IBindingBuilderContext<TTarget, TSource>>> expression)
        {
            return To<TSource>(expression);
        }

        public BindingBuilderTo<TTarget, T> To<T>(Expression<Action<IBindingBuilderContext<TTarget, T>>> expression) where T : class
        {
            return new BindingBuilderTo<TTarget, T>(new BindingBuilderFrom<TTarget, T>(PathOrExpression), expression, default);
        }

        #endregion
    }
}