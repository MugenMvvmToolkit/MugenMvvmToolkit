using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Build;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Bindings.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindingBuilderFrom<TTarget, TSource>
        where TTarget : class
        where TSource : class
    {
        internal readonly object PathOrExpression;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderFrom(object pathOrExpression)
        {
            Should.NotBeNull(pathOrExpression, nameof(pathOrExpression));
            PathOrExpression = pathOrExpression;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo<TTarget, TSource> To(string path) => To<TSource>(path);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo<TTarget, T> To<T>(string path) where T : class => new(new BindingBuilderFrom<TTarget, T>(PathOrExpression), path, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo<TTarget, TSource> To(IExpressionNode expression) => To<TSource>(expression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo<TTarget, T> To<T>(IExpressionNode expression) where T : class => new(new BindingBuilderFrom<TTarget, T>(PathOrExpression), expression, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo<TTarget, TSource> To(Expression<Func<IBindingBuilderContext<TTarget, TSource>, object?>> expression) => To<TSource>(expression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo<TTarget, T> To<T>(Expression<Func<IBindingBuilderContext<TTarget, T>, object?>> expression) where T : class =>
            new(new BindingBuilderFrom<TTarget, T>(PathOrExpression), expression, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo<TTarget, TSource> To(Expression<Action<IBindingBuilderContext<TTarget, TSource>>> expression) => To<TSource>(expression);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo<TTarget, T> To<T>(Expression<Action<IBindingBuilderContext<TTarget, T>>> expression) where T : class =>
            new(new BindingBuilderFrom<TTarget, T>(PathOrExpression), expression, default);
    }
}