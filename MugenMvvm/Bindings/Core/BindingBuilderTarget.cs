﻿using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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
        #region Properties

        public BindableMembersDescriptor<TTarget> TargetMembers => default;

        public BindableMembersDescriptor<TSource> SourceMembers => default;

        #endregion

        #region Methods

        public BindingBuilderFrom<TTarget, TSource> For(string path) => new BindingBuilderFrom<TTarget, TSource>(path);

        public BindingBuilderFrom<TTarget, TSource> For(IExpressionNode expression) => new BindingBuilderFrom<TTarget, TSource>(expression);

        public BindingBuilderFrom<TTarget, TSource> For<TValue>(Expression<Func<TTarget, TValue>> expression) => new BindingBuilderFrom<TTarget, TSource>(expression);

        public BindingBuilderTo<TTarget, TSource> Action(Expression<Func<IBindingBuilderContext<TTarget, TSource>, object?>> expression) => Action<TSource>(expression);

        public BindingBuilderTo<TTarget, T> Action<T>(Expression<Func<IBindingBuilderContext<TTarget, T>, object?>> expression)
            where T : class =>
            new BindingBuilderTo<TTarget, T>(new BindingBuilderFrom<TTarget, T>(MemberExpressionNode.Action), expression, default);

        public BindingBuilderTo<TTarget, TSource> Action(Expression<Action<IBindingBuilderContext<TTarget, TSource>>> expression) => Action<TSource>(expression);

        public BindingBuilderTo<TTarget, T> Action<T>(Expression<Action<IBindingBuilderContext<TTarget, T>>> expression)
            where T : class =>
            new BindingBuilderTo<TTarget, T>(new BindingBuilderFrom<TTarget, T>(MemberExpressionNode.Action), expression, default);

        #endregion
    }
}