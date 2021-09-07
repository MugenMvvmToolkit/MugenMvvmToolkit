using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Build;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Bindings.Parsing.Expressions;

namespace MugenMvvm.Bindings.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindingBuilderTarget<TTarget, TSource>
        where TTarget : class
        where TSource : class
    {
        [BindingMacros(MacrosConstant.Target)]
        public BindableMembersTargetDescriptor<TTarget> TargetMembers => default;

        [BindingMacros(MacrosConstant.Source)]
        public BindableMembersTargetDescriptor<TSource> SourceMembers => default;

        public BindingBuilderFrom<TTarget, TSource> For(string path) => new(path);

        public BindingBuilderFrom<TTarget, TSource> For(IExpressionNode expression) => new(expression);

        public BindingBuilderFrom<TTarget, TSource> For<TValue>(Expression<Func<TTarget, TValue>> expression) => new(expression);

        public BindingBuilderTo<TTarget, TSource> Action(Expression<Func<IBindingBuilderContext<TTarget, TSource>, object?>> expression) => Action<TSource>(expression);

        public BindingBuilderTo<TTarget, T> Action<T>(Expression<Func<IBindingBuilderContext<TTarget, T>, object?>> expression)
            where T : class =>
            new(new BindingBuilderFrom<TTarget, T>(UnaryExpressionNode.ActionMacros), expression, default);

        public BindingBuilderTo<TTarget, TSource> Action(Expression<Action<IBindingBuilderContext<TTarget, TSource>>> expression) => Action<TSource>(expression);

        public BindingBuilderTo<TTarget, T> Action<T>(Expression<Action<IBindingBuilderContext<TTarget, T>>> expression)
            where T : class =>
            new(new BindingBuilderFrom<TTarget, T>(UnaryExpressionNode.ActionMacros), expression, default);
    }
}